using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Azavea.NijPredictivePolicing.Common;
using System.IO;
using Azavea.NijPredictivePolicing.Common.DB;
using Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats;
using System.Security.Policy;
using System.Data;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.Transfer
{
    public class AcsDataManager
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AcsState State = AcsState.None;

        protected string _stateFips;
        public string StateFIPS
        {
            get
            {
                if (string.IsNullOrEmpty(_stateFips))
                {
                    using (var reader = DbClient.GetCommand("select STATE from geographies limit 1").ExecuteReader())
                    {
                        reader.Read();
                        _stateFips = reader.GetString(0);
                    }                   
                }
                return _stateFips;
            }
            set
            {
                _stateFips = value;
            }
        }

        public string WorkingPath;
        public string ShapePath;
        public string CurrentDataPath;
        public string DBFilename;
        
        
        public IDataClient DbClient;


        public AcsDataManager()
        {
            Init();
        }

        public AcsDataManager(AcsState aState)
        {
            this.State = aState;
            Init();
        }


        public void Init()
        {
            this.WorkingPath = FileLocator.GetStateWorkingDir(this.State);
            
            FileUtilities.PathEnsure(this.WorkingPath, "database");
            this.DBFilename = FileUtilities.PathCombine(this.WorkingPath, "database", Settings.CurrentAcsDirectory + ".sqlite");

            this.ShapePath = FileUtilities.PathEnsure(this.WorkingPath, "shapes");
            this.CurrentDataPath = FileUtilities.PathEnsure(this.WorkingPath, Settings.CurrentAcsDirectory);


            //this.DataPath = FileLocator.GetStateBlockGroupDataDir(this.State);
            //this.ShpPath = FileLocator.GetStateBlockGroupDataDir(this.State);            
            //this.DBPath = FileUtilities.PathCombine(this.DataPath, this.State.ToString() + ".sqlite");

            this.DbClient = DataClient.GetDefaultClient(this.DBFilename);            
        }

        public string GetLocalBlockGroupZipFileName()
        {
            return FileUtilities.PathCombine(this.WorkingPath, FileLocator.GetStateBlockGroupFileName(this.State));
        }

        public string GetLocalGeographyFileName()
        {
            return FileLocator.GetStateBlockGroupGeographyFilename(this.CurrentDataPath);
        }

        public string GetRemoteStateShapefileURL()
        {
            string url = Settings.StateBlockGroupShapefileRootURL + 
                Settings.StateBlockGroupShapefileFormatURL;
            url = url.Replace("{FIPS-code}", this.StateFIPS);
            return url;
        }


        public string GetLocalBlockGroupShapefilename()
        {
            string template = Settings.StateBlockGroupShapefileFormatURL;
            template = template.Replace("{FIPS-code}", this.StateFIPS);

            return Path.Combine(this.WorkingPath, template);
        }

        public string GetLocalColumnMappingsDirectory()
        {
            return FileUtilities.PathCombine(FileLocator.TempPath, Settings.ColumnMappingsFileName);
        }


        /// <summary>
        /// Downloads the DATA FILE
        /// </summary>
        /// <returns></returns>
        public bool CheckBlockGroupFile()
        {
            _log.DebugFormat("Downloading block group file for {0}", this.State);

            string desiredUrl = FileLocator.GetStateBlockGroupUrl(this.State);
            string destFilepath = GetLocalBlockGroupZipFileName();



            if (FileDownloader.GetFileByURL(desiredUrl, destFilepath))
            {
                _log.Debug("Download successful");
                if (FileLocator.ExpandZipFile(destFilepath, this.CurrentDataPath))
                {
                    _log.Debug("State block group file decompressed successfully");
                    return true;
                }
                else
                {
                    _log.Error("Error during decompression, TODO: destroy directory");
                }
            }
            else
            {
                _log.Error("An error was encountered while downloading block group data, exiting.");
            }
            return false;
        }

        public bool CheckColumnMappingsFile()
        {
            _log.DebugFormat("Downloading column mappings file ({0})", Settings.ColumnMappingsFileName);

            string desiredUrl = Settings.CurrentColumnMappingsFileUrl;
            string destFilepath = FileUtilities.PathCombine(FileLocator.TempPath, 
                Settings.ColumnMappingsFileName + Settings.ColumnMappingsFileExtension);

            if (FileDownloader.GetFileByURL(desiredUrl, destFilepath))
            {
                _log.Debug("Download successful");
                if (FileLocator.ExpandZipFile(destFilepath, GetLocalColumnMappingsDirectory()))
                {
                    _log.Debug("Column Mappings file decompressed successfully");
                    return true;
                }
                else
                {
                    _log.Error("Error during decompression, TODO: destroy directory");
                }
            }
            else
            {
                _log.Error("An error was encountered while downloading column mappings file, exiting.");
            }
            return false;
        }

        



        /// <summary>
        /// Downloads the SHAPE FILE, must be run before initializing the database!  
        /// Since this will be imported into the database!
        /// </summary>
        /// <returns></returns>
        public bool CheckShapefile()
        {
             _log.DebugFormat("Downloading shapefile of block groups for {0}", this.State);

            string desiredUrl = this.GetRemoteStateShapefileURL();
            string destFilepath = GetLocalBlockGroupShapefilename();

            if (FileDownloader.GetFileByURL(desiredUrl, destFilepath))
            {
                _log.Debug("Download successful");

                if (FileLocator.ExpandZipFile(destFilepath, this.ShapePath))
                {
                    _log.Debug("State block group file decompressed successfully");


                    var client = DbClient;
                    using (var conn = client.GetConnection())
                    {
                        if (!DataClient.HasTable(conn, client, "shapefile"))
                        {
                            _log.Debug("Shapefile table not found, importing...");
                            CreateShapefileTable(conn, "shapefile");
                        }
                    }





                    return true;
                }
                else
                {
                    _log.Error("Error during decompression, TODO: destroy directory");
                }
            }
            else
            {
                _log.Error("An error was encountered while downloading block group data, exiting.");
            }
            return false;
        }

        public bool CreateShapefileTable(DbConnection conn, string tableName)
        {
            string filename = GetLocalBlockGroupShapefilename();
            return ShapefileHelper.ImportShapefile(conn, this.DbClient, filename, tableName);
        }


        public string GetLocalShapefileName()
        {
            var files = Directory.GetFiles(this.ShapePath, "bg*.shp");
            if ((files != null) && (files.Length > 0))
            {
                return Path.Combine(this.ShapePath, Path.GetFileNameWithoutExtension(files[0]));
            }
            return null;
        }


        public DataTable GetShapefileData()
        {
            DataTable dt = null;
            try
            {
                string filename = GetLocalShapefileName();
                if (!string.IsNullOrEmpty(filename))
                {
                    dt = Shapefile.CreateDataTable(filename, this.State.ToString(), ShapefileHelper.GetGeomFactory());
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error opening shapefile", ex);
            }

            return dt;
        }




        public bool CheckDatabase()
        {
            if (!File.Exists(this.DBFilename))
            {
                _log.DebugFormat("Database not generated for {0}, building...", this.State);                
            }
            else
            {
                _log.DebugFormat("Database already generated for {0}", this.State);
            }

            this.InitDatabase();

            return (DbClient.TestDatabaseConnection());
        }

        

        public void CreateGeographiesTable(DbConnection conn)
        {
            string geographyTablename = "geographies";
            string createGeographyTable = DataClient.GenerateTableSQLFromFields(geographyTablename, 
                GeographyFileReader.Columns);
            DbClient.GetCommand(createGeographyTable, conn).ExecuteNonQuery();

            string geographyFilename = GetLocalGeographyFileName();
            GeographyFileReader geoReader = new GeographyFileReader(geographyFilename);
            if (geoReader.HasFile)
            {
                _log.Debug("Importing Geographies File...");
                string tableSelect = string.Format("select * from {0}", geographyTablename);
                var adapter = DataClient.GetMagicAdapter(conn, DbClient, tableSelect);
                var table = DataClient.GetMagicTable(conn, DbClient, tableSelect);

                _log.Debug("Reading...");
                int primKey = 0;
                foreach (List<string> row in geoReader.GetReader())
                {
                    var geoData = row.ToArray();

                    //add a primary key on there
                    object[] rowData = new object[table.Columns.Count];
                    rowData[0] = primKey++;

                    Array.Copy(geoData, 0, rowData, 1, geoData.Length);

                    table.Rows.Add(rowData);
                }

                if ((table != null) && (table.Rows.Count > 0))
                {
                    _log.Debug("Saving...");

                    this.StateFIPS = (table.Rows[0]["STATE"] as string);

                    adapter.Update(table);
                    table.AcceptChanges();
                }



                _log.Debug("Done!");
            }
            else
            {
                _log.Debug("Could not find geographies file, table not initialized!");
            }
        }

        public void CreateColumnMappingsTable(DbConnection conn)
        {
            string tableName = "columnMappings";
            string createSql = DataClient.GenerateTableSQLFromFields(tableName, SequenceFileReader.Columns);
            DbClient.GetCommand(createSql, conn).ExecuteNonQuery();

            string tableSelect = string.Format("select * from {0}", tableName);
            var adapter = DataClient.GetMagicAdapter(conn, DbClient, tableSelect);
            var table = DataClient.GetMagicTable(conn, DbClient, tableSelect);

            string colMapDir = GetLocalColumnMappingsDirectory();
            if (Directory.Exists(colMapDir))
            {
                _log.Debug("Importing Sequence Files...");

                int ixid = 0;
                foreach (string file in Directory.GetFiles(colMapDir, "Seq*.xls", SearchOption.TopDirectoryOnly))
                {
                    //Extract sequence number from filename
                    string localFilename = Path.GetFileName(file);
                    Regex sequenceFormat = new Regex(@"^Seq(\d{1,4})\.xls$");
                    Match match = sequenceFormat.Match(localFilename);
                    if (!match.Groups[0].Success)
                    {
                        _log.Warn("Malformed filename found in sequence files folder; skipping\n\t" + file);
                        continue;
                    }
                    int seqNo = int.Parse(match.Groups[1].Value);

                    //Read data from file
                    var reader = new SequenceFileReader(file).GetReader();
                    if (reader == null)
                    {
                        _log.Error("One of the sequence files is missing; skipping\n\t" + file);
                        continue;
                    }

                    DataSet fileData = reader.AsDataSet(false);
                    if (fileData.Tables == null || fileData.Tables.Count == 0)
                    {
                        _log.Error("One of the sequence files contained no readable worksheets; skipping\n\t" + 
                            file);
                        continue;
                    }
                    else if (fileData.Tables.Count > 1)
                        _log.Warn("One of the sequence files had multiple worksheets; using the first one\n\t" + 
                            file);

                    DataTable firstWorksheet = fileData.Tables[0];
                    if (firstWorksheet.Rows == null || firstWorksheet.Rows.Count < 2)
                    {
                        _log.Error("One of the sequence files did not have enough rows to read; skipping\n\t" + 
                            file);
                        continue;
                    }
                    else if (firstWorksheet.Rows.Count > 2)
                        _log.Warn("One of the sequence files had too many rows\n\t" + file);

                    //Expected values of row: FILEID,FILETYPE,STUSAB,CHARITER,SEQUENCE,LOGRECNO,...
                    DataRow row = firstWorksheet.Rows[1];   //First row is useless
                    if (row.ItemArray == null || row.ItemArray.Length < 7)
                    {
                        _log.Error("One of the sequence files had bad data, skipping\n\t" + file);
                        continue;
                    }

                    //Add data to database
                    for (int i = 6; i < row.ItemArray.Length; i++)
                    {
                        //                         ixid,   COLNAME,          COLNO, SEQNO
                        var toAdd = new object[] { ixid++, row.ItemArray[i], i + 1, seqNo };
                        table.Rows.Add(toAdd);
                    }

                }

                if ((table != null) && (table.Rows.Count > 0))
                {
                    _log.Debug("Saving...");
                    adapter.Update(table);
                    table.AcceptChanges();
                }
                else
                    _log.Error("Could not read any of the sequence files!");

                _log.Debug("Done!");
            }
            else
            {
                _log.Error("Could not find column mappings directory file, table not initialized!");
            }
        }

        /// <summary>
        /// Call this anyway, should only execute code inside safe blocks
        /// </summary>
        /// <returns></returns>
        protected bool InitDatabase()
        {
            if (!DbClient.TestDatabaseConnection())
            {
                _log.Error("Unable to connect to database");
                return false;
            }

            using (var conn = DbClient.GetConnection())
            {
                _log.Debug("Checking for geographies table...");
                if (!DataClient.HasTable(conn, DbClient, "geographies"))
                {
                    CreateGeographiesTable(conn);
                }

                _log.Debug("Checking for columnMappings table...");
                if (!DataClient.HasTable(conn, DbClient, "columnMappings"))
                {
                    CreateColumnMappingsTable(conn);
                }
            }


            return true;
        }










        
    }
}

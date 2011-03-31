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
using Azavea.NijPredictivePolicing.Common.Data;
using GisSharpBlog.NetTopologySuite.Features;
using GeoAPI.Geometries;

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

        public string SummaryLevel;
        public string WKTFilterFilename;
        public string IncludedVariableFile;
        public bool ReplaceTable = false;


        
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
                        if (!DataClient.HasTable(conn, client, "shapetable"))
                        {
                            _log.Debug("Shapefile table not found, importing...");
                            CreateShapefileTable(conn, "shapetable");
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
            string filename = Directory.GetFiles(this.ShapePath, "*.shp")[0];
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











        /// <summary>
        /// Returns all the variable names in the columnMappings table
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllSequenceVariableNames()
        {
            var allvars = new List<string>(1024);
            using (var conn = DbClient.GetConnection())
            {
                var dt = DataClient.GetMagicTable(conn, DbClient, "select COLNAME from columnMappings");
                foreach (DataRow row in dt.Rows)
                {
                    allvars.Add(row[0] as string);
                }
            }
            return allvars;
        }


        public HashSet<string> GetFilteredLRUs(DbConnection conn)
        {
            _log.Debug("Filtering requested LRUs");
            HashSet<string> results = new HashSet<string>();
            if (!string.IsNullOrEmpty(this.SummaryLevel))
            {
                //sql-injection here: fix maybe?
                string sql = string.Format("select LOGRECNO from geographies where SUMLEVEL = '{0}'", this.SummaryLevel);
                using (var reader = DbClient.GetCommand(sql, conn).ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string lrn = reader.GetString(0);
                        if (!results.Contains(lrn))
                            results.Add(lrn);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(this.WKTFilterFilename))
            {
                //TODO: implement
                _log.Warn("Not Yet Implemented");
            }
            return results;
        }

        public DataTable GetRequestedVariables(DbConnection conn)
        {
            DataTable dt = new DataTable();
            _log.Debug("Filtering requested variables");

            if (!string.IsNullOrEmpty(IncludedVariableFile))
            {
                var lines = File.ReadAllLines(this.IncludedVariableFile);
                if ((lines == null) || (lines.Length == 0))
                {
                    _log.Error("**Provided variable file had no contents");
                    return null;
                }

                HashSet<string> variables = new HashSet<string>(lines);

                dt = DataClient.GetMagicTable(conn, DbClient, "select COLNAME, COLNO, SEQNO from columnMappings");
                foreach (DataRow row in dt.Rows)
                {
                    string varName = row[0] as string;
                    if (!variables.Contains(varName))
                    {
                        row.Delete();
                    }
                }
                dt.AcceptChanges();
            }



            //other ways to specify variables?



            return dt;
        }


        public bool CheckBuildVariableTable(string tableName)
        {
            try
            {
                using (var conn = DbClient.GetConnection())
                {
                    if (DataClient.HasTable(conn, DbClient, tableName))
                    {
                        if (ReplaceTable)
                        {
                            DbClient.GetCommand(string.Format("DROP TABLE IF EXISTS {0};", tableName), conn).ExecuteNonQuery();
                        }
                        else
                        {
                            _log.DebugFormat("Table {0} was already built", tableName);
                            return true;
                        }
                    }

                    //get a list of LRUs we want

                    HashSet<string> includedIDs = GetFilteredLRUs(conn);

                    //get all the variable mapping information we want
                    DataTable reqVariablesDT = GetRequestedVariables(conn);

                    //get the respective files
                    DataTable newTable = new DataTable();
                    newTable.Columns.Add("LOGRECNO", typeof(string));

                    Dictionary<string, DataRow> allRows = new Dictionary<string, DataRow>(includedIDs.Count);

                    foreach (var id in includedIDs)
                    {
                        var row = newTable.NewRow();
                        row[0] = id;
                        newTable.Rows.Add(row);

                        allRows[id] = row;
                    }

                    int varNum = 0;


                    _log.Debug("Importing Columns");
                    foreach (DataRow variableRow in reqVariablesDT.Rows)
                    {
                        varNum++;

                        var sequenceNo = Utilities.GetAs<int>(variableRow["SEQNO"] as string, -1);
                        var seqFile = Directory.GetFiles(this.CurrentDataPath, "e*" + sequenceNo.ToString("0000") + "000.txt");    //0001000
                        if ((seqFile == null) || (seqFile.Length == 0))
                        {
                            _log.DebugFormat("Couldn't find sequence file {0}", sequenceNo);
                            continue;
                        }

                        //TODO: alternate column naming?
                        var newColumnName = variableRow["COLNAME"] as string;
                        int columnIDX = Utilities.GetAs<int>(variableRow["COLNO"] as string, -1);

                        _log.DebugFormat("Importing {0}...", newColumnName);

                        if (newTable.Columns.Contains(newColumnName))
                        {
                            newColumnName = newColumnName + varNum;
                        }
                        newTable.Columns.Add(newColumnName, typeof(double));

                        CommaSeparatedValueReader reader = new CommaSeparatedValueReader(seqFile[0], false);
                        foreach (List<string> values in reader)
                        {
                            string lru = values[5];
                            if (!includedIDs.Contains(lru))
                                continue;

                            //_log.Debug("Found one!");

                            if (columnIDX < values.Count)
                            {
                                double val = Utilities.GetAs<double>(values[columnIDX], double.NaN);
                                if (!double.IsNaN(val))
                                {
                                    allRows[lru][newColumnName] = val;
                                }
                            }
                        }
                        reader.Close();
                    }

                    _log.DebugFormat("Creating Table {0}", tableName);
                    string createTableSQL = SqliteDataClient.GenerateTableSQLFromTable(tableName, newTable, "LOGRECNO");
                    DbClient.GetCommand(createTableSQL, conn).ExecuteNonQuery();

                    _log.DebugFormat("Saving Table {0}...", tableName);
                    var dba = DataClient.GetMagicAdapter(conn, DbClient, "select * from " + tableName);
                    dba.Update(newTable);
                    _log.Debug("Done!");



                    _log.Debug("Import complete!");

                    //TODO: save the table to the database
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error while building table", ex);
            }

            return false;
        }




        public bool ExportShapefile(string tableName)
        {
            try
            {
                bool shouldRun = false;
                if (!shouldRun)
                {
                    _log.Warn("**NOT YET COMPLETED**");
                    return false;
                }

                using (var conn = DbClient.GetConnection())
                {
                    //begin...
                    //string destFilepath = GetLocalBlockGroupShapefilename();
                    //string sql = string.Format(".dumpshp(\"Testshp.shp\", \"{0}\");", tableName);
                    //DbClient.GetCommand(sql, conn).ExecuteNonQuery();


                    //FeatureCollection


                    //shapefile
                    if (!DataClient.HasTable(conn, DbClient, "shapetable"))
                    {
                        this.CreateShapefileTable(conn, "shapetable");
                    }

                    //string shapeSQL = @"select s.pkuid, s.Geometry, g.LOGRECNO from shapetable s, geographies g where g.COUNTY = s.COUNTY and g.TRACT = '00' || s.TRACT and g.BLKGRP = s.BLKGROUP";
                    

                    var features = new List<Feature>();


                    var wholeShapeTable = DataClient.GetMagicTable(conn, DbClient, "select * from shapetable");
                    var wholeGeomTable = DataClient.GetMagicTable(conn, DbClient, "select * from geographies");


                    //TODO: figure out way to join against shapefile by LOGRECNO, so we can tie variables to geometries,
                    //TODO: then finish this function



                    string shapeSQL = @"
select s.pkuid, s.Geometry, g.LOGRECNO from shapetable s, geographies g 
where g.COUNTY || '' = s.COUNTY || ''
";

//                    string shapeSQL = @"
//select s.pkuid, s.Geometry, g.LOGRECNO from shapetable s, geographies g 
//where g.COUNTY || '' = s.COUNTY || ''
//and g.TRACT || '' = '00' || s.TRACT || '' 
//and g.BLKGRP || ''  = s.BLKGROUP|| '' ";

                    var shapesDT = DataClient.GetMagicTable(conn, DbClient, shapeSQL);
                    Dictionary<string, DataRow> shapeDict = new Dictionary<string, DataRow>();
                    foreach (DataRow row in shapesDT.Rows)
                    {
                        var id = row["LOGRECNO"] as string;

                        if (shapeDict.ContainsKey(id))
                        {
                            _log.Warn("The query returned multiple copies of this row!");
                        }

                        shapeDict[id] = row;
                    }

                    var variablesDT = DataClient.GetMagicTable(conn, DbClient, "select * from " + tableName);
                    foreach (DataRow row in variablesDT.Rows)
                    {
                        var id = row["LOGRECNO"] as string;
                        IGeometry geom = (IGeometry)shapeDict[id]["Geometry"];

                        AttributesTable t = new AttributesTable();
                        foreach (DataColumn col in variablesDT.Columns)
                        {
                            t.AddAttribute(col.ColumnName, row[col.Ordinal]);
                        }

                        features.Add(new Feature(geom, t));
                    }

                    var writer = new ShapefileDataWriter("testshp", ShapefileHelper.GetGeomFactory());
                    writer.Write(features);
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error while exporting shapefile", ex);
            }
            return false;
        }


    }
}

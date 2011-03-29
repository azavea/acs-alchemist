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
            init();
        }

        public AcsDataManager(AcsState aState)
        {
            this.State = aState;
            init();
        }


        public void init()
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
            string url = Settings.StateBlockGroupShapefileRootURL + Settings.StateBlockGroupShapefileFormatURL;
            url = url.Replace("{FIPS-code}", this.StateFIPS);
            return url;
        }


        public string GetLocalBlockGroupShapefilename()
        {
            string template = Settings.StateBlockGroupShapefileFormatURL;
            template = template.Replace("{FIPS-code}", this.StateFIPS);

            return FileUtilities.PathCombine(this.WorkingPath, template);
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

        



        /// <summary>
        /// Downloads the SHAPE FILE
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



        public DataTable GetShapefileData()
        {
            DataTable dt = null;
            try
            {


                var files = Directory.GetFiles(this.ShapePath, "bg*.shp");
                if ((files != null) && (files.Length > 0))
                {
                    string shapeFileName = Path.Combine(this.ShapePath, Path.GetFileNameWithoutExtension(files[0]));
                    dt = Shapefile.CreateDataTable(shapeFileName, this.State.ToString(), ShapefileHelper.GetGeomFactory());
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
                this.InitDatabase();
            }
            else
            {
                _log.DebugFormat("Database already generated for {0}", this.State);
            }

            return (DbClient.TestDatabaseConnection());
        }

        protected bool InitDatabase()
        {
            if (!DbClient.TestDatabaseConnection())
            {
                _log.Error("Unable to connect to database");
                return false;
            }

            var conn = DbClient.GetConnection();
            string spatialitePath = System.IO.Path.Combine(Environment.CurrentDirectory, "libspatialite-2.dll");
            DbClient.GetCommand("SELECT load_extension('" + spatialitePath + "');", conn).ExecuteNonQuery();
            DbClient.GetCommand("SELECT InitSpatialMetaData()", conn).ExecuteNonQuery();

            string geographyTablename = "geographies";
            string createGeographyTable = DataClient.GenerateTableSQLFromFields(geographyTablename, GeographyFileReader.Columns);
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



            ////-- Spatial Reference System
            //nonQueryDB(conn, "INSERT INTO spatial_ref_sys (srid, auth_name, auth_srid, ref_sys_name, proj4text) VALUES(101, 'POSC', 32214, 'WGS 72 / UTM zone 14N', '+proj=utm +zone=14 +ellps=WGS72 +units=m +no_defs');");

            ////EXAMPLE:
            ////-- Lakes
            //nonQueryDB(conn, "CREATE TABLE lakes (fid INTEGER NOT NULL PRIMARY KEY,name VARCHAR(64)); ");





            return true;
        }


 





        
    }
}

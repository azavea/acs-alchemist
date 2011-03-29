using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Azavea.NijPredictivePolicing.Common;
using System.IO;
using Azavea.NijPredictivePolicing.Common.DB;
using Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.Transfer
{
    public class AcsDataManager
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AcsState State = AcsState.None;
        public string DataPath;
        public string DBPath;
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
            this.DataPath = FileLocator.GetStateBlockGroupDataDir(this.State);
            this.DBPath = FileUtilities.PathCombine(this.DataPath, this.State.ToString() + ".sqlite");
            this.DbClient = DataClient.GetDefaultClient(this.DBPath);

            this.CheckDatabase();
        }

        protected void CheckDatabase()
        {
            if (!File.Exists(this.DBPath))
            {
                _log.DebugFormat("Database not generated for {0}, building...", this.State);
                this.InitDatabase();
            }
            else
            {
                _log.DebugFormat("Database already generated for {0}, opening...", this.State);

                //TODO: Open database!
            }
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


            GeographyFileReader geoReader = new GeographyFileReader(this.State);
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

                _log.Debug("Saving...");
                adapter.Update(table);
                table.AcceptChanges();

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

        protected void ImportGeographyFile()
        {

        }







    }
}

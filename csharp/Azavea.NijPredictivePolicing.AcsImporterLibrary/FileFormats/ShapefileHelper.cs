using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using Azavea.NijPredictivePolicing.Common.DB;
using System.Data;
using System.Data.Common;
using log4net;
using System.IO;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats
{
    public class ShapefileHelper
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SqliteDataClient Client;

        public bool OpenShapefile(string filename, string tableName)
        {
            string databaseFileName = Path.Combine(Path.GetDirectoryName(filename), "shape.dat");
            this.Client = new SqliteDataClient(databaseFileName);

            using (DbConnection conn = Client.GetConnection())
            {
                return ShapefileHelper.ImportShapefile(conn, Client, filename, tableName);
            }
        }

        public static bool ImportShapefile(DbConnection conn, IDataClient client, string filename, string tableName)
        {
            try
            {
                if (DataClient.HasTable(conn, client, tableName))
                {
                    client.GetCommand("DROP TABLE " + tableName).ExecuteNonQuery();
                }

                //string databaseFileName = Path.Combine(Path.GetDirectoryName(filename), "shape.dat");

                //trim off the '.shp' from the end
                filename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));
                string sql = string.Format("CREATE VIRTUAL TABLE " + tableName + " USING VirtualShape('{0}', CP1252, 4269);", filename);
                client.GetCommand(sql, conn).ExecuteNonQuery();
                
                _log.DebugFormat("Imported Shapefile {0} into table {1}",
                    Path.GetFileNameWithoutExtension(filename),
                    tableName);

                return true;

            }
            catch (Exception ex)
            {
                _log.Error("Error loading shapefile", ex);
            }
            return false;
        }


        public DataTable GetSchema()
        {
            if (Client == null)
                return null;

            using (DbConnection conn = Client.GetConnection())
            {
                var dt = conn.GetSchema();
                return conn.GetSchema("Tables");
            }
        }




        public static GeometryFactory GetGeomFactory()
        {
            //Geographic - 4269 - North America NAD83, Geographic, decimal degrees
            //Geographic - 4326 - World WGS84, Geographic, decimal degrees
            return new GeometryFactory(new PrecisionModel(), 4269);
        }


        public static System.Data.DataTable GetTable(string shapefilename, AcsState acsState)
        {
            return Shapefile.CreateDataTable(shapefilename, acsState.ToString(), ShapefileHelper.GetGeomFactory());
        }





    }
}

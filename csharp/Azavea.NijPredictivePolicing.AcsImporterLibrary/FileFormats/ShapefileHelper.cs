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

        public bool OpenShapefile(string filename)
        {
            //TODO: in memory?
            try
            {
                string databaseFileName = Path.Combine(Path.GetDirectoryName(filename), "shape.sqlite");
                this.Client = new SqliteDataClient(databaseFileName);
                if (Client.TestDatabaseConnection())
                {
                    using (DbConnection conn = Client.GetConnection())
                    {
                        string spatialitePath = System.IO.Path.Combine(Environment.CurrentDirectory, "libspatialite-2.dll");
                        int result = Client.GetCommand("SELECT load_extension('" + spatialitePath + "');").ExecuteNonQuery();
                        _log.DebugFormat("spatialite loaded? {0}", result);

                        //result = Client.GetCommand("SELECT InitSpatialMetaData();").ExecuteNonQuery();
                        //_log.DebugFormat("InitSpatialMetaData loaded? {0}", result);

                        //string sql = string.Format(".loadshp {0} shapetable CP1252", filename);
                        string sql = string.Format("CREATE VIRTUAL TABLE test_shape USING VirtualShape(\"{0}\");", filename);
                        
                        result = Client.GetCommand(sql).ExecuteNonQuery();
                        _log.DebugFormat("Shapefile loaded? {0}", result);
                        return true;
                    }
                }
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

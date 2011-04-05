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
using Azavea.NijPredictivePolicing.Common;

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

        /// <summary>
        /// Generates a proj file for a given shapefile if it's missing
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool MakeCensusProjFile(string filename)
        {
            string WGS84NAD83 = "GEOGCS[\"GCS_North_American_1983\",DATUM[\"D_North_American_1983\",SPHEROID[\"GRS_1980\",6378137,298.257222101]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]]";
            string prjFileName = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename)) + ".prj";

            if (!File.Exists(prjFileName))
            {
                File.WriteAllText(prjFileName, WGS84NAD83);
            }
            return true;
        }

        /// <summary>
        /// A .prj file seems to be exactly what we're asking for as input, so copy er over.
        /// </summary>
        /// <param name="wktProjFilename"></param>
        /// <param name="newShapefilename"></param>
        /// <returns></returns>
        public static bool MakeOutputProjFile(string wktProjFilename, string filename)
        {
            string prjFileName = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename)) + ".prj";
            File.WriteAllText(prjFileName, File.ReadAllText(wktProjFilename));
            return true;
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


        public static DbaseFileHeader SetupHeader(DataTable table)
        {
            DbaseFileHeader header = new DbaseFileHeader();
            foreach (DataColumn col in table.Columns)
            {
                Type t = col.DataType;
                string columnName = Utilities.EnsureMaxLength(col.ColumnName, 10);
                //string columnName = "col" + col.Ordinal;

                if (t == typeof(bool))
                {
                    header.AddColumn(columnName, 'L', 1, 0);
                }
                else if (t == typeof(string))
                {
                    header.AddColumn(columnName, 'C', 254, 0);
                }
                else if (t == typeof(DateTime))
                {
                    // D stores only the date
                    //retVal.AddColumn(shapefileColumnName, 'D', 8, 0);
                    header.AddColumn(columnName, 'C', 22, 0);
                }
                else if (t == typeof(float) || t == typeof(double) || t == typeof(decimal))
                {
                    header.AddColumn(columnName, 'N', 18, 10);
                }
                else if (t == typeof(short) || t == typeof(int) || t == typeof(long)
                    || t == typeof(ushort) || t == typeof(uint) || t == typeof(ulong))
                {
                    header.AddColumn(columnName, 'N', 18, 0);
                }
            }

            return header;
        }

        public static string GetRemoteShapefileURL(BoundaryLevels level, string stateFips)
        {
            string url = string.Empty;
            switch (level)
            {
                case BoundaryLevels.census_blockgroups:
                    url = Settings.ShapeFileBlockGroupURL + Settings.ShapeFileBlockGroupFilename;
                    break;
                case BoundaryLevels.census_tracts:
                    url = Settings.ShapeFileTractURL + Settings.ShapeFileTractFilename;
                    break;
                case BoundaryLevels.county_subdivisions:
                    url = Settings.ShapeFileCountySubdivisionsURL + Settings.ShapeFileCountySubdivisionsFilename;
                    break;
                case BoundaryLevels.voting:
                    url = Settings.ShapeFileVotingURL + Settings.ShapeFileVotingFilename;
                    break;
                case BoundaryLevels.zipthree:
                    url = Settings.ShapeFileThreeDigitZipsURL + Settings.ShapeFileThreeDigitZipsFilename;
                    break;
                case BoundaryLevels.zipfive:
                    url = Settings.ShapeFileFiveDigitZipsURL + Settings.ShapeFileFiveDigitZipsFilename;
                    break;
                case BoundaryLevels.counties:
                    url = Settings.ShapeFileCountiesURL + Settings.ShapeFileCountiesFilename;
                    break;

                case BoundaryLevels.states:
                case BoundaryLevels.census_regions:
                case BoundaryLevels.census_divisions:
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(url))
            {
                url = url.Replace("{FIPS-code}", stateFips);
            }

            return url;
        }


        //public static void DisplayBoundaryLevels()
        //{
        //    Type enumType = typeof(BoundaryLevels);
        //    var levels = Enum.GetValues(enumType);

        //    _log.Debug("Boundary Levels: ");
        //    foreach (var value in levels)
        //    {
        //        _log.Debug(value);
        //    }
        //}





    }
}

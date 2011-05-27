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
using System.Text.RegularExpressions;
using GeoAPI.CoordinateSystems;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats
{
    public static class ShapefileHelper
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Regex _forbiddenShapefiles;

        static ShapefileHelper()
        {
            StringBuilder newRegex = new StringBuilder(512);

            string [] levels = new string[] {
                Settings.ShapeFileBlockGroupFilename,
                Settings.ShapeFileTractFilename,
                Settings.ShapeFileCountySubdivisionsFilename,
                Settings.ShapeFileVotingFilename,
                Settings.ShapeFileThreeDigitZipsFilename,
                Settings.ShapeFileFiveDigitZipsFilename,
                Settings.ShapeFileCountiesFilename
                };

            foreach (string level in levels)
            {
                string temp = level.Replace(Settings.FipsPlaceholder, "\\d{2}").Replace("_shp.zip", "");
                newRegex.Append("^").Append(temp).Append("$").Append("|");
            }

            ShapefileHelper._forbiddenShapefiles = new Regex(newRegex.ToString(0, newRegex.Length - 1));
        }

        public static bool OpenShapefile(string filename, string tableName)
        {
            string databaseFileName = Path.Combine(Path.GetDirectoryName(filename), "shape.dat");
            var client = new SqliteDataClient(databaseFileName);

            using (DbConnection conn = client.GetConnection())
            {
                return ShapefileHelper.ImportShapefile(conn, client, filename, tableName, 4269);
            }
        }

        /// <summary>
        /// Imports a shapefile into the database.  Do not use this to load census shapefiles.
        /// </summary>
        /// <param name="filename">The path of the file to import</param>
        /// <param name="DbClient">The database to load the file into</param>
        /// <param name="srid">The SRID to use.  If srid &gt;= 0, the .prj won't be checked.  If srid &lt; 0, the .prj file will be required, and srid will be changed to whatever is in the .prj file</param>
        /// <returns>True on success, False on failure</returns>
        public static bool LoadShapefile(string filename, IDataClient DbClient, ref int srid)
        {
            if ((string.IsNullOrEmpty(filename)) || (!File.Exists(filename)))
            {
                _log.ErrorFormat("Couldn't find file {0}", filename);
                return false;
            }

            if (ShapefileHelper.IsForbiddenShapefileName(Path.GetFileNameWithoutExtension(filename)))
            {
                _log.ErrorFormat("{0} conflicts with a census shapefile name, please rename", filename);
                return false;
            }

            try
            {
                //string workingDirectory = FileUtilities.SafePathEnsure(TempPath, Path.GetFileNameWithoutExtension(filename));
                using (DbConnection conn = DbClient.GetConnection())
                {
                    string prjFileName = Path.Combine(Path.GetDirectoryName(filename),
                        Path.GetFileNameWithoutExtension(filename)) + ".prj";
                    if (srid < 0 && File.Exists(prjFileName))
                    {
                        ICoordinateSystem crs = Utilities.GetCoordinateSystemByWKTFile(prjFileName);
                        string authority = crs.Authority.ToLower();
                        if (authority == "epsg" || authority == "epgs")
                            srid = (int)crs.AuthorityCode;
                        else
                        {
                            _log.Error("Could not use the authority code in the shapefile's .prj file and none was provided.  Please either specify the correct EPSG authority code / SRID on the command line or reproject your shapefile to a projection with a valid EPSG authority code.");
                            return false;
                        }
                    }

                    if (ShapefileHelper.ImportShapefile(conn, DbClient, filename, Path.GetFileNameWithoutExtension(filename), srid))
                    {
                        

                        _log.Debug("Shapefile imported successfully...");
                    }
                    else
                    {
                        _log.Error("Error while importing shapefile.");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error while importing shapefile", ex);
            }

            return false;
        }


        public static bool ImportShapefile(DbConnection conn, IDataClient client, 
            string filename, string tableName, int srid)
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
                string sql = string.Format("CREATE VIRTUAL TABLE " + tableName + " USING VirtualShape('{0}', CP1252, {1});", filename, srid);
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


        public static DataTable GetSchema(SqliteDataClient client)
        {
            if (client == null)
                return null;

            using (DbConnection conn = client.GetConnection())
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
            string prjFileName = Path.Combine(Path.GetDirectoryName(filename), 
                Path.GetFileNameWithoutExtension(filename)) + ".prj";
            try
            {
                if (!File.Exists(Settings.AcsPrjFilePath))
                {
                    if (!File.Exists(prjFileName))
                    {
                        File.WriteAllText(prjFileName, Settings.DefaultPrj);
                    }
                }
                else
                {
                    File.Copy(Settings.AcsPrjFilePath, prjFileName);
                }
            }
            catch
            {
                return false;
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

                AddColumn(header, columnName, t);
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
                url = url.Replace(Settings.FipsPlaceholder, stateFips);
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


        public static bool IsForbiddenShapefileName(string name)
        {
            return ShapefileHelper._forbiddenShapefiles.IsMatch(name.ToLower().Replace(".shp", ""));
        }






        public static void AddColumn(DbaseFileHeader header, string columnName, Type t)
        {
            columnName = Utilities.EnsureMaxLength(columnName, 10);
            
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



    }
}

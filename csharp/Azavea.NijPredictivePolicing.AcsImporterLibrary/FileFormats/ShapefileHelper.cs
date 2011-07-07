/*
  Copyright (c) 2011 Azavea, Inc.
 
  This file is part of _SOLUTIONNAME_.

  _SOLUTIONNAME_ is free software: you can redistribute it and/or modify
  it under the terms of the GNU Lesser General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  _SOLUTIONNAME_ is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Lesser General Public License for more details.

  You should have received a copy of the GNU Lesser General Public License
  along with _SOLUTIONNAME_.  If not, see <http://www.gnu.org/licenses/>.
*/

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
    /// <summary>
    /// This class contains a number of small utilities to simplify the use of shapefiles in the importer
    /// </summary>
    public static class ShapefileHelper
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Imports a shapefile into the database.  Do not use this to load census shapefiles.
        /// </summary>
        /// <param name="filename">The path of the file to import</param>
        /// <param name="DbClient">The database to load the file into</param>
        /// <returns>True on success, False on failure</returns>
        public static bool LoadShapefile(string filename, string tableName, IDataClient DbClient, out ICoordinateSystem CRS)
        {
            CRS = null;
            if ((string.IsNullOrEmpty(filename)) || (!File.Exists(filename)))
            {
                _log.ErrorFormat("LoadShapefile failed: filename was empty or file does not exist {0}", filename);
                return false;
            }

            try
            {
                string fileWithoutExt = Path.GetFileNameWithoutExtension(filename);
                string prjFileName = Path.Combine(Path.GetDirectoryName(filename), fileWithoutExt) + ".prj";

                if (ShapefileHelper.IsForbiddenShapefileName(fileWithoutExt))
                {
                    _log.ErrorFormat("LoadShapefile failed: {0} conflicts with a reserved census shapefile name, please rename", fileWithoutExt);
                    return false;
                }

                if (File.Exists(prjFileName))
                {
                    CRS = Utilities.GetCoordinateSystemByWKTFile(prjFileName);
                }
                else
                {
                    _log.ErrorFormat("LoadShapefile failed: shapefile {0} is missing a .prj file.", filename);
                    return false;
                }

                //string workingDirectory = FileUtilities.SafePathEnsure(TempPath, Path.GetFileNameWithoutExtension(filename));
                using (DbConnection conn = DbClient.GetConnection())
                {
                    if (!ShapefileHelper.ImportShapefile(conn, DbClient, filename, tableName, (int)CRS.AuthorityCode))
                    {
                        _log.Error("LoadShapefile failed: unable to import shapefile.");
                        return false;
                    }
                }

                _log.Debug("Shapefile imported successfully...");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error while importing shapefile", ex);
            }

            return false;
        }


        /// <summary>
        /// Imports the provided shapefile into a sqlite database using the VirtualShape extension
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="client"></param>
        /// <param name="filename"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool ImportShapefile(DbConnection conn, IDataClient client, string filename, string tableName, int srid)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    _log.ErrorFormat("ImportShapefile Failed!: File does not exist {0}", filename);
                    return false;
                }

                if (DataClient.HasTable(conn, client, tableName))
                {
                    client.GetCommand("DROP TABLE " + tableName).ExecuteNonQuery();
                }

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
                _log.Error("ImportShapefile failed: Error while loading shapefile ", ex);
            }
            return false;
        }


        /// <summary>
        /// Helper function for checking structure of existing Db
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static DataTable GetSchema(SqliteDataClient client)
        {
            if (client == null)
                return null;

            using (DbConnection conn = client.GetConnection())
            {
                //var dt = conn.GetSchema();
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
                    File.Copy(Settings.AcsPrjFilePath, prjFileName, true);
                }
            }
            catch (Exception ex)
            {
                _log.Error("MakeCensusProjFile failed: An exception was thrown ", ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Copies an existing .prj file next to an existing shapefile (with correct destination name)
        /// </summary>
        /// <param name="wktProjFilename"></param>
        /// <param name="newShapefilename"></param>
        /// <returns></returns>
        public static bool MakeOutputProjFile(string sourceProjectionFilename, string destShapefilename)
        {
            string prjFileName = Path.Combine(
                Path.GetDirectoryName(destShapefilename), 
                Path.GetFileNameWithoutExtension(destShapefilename)) + ".prj";

            File.Copy(sourceProjectionFilename, prjFileName, true);        //File.WriteAllText(prjFileName, File.ReadAllText(wktProjFilename));
            return true;
        }


        /// <summary>
        /// Constructs the default GeometryFactory for 4269 (census projection)
        /// </summary>
        /// <returns></returns>
        public static GeometryFactory GetGeomFactory()
        {
            //Geographic - 4269 - North America NAD83, Geographic, decimal degrees
            //Geographic - 4326 - World WGS84, Geographic, decimal degrees
            return new GeometryFactory(new PrecisionModel(), 4269);
        }

        ///// <summary>
        ///// Helper function for cre
        ///// </summary>
        ///// <param name="shapefilename"></param>
        ///// <param name="acsState"></param>
        ///// <returns></returns>
        //public static System.Data.DataTable GetTable(string shapefilename, AcsState acsState)
        //{
        //    return Shapefile.CreateDataTable(shapefilename, acsState.ToString(), ShapefileHelper.GetGeomFactory());
        //}


        /// <summary>
        /// Consumes an ADO.net datatable and correctly initializes a Shapefile header object
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Helper function for SetupHeader, correctly maps C# data types to DbaseFileHeader types
        /// </summary>
        /// <param name="header"></param>
        /// <param name="columnName"></param>
        /// <param name="t"></param>
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


        /// <summary>
        /// Helper function for constructing the correct census URL for a particular Boundary Level / State
        /// </summary>
        /// <param name="level"></param>
        /// <param name="stateFips"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Checks to make sure a given table name doesn't conflict
        /// with any of the census shapefiles
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsForbiddenShapefileName(string name)
        {
            string[] levels = new string[] {
                Settings.ShapeFileBlockGroupFilename,
                Settings.ShapeFileTractFilename,
                Settings.ShapeFileCountySubdivisionsFilename,
                Settings.ShapeFileVotingFilename,
                Settings.ShapeFileThreeDigitZipsFilename,
                Settings.ShapeFileFiveDigitZipsFilename,
                Settings.ShapeFileCountiesFilename
                };

            StringBuilder newRegex = new StringBuilder(512);
            foreach (string level in levels)
            {
                string temp = level.Replace(Settings.FipsPlaceholder, "\\d{2}").Replace("_shp.zip", "");
                newRegex.Append("^").Append(temp).Append("$").Append("|");
            }

            var regex = new Regex(newRegex.ToString(0, newRegex.Length - 1));
            return regex.IsMatch(name.ToLower().Replace(".shp", ""));
        }

        /// <summary>
        /// Helper function for testing the shapefile importer
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool OpenShapefile(string filename, string tableName)
        {
            string databaseFileName = Path.Combine(Path.GetDirectoryName(filename), "shape.dat");
            var client = new SqliteDataClient(databaseFileName);

            using (DbConnection conn = client.GetConnection())
            {
                return ShapefileHelper.ImportShapefile(conn, client, filename, tableName, -1);
            }
        }


    }
}

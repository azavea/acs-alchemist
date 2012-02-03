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
using System.Collections;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.CoordinateSystems;
using ProjNet.CoordinateSystems;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.Transfer
{
    public class AcsDataManager : IDisposable
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Gets selected State FIPS code as a string
        /// </summary>
        public string StateFIPS { get { return ((int)(this.State)).ToString("00"); } }

        /// <summary>
        /// Selected State
        /// </summary>
        public AcsState State = AcsState.None;

        public string WorkingPath;
        public string ShapePath;
        public string CurrentDataPath;
        public string DBFilename;

        protected string _summaryLevel;
        public string SummaryLevel
        {
            get
            {
                return _summaryLevel;
            }
            set
            {
                _summaryLevel = value;
                if (!string.IsNullOrEmpty(value))
                {
                    _summaryLevel = Utilities.GetAs<int>(value, -1).ToString("000");
                }
                else
                {
                    _summaryLevel = string.Empty;
                }
            }
        }

        /// <summary>
        /// If set, the manager will look for a file of WKTs that it will use to ensure all exported geometries
        /// at least intersect
        /// </summary>
        public string ExportFilterFilename;

        /// <summary>
        /// If set, the manager will read in as a CSV, and import the variable values into the provided job table
        /// </summary>
        public string DesiredVariablesFilename;

        /// <summary>
        /// If set, the manager will erase a previous job with the same name, and replace it
        /// </summary>
        public bool ReplaceTable = false;

        public string OutputProjectionFilename;

        public double GridCellWidth = 10000;
        public double GridCellHeight = 10000;

        /// <summary>
        /// If set, the manager will use this envelope while calculating the export grid
        /// (minx, miny, maxx, maxy)
        /// </summary>
        public string GridEnvelopeFilename;

        public bool IncludeEmptyGridCells = false;
        public string OutputFolder;
        public bool PreserveJam;

        /// <summary>
        /// SRID to use for shapefile filter
        /// </summary>
        //public int SRID;

        /// <summary>
        /// If true, add column GEOID_STRP to the shapefile output with values the same as GEOID except without the "15000US" prefix
        /// </summary>
        public bool AddStrippedGEOIDcolumn = false;
        
        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// Initialize our properties, and ensure our path structure is intact
        /// </summary>
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
        public string GetLocalColumnMappingsDirectory()
        {
            return FileUtilities.PathCombine(FileLocator.TempPath, Settings.ColumnMappingsFileName);
        }
        


        //public string GetRemoteStateShapefileURL()
        //{
        //    string url = Settings.StateBlockGroupShapefileRootURL +  Settings.StateBlockGroupShapefileFormatURL;
        //    url = url.Replace("{FIPS-code}", this.StateFIPS);
        //    return url;
        //}
        //public string GetLocalBlockGroupShapefilename()
        //{
        //    string template = Settings.StateBlockGroupShapefileFormatURL;
        //    template = template.Replace("{FIPS-code}", this.StateFIPS);

        //    return Path.Combine(this.WorkingPath, template);
        //}


        public bool CheckColumnMappingsFile()
        {
            _log.DebugFormat("Downloading column mappings file ({0})", Settings.ColumnMappingsFileName);

            string desiredUrl = Settings.CurrentColumnMappingsFileUrl;
            string destFilepath = FileUtilities.PathCombine(FileLocator.TempPath,
                Settings.ColumnMappingsFileName + Settings.ColumnMappingsFileExtension);


            if (!FileDownloader.GetFileByURL(desiredUrl, destFilepath))
            {
                _log.ErrorFormat("Downloading sequence files failed: unable to retrieve {0} ", desiredUrl);
                return false;
            }

            if (!FileLocator.ExpandZipFile(destFilepath, GetLocalColumnMappingsDirectory()))
            {
                _log.ErrorFormat("Downloading sequence files failed: unable to expand file {0}", destFilepath);
                return false;
            }

            _log.Debug("Downloading sequence files... Done!");
            return true;
        }

        /// <summary>
        /// Downloads the census DATA for the given state
        /// </summary>
        /// <returns></returns>
        public bool CheckBlockGroupFile()
        {
            _log.DebugFormat("Downloading census data for {0}", this.State);

            string desiredUrl = FileLocator.GetStateBlockGroupUrl(this.State);
            string destFilepath = GetLocalBlockGroupZipFileName();


            if (!FileDownloader.GetFileByURL(desiredUrl, destFilepath))
            {
                _log.ErrorFormat("Downloading census data failed: unable to retrieve {0} ", desiredUrl);
                return false;
            }

            if (!FileLocator.ExpandZipFile(destFilepath, this.CurrentDataPath))
            {
                _log.ErrorFormat("Downloading census data failed: unable to expand file {0}", destFilepath);
                return false;
            }

            _log.Debug("Downloading census data... Done!");
            return true;
        }



        /// <summary>
        /// Downloads the SHAPE FILEs, must be run before initializing the database!  
        /// Since this will be imported into the database!
        /// </summary>
        /// <returns></returns>
        public bool CheckShapefiles()
        {
            BoundaryLevels[] shapeFileLevels = new BoundaryLevels[] {
                BoundaryLevels.census_blockgroups,
                BoundaryLevels.census_tracts,
                /*
                BoundaryLevels.county_subdivisions,
                BoundaryLevels.zipthree,
                BoundaryLevels.zipfive,
                BoundaryLevels.counties
                 */
            };

            foreach (BoundaryLevels level in shapeFileLevels)
            {
                string url = ShapefileHelper.GetRemoteShapefileURL(level, this.StateFIPS);
                string localPath = Path.Combine(this.WorkingPath, Path.GetFileName(url));
                string name = level.ToString();

                if (!DownloadAndImportShapefile(url, localPath, name, name))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Helper function for retrieving a given zip file from a URL, 
        /// decompressing it, finding a shapefile in it, and importing that into the database
        /// </summary>
        /// <param name="desiredUrl"></param>
        /// <param name="destFilepath"></param>
        /// <param name="niceName"></param>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public bool DownloadAndImportShapefile(string desiredUrl, string destFilepath, string niceName, string tablename)
        {
            _log.DebugFormat("Downloading shapefile of {0} for {1}", niceName, this.State);

            if (!FileDownloader.GetFileByURL(desiredUrl, destFilepath))
            {
                _log.ErrorFormat("Shapefile download failed: Could not download {0}", niceName);
                return false;
            }

            if (!FileLocator.ExpandZipFile(destFilepath, this.ShapePath))
            {
                _log.ErrorFormat("Shapefile import failed: Could not decompress {0}", niceName);
                return false;
            }
            _log.DebugFormat("State {0} decompressed successfully", niceName);


            var client = DbClient;
            using (var conn = client.GetConnection())
            {
                if (DataClient.HasTable(conn, client, tablename))
                {
                    _log.DebugFormat("{0} table already exists, skipping...", tablename);
                    return true;
                }
                else
                {
                    _log.DebugFormat("{0} table not found, importing...", tablename);

                    var filenames = FileUtilities.FindFileNameInZipLike(destFilepath, "*.shp");
                    if ((filenames == null) || (filenames.Count == 0))
                    {
                        _log.ErrorFormat("Compressed file {0} didn't contain a shapefile!", destFilepath);
                        return false;
                    }

                    foreach (string filename in filenames)
                    {
                        string fullShapefilename = Path.Combine(this.ShapePath, filename);

                        ShapefileHelper.MakeCensusProjFile(fullShapefilename);
                        ShapefileHelper.ImportShapefile(conn, this.DbClient,
                            fullShapefilename, tablename, 4269);

                        //TODO: multiple shape files in one zip?
                        break;
                    }
                }
            }
            return true;
        }



        /// <summary>
        /// Checks to see if the database exists, and if not, initializes it with the provided inputs
        /// </summary>
        /// <returns></returns>
        public bool CheckDatabase()
        {
            _log.DebugFormat("Looking for local {0} database", this.State);

            if (!File.Exists(this.DBFilename))
            {
                _log.DebugFormat("Building local {0} database...", this.State);
            }

            if (!this.InitDatabase())
            {
                _log.Error("Building local {0} database... Failed!  There was a problem detected.");
                return false;
            }

            return (DbClient.TestDatabaseConnection());
        }

        

        public bool CreateGeographiesTable(DbConnection conn)
        {
            //create the table
            string createGeographyTableSQL = DataClient.GenerateTableSQLFromFields(DbConstants.TABLE_Geographies,  GeographyFileReader.Columns);
            DbClient.GetCommand(createGeographyTableSQL, conn).ExecuteNonQuery();

            //parse in the file
            string geographyFilename = GetLocalGeographyFileName();
            GeographyFileReader geoReader = new GeographyFileReader(geographyFilename);
            if (geoReader.HasFile)
            {
                _log.Debug("Importing Geographies File...");
                string tableSelect = string.Format("select * from {0}", DbConstants.TABLE_Geographies);
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

                    //this.StateFIPS = (table.Rows[0]["STATE"] as string);

                    adapter.Update(table);
                    table.AcceptChanges();
                }

                _log.Debug("Importing Geographies File... Done!");
                return true;
            }
            else
            {
                _log.Debug("Could not find geographies file, table could not be initialized!");
                return false;
            }
        }

        public bool CreateColumnMappingsTable(DbConnection conn)
        {
            //create table
            string createMappingsTableSql = DataClient.GenerateTableSQLFromFields(DbConstants.TABLE_ColumnMappings, SequenceFileReader.Columns);
            DbClient.GetCommand(createMappingsTableSql, conn).ExecuteNonQuery();

            string tableSelect = string.Format("select * from {0}", DbConstants.TABLE_ColumnMappings);
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
                        _log.ErrorFormat("Sequence File had no readable variables: {0} ", file);
                        continue;
                    }
                    else if (fileData.Tables.Count > 1)
                    {
                        _log.WarnFormat("Sequence File had multiple worksheets : {0} ", file);
                    }

                    DataTable firstWorksheet = fileData.Tables[0];
                    if (firstWorksheet.Rows == null || firstWorksheet.Rows.Count < 2)
                    {
                        _log.ErrorFormat("Sequence File didn't have enough rows: {0} ", file);
                        continue;
                    }
                    else if (firstWorksheet.Rows.Count > 2)
                    {
                        _log.WarnFormat("Sequence File had too many rows: {0} ", file);
                    }

                    //Expected values of row: FILEID,FILETYPE,STUSAB,CHARITER,SEQUENCE,LOGRECNO,...
                    DataRow columnIDRow = firstWorksheet.Rows[0];
                    DataRow columnNameRow = firstWorksheet.Rows[1];

                    if (columnIDRow.ItemArray == null || columnIDRow.ItemArray.Length < 7)
                    {
                        _log.Error("One of the sequence files had bad data, skipping\n\t" + file);
                        continue;
                    }

                    //Add data to database
                    for (int i = 6; i < columnIDRow.ItemArray.Length; i++)
                    {
                        //This file has _ separating Table Number and offset, everywhere else doesn't
                        string columnID = columnIDRow[i].ToString().Trim().Replace("_", "");
                        string columnName = (columnNameRow[i] as string);

                        //                         ixid, CENSUS_TABLE_ID, NAME, COLNO, SEQNO
                        var toAdd = new object[] { ixid++, columnID, columnName, i, seqNo };
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
                {
                    _log.Error("Could not read any of the sequence files!");
                    return false;
                }

                _log.Debug("Importing Sequence Files... Done!");
                return true;
            }
            else
            {
                _log.Error("Could not find column mappings directory file, table not initialized!");
                return false;
            }
        }

        ///// <summary>
        ///// Given a csv file containing a list of TABLEIDs and optional Names, create a table from it
        ///// </summary>
        ///// <param name="conn"></param>
        ///// <param name="filename"></param>
        //public void CreateDesiredColumnsTable(DbConnection conn)
        //{

        //}


        /// <summary>
        /// Call this anyway, should only execute code inside safe blocks
        /// </summary>
        /// <returns></returns>
        protected bool InitDatabase()
        {
            if (!DbClient.TestDatabaseConnection())
            {
                _log.Error("InitDatabase failed: Unable to connect to database");
                return false;
            }

            using (var conn = DbClient.GetConnection())
            {
                _log.Debug("Checking for geographies table...");
                if (!DataClient.HasTable(conn, DbClient, "geographies"))
                {
                    if (!CreateGeographiesTable(conn))
                    {
                        //error message is in guard function
                        return false;
                    }
                }

                _log.Debug("Checking for columnMappings table...");
                if (!DataClient.HasTable(conn, DbClient, "columnMappings"))
                {
                    if (!CreateColumnMappingsTable(conn))
                    {
                        //error message is in guard function
                        return false;
                    }
                }
            }


            return true;
        }











        /// <summary>
        /// Returns all the variable names in the columnMappings table
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllSequenceVariables()
        {
            using (var conn = DbClient.GetConnection())
            {
                return DataClient.GetMagicTable(conn, DbClient, "select CENSUS_TABLE_ID, COLNAME from columnMappings");
            }
        }


        public HashSet<string> GetFilteredLRUs(DbConnection conn)
        {
            _log.Debug("Filtering requested LRUs");
            HashSet<string> results = new HashSet<string>();
            if (!string.IsNullOrEmpty(this.SummaryLevel))
            {
                using (var cmd = DbClient.GetCommand("select LOGRECNO from geographies where SUMLEVEL = @sum", conn))
                {
                    DbClient.AddParameter(cmd, "sum", this.SummaryLevel);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string lrn = reader.GetString(0);
                            results.Add(lrn);
                        }
                    }
                }
            }
            //else if (!string.IsNullOrEmpty(this.WKTFilterFilename))
            //{
            //    //TODO: implement
            //    _log.Warn("Not Yet Implemented");
            //}
            return results;
        }


        /// <summary>
        /// Currently rebuilds the requested variables table every time
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public DataTable GetRequestedVariables(DbConnection conn)
        {
            _log.Debug("Processing requested variables... ");

            DataTable dt = null;
            if (!string.IsNullOrEmpty(DesiredVariablesFilename))
            {
                DesiredColumnsReader fileReader = new DesiredColumnsReader();
                bool importSucceeded = fileReader.ImportDesiredVariables(conn, DbClient,
                    this.DesiredVariablesFilename, DbConstants.TABLE_DesiredColumns);

                if (importSucceeded)
                {
                    _log.DebugFormat("Variable file {0} imported successfully", this.DesiredVariablesFilename);
                    string getRequestedVariablesSQL = string.Format(
                      @"SELECT    columnMappings.CENSUS_TABLE_ID, 
                                  columnMappings.COLNO, 
                                  columnMappings.SEQNO, 
                                  {0}.CUSTOM_COLUMN_NAME AS COLNAME 
                        FROM      columnMappings, {0} 
                        WHERE     columnMappings.CENSUS_TABLE_ID = {0}.CENSUS_TABLE_ID;",
                        DbConstants.TABLE_DesiredColumns);

                    dt = DataClient.GetMagicTable(conn, DbClient, getRequestedVariablesSQL);
                    if (dt == null || dt.Rows == null || dt.Rows.Count == 0)
                    {
                        dt = null;
                    }
                }
                else
                {
                    _log.Warn("Processing requested variables... Failed!");
                    return null;
                }
            }

            _log.Debug("Processing requested variables... Done!");
            return dt;
        }


        //public List<IGeometry> GetFilteringGeometries()
        //{
        //    List<IGeometry> results = null;
        //    if (!string.IsNullOrEmpty(this.ExportFilterFilename))
        //    {
        //        if (!File.Exists(this.ExportFilterFilename))
        //        {
        //            _log.Error("Provided WKT file doesn't exist");
        //            return null;
        //        }

        //        WKTReader reader = new WKTReader(ShapefileHelper.GetGeomFactory());
        //        string[] lines = File.ReadAllLines(this.ExportFilterFilename);
        //        results = new List<IGeometry>(lines.Length);

        //        foreach (string line in lines)
        //        {
        //            if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("//"))
        //                continue;

        //            results.Add(reader.Read(line));
        //        }
        //    }
        //    return results;
        //}

        public List<IGeometry> GetFilteringGeometries(string filename, ICoordinateSystem outputCrs)
        {
            if ((string.IsNullOrEmpty(filename)) || (!File.Exists(filename)))
            {
                _log.ErrorFormat("GetFilteringGeometries failed, shapefile is empty or does not exist {0} ", filename);
                return null;
            }

            //inputCrs should be valid if LoadShapefile returns true
            ICoordinateSystem inputCrs;
            if (!ShapefileHelper.LoadShapefile(filename, Path.GetFileNameWithoutExtension(filename), DbClient, out inputCrs))
            {
                _log.Error("Could not load filtering geometries!");
                return null;
            }

            string tableName = Path.GetFileNameWithoutExtension(filename);
            string sqlcmd = "SELECT AsBinary(Geometry) AS Geometry FROM " + tableName;
            var wholeShapeTable = DataClient.GetMagicTable(DbClient.GetConnection(), DbClient, sqlcmd);
            List<IGeometry> results = new List<IGeometry>(wholeShapeTable.Rows.Count);
            ICoordinateTransformation reprojector = null;
            if (outputCrs != null)
            {
                reprojector = Utilities.BuildTransformationObject(inputCrs, outputCrs);
            }

            GisSharpBlog.NetTopologySuite.IO.WKBReader binReader = new WKBReader(
                new GeometryFactory());

            foreach (DataRow row in wholeShapeTable.Rows)
            {
                byte[] geomBytes = (byte[])row["Geometry"];
                IGeometry geom = binReader.Read(geomBytes);
                if (geom == null || geom.Dimension != Dimensions.Surface)
                {
                    continue;
                }

                if (reprojector != null)
                {
                    geom = Utilities.ReprojectGeometry(geom, reprojector);
                }

                results.Add(geom);
            }

            //Cleanup so we don't store lots of crap in database
            sqlcmd = "DROP TABLE " + tableName;
            var droptable = DbClient.GetCommand(sqlcmd);
            droptable.ExecuteNonQuery();

            if (results.Count == 0)
            {
                _log.Error("Could not use shapefile to create filter because no two dimensional geometries were found");
                return null;
            }


            return results;
        }



        /// <summary>
        /// Gets the filtered LogicalRecordNumbers, and the requested variables, and builds a table of their crossjoin
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
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


                    //gets a list of the LRUs we want
                    HashSet<string> requestedLRNs = GetFilteredLRUs(conn);

                    //gets all the variable mapping information we need
                    DataTable reqVariablesDT = GetRequestedVariables(conn);
                    if ((reqVariablesDT == null) || (reqVariablesDT.Rows.Count == 0))
                    {
                        _log.Warn("No variables requested");
                        return false;
                    }


                    DataTable newTable = new DataTable();
                    newTable.Columns.Add("LOGRECNO", typeof(string));
                    Dictionary<string, DataRow> rowsByLRN = new Dictionary<string, DataRow>(requestedLRNs.Count);                                        
                    foreach (var id in requestedLRNs)
                    {
                        var row = newTable.NewRow();
                        row[0] = id;
                        newTable.Rows.Add(row);

                        rowsByLRN[id] = row;
                    }

                    bool shouldNotParseErrorValues = PreserveJam;
                    
                    _log.Debug("Importing Columns");
                    int varNum = 0;
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

                        var errorFile = Directory.GetFiles(this.CurrentDataPath, "m*" + sequenceNo.ToString("0000") + "000.txt");    //0001000
                        if ((errorFile == null) || (errorFile.Length == 0))
                        {
                            _log.DebugFormat("Couldn't find error margin file {0}", sequenceNo);
                        }

                        //These next two if statement checks should probably be removed once
                        //#19869 is resolved
                        //Until that case is resolved, we can't guarantee rows in reqVariablesDT will be
                        //unique so they should stay.

                        //TODO: alternate column naming?
                        string newColumnName = variableRow["COLNAME"] as string;
                        if (newTable.Columns.Contains(newColumnName))
                        {
                            newColumnName = newColumnName + varNum;
                        }
                        newTable.Columns.Add(newColumnName, typeof(double));

                        //this really ought to be unique.
                        string newErrorMarginColumnName = "m" + newColumnName;
                        if (newTable.Columns.Contains(newErrorMarginColumnName))
                        {
                            newErrorMarginColumnName = newErrorMarginColumnName + varNum;
                        }

                        if (shouldNotParseErrorValues)
                        {
                            newTable.Columns.Add(newErrorMarginColumnName, typeof(string));
                        }
                        else
                        {
                            newTable.Columns.Add(newErrorMarginColumnName, typeof(double));
                        }
                        


                        _log.DebugFormat("Importing {0}...", newColumnName);
                        int columnIDX = Utilities.GetAs<int>(variableRow["COLNO"] as string, -1);



                        CommaSeparatedValueReader reader = new CommaSeparatedValueReader(seqFile[0], false);
                        foreach (List<string> values in reader)
                        {
                            string lrn = values[5];
                            if (!requestedLRNs.Contains(lrn))
                                continue;

                            if (columnIDX < values.Count)
                            {
                                double val = Utilities.GetAs<double>(values[columnIDX], double.NaN);
                                if (!double.IsNaN(val))
                                {
                                    rowsByLRN[lrn][newColumnName] = val;
                                }
                            }
                        }
                        reader.Close();

                        //these error files better have the exact same format!
                        reader = new CommaSeparatedValueReader(errorFile[0], false);
                        foreach (List<string> values in reader)
                        {
                            string lrn = values[5];
                            if (!requestedLRNs.Contains(lrn))
                                continue;

                            if (columnIDX < values.Count)
                            {
                                if (shouldNotParseErrorValues)
                                {
                                    rowsByLRN[lrn][newErrorMarginColumnName] = values[columnIDX];
                                }
                                else
                                {
                                    double val = Utilities.GetAs<double>(values[columnIDX], double.NaN);
                                    if (!double.IsNaN(val))
                                    {
                                        rowsByLRN[lrn][newErrorMarginColumnName] = val;
                                    }
                                    rowsByLRN[lrn][newErrorMarginColumnName] = val;
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


        protected Dictionary<string, DataRow> GetShapeRowsByLOGRECNO(DbConnection conn)
        {
            ////shapefile
            //if (!DataClient.HasTable(conn, DbClient, "shapetable"))
            //{
            //    this.CreateShapefileTable(conn, "shapetable");
            //}

            //really, we should be able to do this in one SQL statement, but for whatever reason, 
            //sqlite doesn't want to let us

            //something like this:
            //            string shapeSQL = @"
            //select s.pkuid, s.Geometry, g.LOGRECNO from shapetable s, geographies g 
            //where cast(g.COUNTY as integer) = cast(s.COUNTY as integer)
            //and g.TRACT like s.TRACT || '00'
            //and cast(g.BLKGRP as integer) = cast(s.BLKGROUP as integer) ";

            var temp = DataClient.GetMagicTable(conn, DbClient, "select * from census_tracts"); // where logrecno like '%883%'
            string geomSQL = "select LOGRECNO, trim(COUNTY) as county, trim(TRACT) as tract, trim(BLKGRP) as blkgroup, GEOID from geographies order by county, tract, blkgrp ";

            string shapeSQL = string.Empty;
            switch (this.SummaryLevel)
            {
                //census_regions
                //census_divisions
                //states
                //counties
                case "060":
                    //TODO: add subdivisions
                    shapeSQL = "select *, '' as GEOID from county_subdivisions ";
                    break;
                case "140":
                    //tracts
                    shapeSQL = "select trim(COUNTY) as county, trim(TRACT) as tract, '' as blkgroup, AsBinary(Geometry) as Geometry, '' as GEOID from census_tracts ";
                    break;
                case "150":
                    //block groups
                    shapeSQL = "select trim(COUNTY) as county, trim(TRACT) as tract, trim(BLKGROUP) as blkgroup, AsBinary(Geometry) as Geometry, '' as GEOID from census_blockgroups ";
                    break;
                default:
                    shapeSQL = "select trim(COUNTY) as county, trim(TRACT) as tract, trim(BLKGROUP) as blkgroup, AsBinary(Geometry) as Geometry, '' as GEOID from census_blockgroups ";
                    _log.Error("No summary level listed, choosing blockgroups");
                    break;
            }
            shapeSQL += "order by county, tract, blkgroup";

            var wholeGeomTable = DataClient.GetMagicTable(conn, DbClient, geomSQL);
            var wholeShapeTable = DataClient.GetMagicTable(conn, DbClient, shapeSQL);


            var geomKeys = new Dictionary<string, DataRow>();
            foreach (DataRow row in wholeGeomTable.Rows)
            {
                string county = Utilities.GetAs<string>(row["COUNTY"], "-1");
                string tract = Utilities.GetAs<string>(row["TRACT"], "-1");
                string blkgroup = Utilities.GetAs<string>(row["BLKGROUP"], "-1");
                if (tract.Trim().Length != 6)
                    tract += "00";

                string key = string.Format("{0}_{1}_{2}", county, tract, blkgroup);
                geomKeys[key] = row;
            }

            var dict = new Dictionary<string, DataRow>();
            GisSharpBlog.NetTopologySuite.IO.WKBReader binReader = new WKBReader(
                    ShapefileHelper.GetGeomFactory());
            GisSharpBlog.NetTopologySuite.IO.WKBWriter binWriter = new WKBWriter();
            foreach (DataRow row in wholeShapeTable.Rows)
            {
                string county = Utilities.GetAs<string>(row["COUNTY"], "-1");
                string tract = Utilities.GetAs<string>(row["TRACT"], "-1");
                string blkgroup = Utilities.GetAs<string>(row["BLKGROUP"], "-1");
                if (tract.Trim().Length != 6)
                    tract += "00";

                string key = string.Format("{0}_{1}_{2}", county, tract, blkgroup);
                if (geomKeys.ContainsKey(key))
                {
                    string logrecno = geomKeys[key]["LOGRECNO"] as string;
                    row["GEOID"] = geomKeys[key]["GEOID"];

                    if (dict.ContainsKey(logrecno))
                    {
                        /* In theory, there should be a one-to-one relationship between logrecno and Geometry.
                         * In practice, there are a small number of geometries that are discontiguous or self-
                         * intersecting.  Because the geometry shapefile only uses single polygons and not
                         * multipolygons, they solve this problem by having duplicate rows in the geometry 
                         * shapefile with the same county, tract, and blkgroup numbers, one for each part of
                         * the multipolygon.  This is undocumented AFAIK, but can be seen in several places (see
                         * logrecno 0014948 for PA for example).  To account for this, we union geometries 
                         * together whenever we encounter duplicates and inform the user, so we don't end up
                         * with missing geometries in the output.
                         * 
                         * As this link indicates, tracts 9400 - 9499 are especially prone to this phenomenon:
                         * http://www.census.gov/geo/www/cob/tr_metadata.html
                         */
                        
                        _log.DebugFormat("Duplicate records encountered for logical record number {0} (County:{1}, Tract:{2}, Block Group:{3}).", logrecno, county, tract, blkgroup);
                        _log.DebugFormat("Attempting to merge geometries for duplicates together.");
                        try
                        {
                            byte[] geomBytes = (byte[])row["Geometry"];
                            IGeometry geomToAdd = binReader.Read(geomBytes);
                            geomBytes = (byte[])dict[logrecno]["Geometry"];
                            IGeometry currentGeom = binReader.Read(geomBytes);
                            geomBytes = binWriter.Write(currentGeom.Union(geomToAdd));
                            row["Geometry"] = geomBytes;    //saved to dict at end of current if clause
                            _log.Debug("Geometry merge succeeded!  Please double check all features in the output that match the above logrecno for consistency.");
                        }
                        catch (Exception)
                        {
                            _log.Error("Geometry merge failed; only one geometry will be used!  Output may be missing geometries!");
                        }
                    }

                    dict[logrecno] = row;
                }
                else
                {
                    _log.DebugFormat("Couldn't find a geometry matching County:{0}, Tract:{1}, Block Group:{2}", 
                        county, tract, blkgroup);
                }
            }

            return dict;
        }


        public bool IsIncluded(IGeometry geom, List<IGeometry> filteringGeoms)
        {
            if ((filteringGeoms == null) || (filteringGeoms.Count == 0))
            {
                return true;
            }
            else
            {
                for (int i = 0; i < filteringGeoms.Count; i++)
                {
                    var filt = filteringGeoms[i];
                    for (int g = 0; g < filt.NumGeometries; g++)
                    {
                        var fg = filt.GetGeometryN(g);
                        if (fg.Intersects(geom))
                        {
                            //double commonArea = fg.Intersection(geom).Area;
                            //if(commonArea > 0.01 * fg.Area || commonArea > 0.01 * geom.Area)
                                return true;
                        }
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Given a table, returns a list of features to export.  Assumes table geometries are in WGS84 
        /// </summary>
        /// <param name="tableName">Name of the sqlite table</param>
        /// <param name="spatialFilter">Should we load and use a spatial filter?</param>
        /// <returns></returns>
        public List<Feature> GetShapeFeaturesToExport(string tableName, bool spatialFilter)
        {
            using (var conn = DbClient.GetConnection())
            {
                Dictionary<string, DataRow> shapeDict = GetShapeRowsByLOGRECNO(conn);
                var variablesDT = DataClient.GetMagicTable(conn, DbClient, "select * from " + tableName);
                if ((variablesDT == null) || (variablesDT.Rows.Count == 0))
                {
                    _log.Warn("Nothing to export, data table is empty");
                    return null;
                }

                bool shouldReproject = (!string.IsNullOrEmpty(this.OutputProjectionFilename));
                ICoordinateTransformation reprojector = null;
                ICoordinateSystem destCRS = null;
                if (!string.IsNullOrEmpty(this.OutputProjectionFilename))
                {
                    destCRS = Utilities.GetCoordinateSystemByWKTFile(this.OutputProjectionFilename);
                    reprojector = Utilities.BuildTransformationObject(GeographicCoordinateSystem.WGS84, destCRS);

                    //Reproject everything in this file to the requested projection...                    
                    //exportFeatures = Utilities.ReprojectFeaturesTo(exportFeatures, this.OutputProjectionFilename);

                    //THESE MUST BE PROVIDED ALREADY PROJECTED!
                    //filteringGeoms = Utilities.ReprojectFeaturesTo(filteringGeoms, this.OutputProjectionFilename);
                }

                //TODO:
                bool shouldAddMissingShapes = this.IncludeEmptyGridCells;
                HashSet<string> allShapeIDs = new HashSet<string>(shapeDict.Keys);

                //Build hashset, remove by shapeid as shapes are added,
                //go through and add missing shapes if 'shouldAddMissingShapes' is set...
                bool variablesHaveGeoID = variablesDT.Columns.Contains("GEOID");

                List<IGeometry> filteringGeoms = null;
                if (!string.IsNullOrEmpty(this.ExportFilterFilename))
                {
                    filteringGeoms = (spatialFilter) ? GetFilteringGeometries(this.ExportFilterFilename, destCRS) : null;
                    //Die if we're supposed to have a filter but don't
                    if (spatialFilter && filteringGeoms == null)
                    {
                        return null;
                    }
                }

                GisSharpBlog.NetTopologySuite.IO.WKBReader binReader = new WKBReader(
                    ShapefileHelper.GetGeomFactory());
                var features = new List<Feature>(variablesDT.Rows.Count);

                foreach (DataRow row in variablesDT.Rows)
                {
                    var id = row["LOGRECNO"] as string;
                    if (!shapeDict.ContainsKey(id))
                        continue;

                    allShapeIDs.Remove(id);

                    AttributesTable t = new AttributesTable();
                    foreach (DataColumn col in variablesDT.Columns)
                    {
                        //produces more sane results.
                        t.AddAttribute(
                            Utilities.EnsureMaxLength(col.ColumnName, 10),
                            Utilities.GetAsType(col.DataType, row[col.Ordinal], null)
                            );
                    }

                    byte[] geomBytes = (byte[])shapeDict[id]["Geometry"];
                    IGeometry geom = binReader.Read(geomBytes);
                    if (geom == null)
                    {
                        _log.WarnFormat("Geometry for LRN {0} was empty!", id);
                        continue;
                    }

                    if (shouldReproject)
                    {
                        geom = Utilities.ReprojectGeometry(geom, reprojector);
                    }

                    if (spatialFilter)
                    {
                        if (!IsIncluded(geom, filteringGeoms))
                        {
                            continue;
                        }
                    }

                    if (!variablesHaveGeoID)
                    {
                        t.AddAttribute("GEOID", shapeDict[id]["GEOID"]);
                    }

                    if (this.AddStrippedGEOIDcolumn)
                    {
                        t.AddAttribute("GEOID_STRP", (t["GEOID"] as string).Replace(Settings.GeoidPrefix, ""));
                    }

                    features.Add(new Feature(geom, t));
                }

                if (shouldAddMissingShapes)
                {
                    _log.DebugFormat("Adding {0} missing shapes", allShapeIDs.Count);
                    foreach (string id in allShapeIDs)
                    {
                        byte[] geomBytes = (byte[])shapeDict[id]["Geometry"];
                        IGeometry geom = binReader.Read(geomBytes);
                        if (geom == null)
                        {
                            _log.WarnFormat("Geometry for LRN {0} was empty!", id);
                            continue;
                        }

                        if (shouldReproject)
                        {
                            geom = Utilities.ReprojectGeometry(geom, reprojector);
                        }

                        if (spatialFilter)
                        {
                            if (!IsIncluded(geom, filteringGeoms))
                            {
                                continue;
                            }
                        }

                        AttributesTable t = new AttributesTable();
                        foreach (DataColumn col in variablesDT.Columns)
                        {
                            //produces more sane results.
                            t.AddAttribute(Utilities.EnsureMaxLength(col.ColumnName, 10), null);
                        }

                        if (!variablesHaveGeoID)
                        {
                            t.AddAttribute("GEOID", shapeDict[id]["GEOID"]);
                        }

                        if (this.AddStrippedGEOIDcolumn)
                        {
                            t.AddAttribute("GEOID_STRP", (t["GEOID"] as string).Replace(Settings.GeoidPrefix, ""));
                        }
                        t["LOGRECNO"] = id;
                        features.Add(new Feature(geom, t));
                    }
                }

                return features;
            }
        }

        /// <summary>
        /// Retrieves the contents of the specified table, and writes them to a shapefile
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool ExportShapefile(string tableName)
        {
            try
            {
                _log.DebugFormat("Retrieving contents of job {0}", tableName);

                var exportFeatures = GetShapeFeaturesToExport(tableName, true);
                if ((exportFeatures == null) || (exportFeatures.Count == 0))
                {
                    _log.ErrorFormat("Export of Job \"{0}\" failed, no features to export!", tableName);
                    return false;
                }

                DbaseFileHeader header = null;
                using (var conn = DbClient.GetConnection())
                {
                    //Dictionary<string, DataRow> shapeDict = GetShapeRowsByLOGRECNO(conn);
                    var variablesDT = DataClient.GetMagicTable(conn, DbClient, "select * from " + tableName + " where 0 = 1 ");
                    header = ShapefileHelper.SetupHeader(variablesDT);
                }

                ShapefileHelper.AddColumn(header, "GEOID", typeof(string));
                if (this.AddStrippedGEOIDcolumn)
                {
                    ShapefileHelper.AddColumn(header, "GEOID_STRP", typeof(string));
                }

                header.NumRecords = exportFeatures.Count;

                string newShapefilename = Path.Combine(Environment.CurrentDirectory, tableName);
                if (!string.IsNullOrEmpty(OutputFolder))
                {
                    newShapefilename = Path.Combine(OutputFolder, tableName);
                }
                string destPath = Path.GetDirectoryName(newShapefilename);
                FileUtilities.SafePathEnsure(destPath);


                var writer = new ShapefileDataWriter(newShapefilename, ShapefileHelper.GetGeomFactory());
                writer.Header = header;

                if (!string.IsNullOrEmpty(this.OutputProjectionFilename))
                {
                    ShapefileHelper.MakeOutputProjFile(this.OutputProjectionFilename, newShapefilename);
                }
                else
                {
                    ShapefileHelper.MakeCensusProjFile(newShapefilename);
                }
                writer.Write(exportFeatures);

                _log.Debug("Shapefile exported successfully");
                return true;
            }
            catch (FileNotFoundException notFound)
            {
                _log.Error("A needed file couldn't be found: " + notFound.FileName);
                _log.Fatal("The export cannot continue.  Exiting...");
                Environment.Exit(-1);
            }
            catch (Exception ex)
            {
                _log.Error("Error while exporting shapefile", ex);
            }
            return false;
        }




        /// <summary>
        /// Retrieves the contents of the specified table, and writes them to a shapefile
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool ExportGriddedShapefile(string tableName)
        {
            try
            {
                _log.DebugFormat("Retrieving contents of job {0}", tableName);
                //don't filter out geometries, we'll do that at the cell level
                var exportFeatures = GetShapeFeaturesToExport(tableName, false);
                if ((exportFeatures == null) || (exportFeatures.Count == 0))
                {
                    _log.ErrorFormat("Export of Job \"{0}\" failed, no features to export!", tableName);
                    return false;
                }

                ICoordinateSystem outputCrs = GeographicCoordinateSystem.WGS84;
                if (!string.IsNullOrEmpty(this.OutputProjectionFilename))
                {
                    outputCrs = Utilities.GetCoordinateSystemByWKTFile(this.OutputProjectionFilename);
                }

                //if we need to reproject:
                List<IGeometry> filteringGeoms = GetFilteringGeometries(this.ExportFilterFilename, outputCrs);         

                //put everything into a basic spatial index
                Envelope env = new Envelope();
                var index = new GisSharpBlog.NetTopologySuite.Index.Strtree.STRtree();
                for (int i = 0; i < exportFeatures.Count; i++)
                {
                    Feature f = exportFeatures[i];
                    index.Insert(f.Geometry.EnvelopeInternal, f);
                    env.ExpandToInclude(f.Geometry.EnvelopeInternal);
                }
                index.Build();



                //adjust envelope to only scan area inside filtering geometries                
                if (!string.IsNullOrEmpty(this.GridEnvelopeFilename))
                {
                    //a specified envelope file overrides the envelope of the filtering geometries
                    env = GetGridEnvelope();
                }
                else if ((filteringGeoms != null) && (filteringGeoms.Count > 0))
                {
                    //in the absence ... //TODO: finish--
                    env = new Envelope();
                    for (int i = 0; i < filteringGeoms.Count; i++)
                    {
                        env.ExpandToInclude(filteringGeoms[i].EnvelopeInternal);
                    }
                }



                //progress tracking
                DateTime start = DateTime.Now, lastCheck = DateTime.Now;
                int lastProgress = 0;


                var features = new List<Feature>(exportFeatures.Count);
                //var cellStepPoint = Utilities.GetCellFeetForProjection(GridCellWidthFeet, GridCellHeightFeet);
                //double cellWidth = cellStepPoint.X;
                //double cellHeight = cellStepPoint.Y;
                
                double cellWidth = GridCellWidth;
                double cellHeight = GridCellHeight;
                bool discardEmptyGridCells = !IncludeEmptyGridCells;
                

                int numRows = (int)Math.Ceiling(env.Height / cellHeight);
                int numCols = (int)Math.Ceiling(env.Width / cellWidth);
                int expectedCells = numRows * numCols;

                if (expectedCells > 1000000)
                {
                    _log.Warn("**********************");
                    _log.Warn("Your selected area will produce a shapefile with over a million cells, is that a good idea?");
                    _log.WarnFormat("Area of {0}, Expected Cell Count of {1}", env.Area, expectedCells);
                    _log.Warn("**********************");
                }

                DbaseFileHeader header = null;
                using (var conn = DbClient.GetConnection())
                {
                    //Dictionary<string, DataRow> shapeDict = GetShapeRowsByLOGRECNO(conn);
                    var variablesDT = DataClient.GetMagicTable(conn, DbClient, "select * from " + tableName + " where 0 = 1 ");
                    header = ShapefileHelper.SetupHeader(variablesDT);
                }
                ShapefileHelper.AddColumn(header, "CELLID", typeof(string));
                ShapefileHelper.AddColumn(header, "GEOID", typeof(string));
                if (this.AddStrippedGEOIDcolumn)
                {
                    ShapefileHelper.AddColumn(header, "GEOID_STRP", typeof(string));
                }

                int cellCount = 0;
                int xidx = 0;
                for (double x = env.MinX; x < env.MaxX; x += cellWidth)
                {
                    xidx++;
                    int yidx = 0;
                    for (double y = env.MinY; y < env.MaxY; y += cellHeight)
                    {
                        yidx++;
                        cellCount++;
                        string cellID = string.Format("{0}_{1}", xidx, yidx);

                        Envelope cellEnv = new Envelope(x, x + cellWidth, y, y + cellHeight);
                        IGeometry cellCenter = new Point(cellEnv.Centre);
                        IGeometry cellGeom = Utilities.IEnvToIGeometry(cellEnv);

                        Feature found = null;
                        IList mightMatch = index.Query(cellGeom.EnvelopeInternal);
                        foreach (Feature f in mightMatch)
                        {
                            if (f.Geometry.Contains(cellCenter))
                            {
                                found = f;
                                break;
                            }
                        }

                        if ((found == null) && (discardEmptyGridCells))
                        {
                            //_log.DebugFormat("No feature found for cell {0}", cellID);
                            continue;
                        }


                        //if we have filtering geometries, skip a cell if it isn't included
                        if (!IsIncluded(cellGeom, filteringGeoms))
                        {
                            continue;
                        }

                        if ((cellCount % 1000) == 0)
                        {
                            int step = (int)((((double)cellCount) / ((double)expectedCells)) * 100.0);
                            TimeSpan elapsed = (DateTime.Now - lastCheck);
                            if ((step != lastProgress) && (elapsed.TotalSeconds > 1))
                            {
                                _log.DebugFormat("{0:###.##}% complete, {1:#0.0#} seconds, {2} built, {3} checked, {4} left",
                                   step, (DateTime.Now - start).TotalSeconds,
                                   features.Count,
                                   cellCount,
                                   expectedCells - cellCount
                                   );
                                lastCheck = DateTime.Now;
                                lastProgress = step;
                            }
                        }

                        //this is a lot of work just to add an id...
                        AttributesTable attribs = new AttributesTable();
                        if (found != null)
                        {
                            //if (!found.Attributes.GetNames().Contains("GEOID"))
                            //    throw new Exception("GEOID NOT FOUND!!!!");
                            foreach (string name in found.Attributes.GetNames())
                            {
                                attribs.AddAttribute(name, found.Attributes[name]);
                            }
                            attribs.AddAttribute("CELLID", cellID);
                        }
                        else
                        {
                            foreach (var field in header.Fields)
                            {
                                attribs.AddAttribute(field.Name, null);
                            }
                            attribs["CELLID"] = cellID;
                        }
                        
                        features.Add(new Feature(cellGeom, attribs));
                    }
                }
                _log.Debug("Done building cells, Saving Shapefile...");
                header.NumRecords = features.Count;

                if (features.Count == 0)
                {
                    _log.Error("No features found, exiting!");
                    return false;
                }

                string newShapefilename = Path.Combine(Environment.CurrentDirectory, tableName);
                if (!string.IsNullOrEmpty(OutputFolder))
                {
                    newShapefilename = Path.Combine(OutputFolder, tableName);
                }

                var writer = new ShapefileDataWriter(newShapefilename, ShapefileHelper.GetGeomFactory());
                writer.Header = header;
                writer.Write(features);


                if (!string.IsNullOrEmpty(this.OutputProjectionFilename))
                {
                    //Reproject everything in this file to the requested projection...                                        
                    ShapefileHelper.MakeOutputProjFile(this.OutputProjectionFilename, newShapefilename);
                }
                else
                {
                    ShapefileHelper.MakeCensusProjFile(newShapefilename);
                }

                _log.Debug("Done! Shapefile exported successfully");
                return true;
            }
            catch (FileNotFoundException notFound)
            {
                _log.Error("A needed file couldn't be found: " + notFound.FileName);
                _log.Fatal("The export cannot continue.  Exiting...");
                Environment.Exit(-1);
            }
            catch (Exception ex)
            {
                _log.Error("Error while exporting shapefile", ex);
            }
            return false;
        }

        public Envelope GetGridEnvelope()
        {
            Envelope env = null;
            if (!string.IsNullOrEmpty(GridEnvelopeFilename))
            {
                string[] lines = File.ReadAllLines(GridEnvelopeFilename);
                foreach (string line in lines)
                {
                    if (line.StartsWith("#") || line.StartsWith("//"))
                        continue;

                    string[] chunks = line.Split(" ,:".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    env = new Envelope(
                        Utilities.GetAs<double>(chunks[0], -1),     //env.MinX
                        Utilities.GetAs<double>(chunks[2], -1),  //env.MaxX
                        Utilities.GetAs<double>(chunks[1], -1),   //env.MinY
                        Utilities.GetAs<double>(chunks[3], -1)    //env.MaxY
                    );
                    break;
                }
            }
            
            return env;
        }









        public void Dispose()
        {
            if (DbClient != null)
            {
                //Don't need this hanging around...
                DbClient.
                    GetCommand(string.Format("DROP TABLE IF EXISTS {0};", DbConstants.TABLE_DesiredColumns)).
                    ExecuteNonQuery();
            }
        }

        public void SetGridParam(string gridArgs)
        {
            if (string.IsNullOrEmpty(gridArgs))
                return;

            //"100 100", "100x100", "200_300", "100:100"
            var chunks = gridArgs.Split("x_:, ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (chunks.Length > 0)
            {
                //only provided the one, make em square!
                this.GridCellWidth = Utilities.GetAs<double>(chunks[0], 10000);
                this.GridCellHeight = this.GridCellWidth;
            }
            if (chunks.Length > 1)
            {
                //they provided both
                this.GridCellHeight = Utilities.GetAs<double>(chunks[0], 10000);
            }

            _log.DebugFormat("Set Grid cell width to {0}, and height to {1} ",
                this.GridCellWidth,
                this.GridCellHeight);
        }



    }
}

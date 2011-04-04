﻿using System;
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
    public class AcsDataManager : IDisposable
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AcsState State = AcsState.None;

        public const string DesiredColumnsTableName = "desiredColumns";

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
        /// Downloads the SHAPE FILEs, must be run before initializing the database!  
        /// Since this will be imported into the database!
        /// </summary>
        /// <returns></returns>
        public bool CheckShapefiles()
        {
            BoundaryLevels[] shapeFileLevels = new BoundaryLevels[] {
                BoundaryLevels.census_blockgroups,
                BoundaryLevels.census_tracts,
                BoundaryLevels.county_subdivisions,
                BoundaryLevels.zipthree,
                BoundaryLevels.zipfive,
                BoundaryLevels.counties
            };

            foreach (BoundaryLevels level in shapeFileLevels)
            {
                string url = ShapefileHelper.GetRemoteShapefileURL(level, this.StateFIPS);
                string localPath = Path.Combine(this.WorkingPath, Path.GetFileName(url));

                GetAndBuildShapefile(url, localPath, level.ToString(), level.ToString());
            }

            return true;
        }


        public bool GetAndBuildShapefile(string desiredUrl, string destFilepath, string niceName, string tablename)
        {
            _log.DebugFormat("Downloading shapefile of {0} for {1}", niceName, this.State);

            if (FileDownloader.GetFileByURL(desiredUrl, destFilepath))
            {
                _log.Debug("Download successful");

                if (FileLocator.ExpandZipFile(destFilepath, this.ShapePath))
                {
                    _log.DebugFormat("State {0} decompressed successfully", niceName);

                    var client = DbClient;
                    using (var conn = client.GetConnection())
                    {
                        if (!DataClient.HasTable(conn, client, tablename))
                        {
                            _log.DebugFormat("{0} table not found, importing...", tablename);

                            var filenames = FileUtilities.FindFileNameInZipLike(destFilepath, "*.shp");
                            if ((filenames != null) && (filenames.Count > 0))
                            {
                                foreach (string filename in filenames)
                                {
                                    ShapefileHelper.ImportShapefile(conn, this.DbClient,
                                        Path.Combine(this.ShapePath, filename),
                                        tablename);

                                    //TODO: multiple shape files in one zip?
                                    break;
                                }
                            }
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
                _log.ErrorFormat("An error was encountered while downloading {0}, exiting.", niceName);
            }
            return false;
        }



        ///// <summary>
        ///// Downloads the SHAPE FILE, must be run before initializing the database!  
        ///// Since this will be imported into the database!
        ///// </summary>
        ///// <returns></returns>
        //public bool CheckShapefile()
        //{
        //     _log.DebugFormat("Downloading shapefile of block groups for {0}", this.State);

        //    string desiredUrl = this.GetRemoteStateShapefileURL();
        //    string destFilepath = GetLocalBlockGroupShapefilename();

        //    if (FileDownloader.GetFileByURL(desiredUrl, destFilepath))
        //    {
        //        _log.Debug("Download successful");

        //        if (FileLocator.ExpandZipFile(destFilepath, this.ShapePath))
        //        {
        //            _log.Debug("State block group file decompressed successfully");

        //            var client = DbClient;
        //            using (var conn = client.GetConnection())
        //            {
        //                if (!DataClient.HasTable(conn, client, "shapetable"))
        //                {
        //                    _log.Debug("Shapefile table not found, importing...");
        //                    CreateShapefileTable(conn, "shapeblockgroups");
        //                }
        //            }

        //            return true;
        //        }
        //        else
        //        {
        //            _log.Error("Error during decompression, TODO: destroy directory");
        //        }
        //    }
        //    else
        //    {
        //        _log.Error("An error was encountered while downloading block group data, exiting.");
        //    }
        //    return false;
        //}

        public bool CreateShapefileTable(DbConnection conn, string tableName)
        {
            string filename = Directory.GetFiles(this.ShapePath, "*.shp")[0];
            return ShapefileHelper.ImportShapefile(conn, this.DbClient, filename, tableName);
        }

        //public string GetLocalShapefileName()
        //{
        //    var files = Directory.GetFiles(this.ShapePath, "bg*.shp");
        //    if ((files != null) && (files.Length > 0))
        //    {
        //        return Path.Combine(this.ShapePath, Path.GetFileNameWithoutExtension(files[0]));
        //    }
        //    return null;
        //}


        //public DataTable GetShapefileData()
        //{
        //    DataTable dt = null;
        //    try
        //    {
        //        string filename = GetLocalShapefileName();
        //        if (!string.IsNullOrEmpty(filename))
        //        {
        //            dt = Shapefile.CreateDataTable(filename, this.State.ToString(), ShapefileHelper.GetGeomFactory());
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.Error("Error opening shapefile", ex);
        //    }
        //
        //    return dt;
        //}




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
                    DataRow row = firstWorksheet.Rows[0];   
                    if (row.ItemArray == null || row.ItemArray.Length < 7)
                    {
                        _log.Error("One of the sequence files had bad data, skipping\n\t" + file);
                        continue;
                    }

                    //Add data to database
                    for (int i = 6; i < row.ItemArray.Length; i++)
                    {
                        //This file has _ separating Table Number and offset, everywhere else doesn't
                        string scrubbedTableId = row.ItemArray[i].ToString().Trim().Replace("_", "");
                        //                         ixid,   CENSUS_TABLE_ID,         COLNO, SEQNO
                        var toAdd = new object[] { ixid++, scrubbedTableId, i + 1, seqNo };
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
        public List<string> GetAllSequenceVariableTableIds()
        {
            List<string> variableNames = null;
            using (var conn = DbClient.GetConnection())
            {
                var dt = DataClient.GetMagicTable(conn, DbClient, "select CENSUS_TABLE_ID from columnMappings");
                variableNames = new List<string>(dt.Rows.Count);
                foreach (DataRow row in dt.Rows)
                {
                    variableNames.Add(row[0] as string);
                }
            }
            return variableNames;
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


        /// <summary>
        /// Currently rebuilds the requested variables table every time
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public DataTable GetRequestedVariables(DbConnection conn)
        {
            _log.Debug("Filtering requested variables");

            DataTable dt = null;
            if (!string.IsNullOrEmpty(IncludedVariableFile))
            {
                DesiredColumnsReader fileReader = new DesiredColumnsReader();
                if (fileReader.CreateTemporaryTable(conn, DbClient, this.IncludedVariableFile, AcsDataManager.DesiredColumnsTableName))
                {
                    _log.DebugFormat("Variable file {0} imported successfully", this.IncludedVariableFile);
                    string getRequestedVariablesSQL = string.Format(
                        @"SELECT columnMappings.CENSUS_TABLE_ID, columnMappings.COLNO, columnMappings.SEQNO, {0}.CUSTOM_COLUMN_NAME AS COLNAME 
                        FROM columnMappings, {0} 
                        WHERE columnMappings.CENSUS_TABLE_ID = {0}.CENSUS_TABLE_ID;",
                            AcsDataManager.DesiredColumnsTableName);

                    dt = DataClient.GetMagicTable(conn, DbClient, getRequestedVariablesSQL);
                }
                else
                {
                    _log.Warn("Unable to read/build requested variables list");
                }
            }

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


                    //gets a list of the LRUs we want
                    HashSet<string> requestedLRNs = GetFilteredLRUs(conn);

                    //gets all the variable mapping information we need
                    DataTable reqVariablesDT = GetRequestedVariables(conn);
                    if ((reqVariablesDT == null) || (reqVariablesDT.Rows.Count == 0))
                    {
                        _log.Fatal("No variables requested");
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


            string geomSQL = "select LOGRECNO, trim(COUNTY) as county, trim(TRACT) as tract, trim(BLKGRP) as blkgrp from geographies order by county, tract, blkgrp ";
            string shapeSQL = 
@"
select trim(COUNTY) as county, trim(TRACT) || '00' as tract, trim(BLKGROUP) as blkgroup, AsBinary(Geometry) as Geometry from census_blockgroups
UNION
select trim(COUNTY) as county, trim(TRACT) || '00' as tract, '' as blkgroup, AsBinary(Geometry) as Geometry from census_tracts 
order by county, tract, blkgroup";

            var wholeGeomTable = DataClient.GetMagicTable(conn, DbClient, geomSQL);
            var wholeShapeTable = DataClient.GetMagicTable(conn, DbClient, shapeSQL);

           
            var geomKeys = new Dictionary<string, DataRow>();
            foreach (DataRow row in wholeGeomTable.Rows)
            {
                string key = string.Format("{0}_{1}_{2}",
                    Utilities.GetAs<int>(row["COUNTY"], -1),
                    Utilities.GetAs<int>(row["TRACT"], -1),
                    Utilities.GetAs<int>(row["BLKGRP"], -1)
                );
                geomKeys[key] = row;
            }

            var dict = new Dictionary<string, DataRow>();
            foreach (DataRow row in wholeShapeTable.Rows)
            {
                string key = string.Format("{0}_{1}_{2}",
                   Utilities.GetAs<int>(row["COUNTY"], -1),
                   Utilities.GetAs<int>(row["TRACT"], -1),
                   Utilities.GetAs<int>(row["BLKGROUP"], -1)
                );

                if (geomKeys.ContainsKey(key))
                {
                    string logrecno = geomKeys[key]["LOGRECNO"] as string;
                    dict[logrecno] = row;
                }
            }

            return dict;
        }





        public bool ExportShapefile(string tableName)
        {
            try
            {
                //bool shouldRun = false;
                //if (!shouldRun)
                //{
                _log.Warn("**NOT YET COMPLETED**");
                //    return false;
                //}

                using (var conn = DbClient.GetConnection())
                {
                    Dictionary<string, DataRow> shapeDict = GetShapeRowsByLOGRECNO(conn);
                    var variablesDT = DataClient.GetMagicTable(conn, DbClient, "select * from " + tableName);
                    if ((variablesDT == null) || (variablesDT.Rows.Count == 0))
                    {
                        _log.Fatal("Nothing to export, data table is empty");
                        return false;
                    }


                    GisSharpBlog.NetTopologySuite.IO.WKBReader binReader = new WKBReader(ShapefileHelper.GetGeomFactory());
                    var features = new List<Feature>(variablesDT.Rows.Count);
                    var header = ShapefileHelper.SetupHeader(variablesDT);

                    foreach (DataRow row in variablesDT.Rows)
                    {
                        var id = row["LOGRECNO"] as string;
                        if (!shapeDict.ContainsKey(id))
                            continue;

                        AttributesTable t = new AttributesTable();
                        foreach (DataColumn col in variablesDT.Columns)
                        {
                            //produces more sane results.
                            //t.AddAttribute("col" + col.Ordinal, Utilities.GetAsType(col.DataType, row[col.Ordinal], null));

                            t.AddAttribute(col.ColumnName, Utilities.GetAsType(col.DataType, row[col.Ordinal], null));
                        }

                        byte[] geomBytes = (byte[])shapeDict[id]["Geometry"];
                        IGeometry geom = binReader.Read(geomBytes);
                        if (geom == null)
                        {
                            _log.WarnFormat("Geometry for LRN {0} was empty!", id);
                            continue;
                        }

                        features.Add(new Feature(geom, t));
                    }
                    header.NumRecords = features.Count;

                    string newShapefilename = Path.Combine(Environment.CurrentDirectory, tableName);
                    var writer = new ShapefileDataWriter(newShapefilename, ShapefileHelper.GetGeomFactory());
                    writer.Header = header;
                    writer.Write(features);                    
                }

                _log.Debug("Shapefile exported successfully...");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error while exporting shapefile", ex);
            }
            return false;
        }


        public void Dispose()
        {
            if (DbClient != null)
            {
                //Don't need this hanging around...
                DbClient.
                    GetCommand(string.Format("DROP TABLE IF EXISTS {0};", AcsDataManager.DesiredColumnsTableName)).
                    ExecuteNonQuery();
            }
        }
    }
}

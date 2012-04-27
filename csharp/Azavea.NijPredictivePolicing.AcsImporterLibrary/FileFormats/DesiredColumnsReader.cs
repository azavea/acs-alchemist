/*
  Copyright (c) 2012 Azavea, Inc.
 
  This file is part of ACS Alchemist.

  ACS Alchemist is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  ACS Alchemist is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with ACS Alchemist.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using Azavea.NijPredictivePolicing.Common.Data;
using System.Data.Common;
using Azavea.NijPredictivePolicing.Common.DB;
using Azavea.NijPredictivePolicing.Common;
using System.Data;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats
{
    /// <summary>
    /// Input should be a csv list of (CENSUS_TABLE_ID, Name) pairs, 
    /// where:
    /// CENSUS_TABLE_ID: is a foreign key into columnMappings.CENSUS_TABLE_ID
    /// Name: is an optional alias
    /// </summary>
    public class DesiredColumnsReader : CommaSeparatedValueReader
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        delegate string TableChecker(DataTable dt);
        
        public const string DropTableSQL = "DROP TABLE IF EXISTS \"{0}\";";
        public const string CreateTableSQL = 
            @"DROP TABLE IF EXISTS ""{0}""; CREATE TABLE ""{0}"" (
            CENSUS_TABLE_ID VARCHAR(32) NOT NULL PRIMARY KEY,
            CUSTOM_COLUMN_NAME VARCHAR(10));";

        /// <summary>
        /// Holds onto the name for the variables table
        /// </summary>
        public string tempTableName;


        public void RemoveTemporaryTable(DbConnection conn, IDataClient client)
        {
            client.GetCommand(string.Format(DropTableSQL, tempTableName), conn).ExecuteNonQuery();
        }

        /// <summary>
        /// Helper function for ImportDesiredVariables
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="line"></param>
        /// <param name="keys"></param>
        private void AddIntToDict(Dictionary<string, List<int>> dict, int line, params string[] keys)
        {
            if ((keys == null) || (keys.Length == 0))
                return;

            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                if (!dict.ContainsKey(key))
                    dict[key] = new List<int>();

                dict[key].Add(line);
            }
        }

        /// <summary>
        /// Helper function for ImportDesiredVariables
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="line"></param>
        /// <param name="keys"></param>
        private void IfSetAddIntToDict(HashSet<string> set, Dictionary<string, List<int>> dict, int line, params string[] keys)
        {
            if ((keys == null) || (keys.Length == 0))
                return;

            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];

                if (!set.Contains(key))
                {
                    continue;
                }

                if (!dict.ContainsKey(key))
                    dict[key] = new List<int>();

                dict[key].Add(line);
            }
        }


        protected DataTable ReadVariablesFile(string filename, DataTable dt)
        {
            int line = 0;

            try
            {
                if (string.IsNullOrEmpty(_filename))
                {
                    if (!this.LoadFile(filename))
                    {
                        _log.ErrorFormat("ImportDesiredVariables failed: the file couldn't be read");
                    }
                }
                foreach (List<string> row in this)
                {
                    line++;     //base 1

                    if ((row.Count < 1) || (string.IsNullOrEmpty(row[0])))
                    {
                        //skip blank lines
                        continue;
                    }
                    if (row.Count > 2)
                    {
                        _log.WarnFormat("Line {0}: too many fields ({1}) provided, using first two", line, row.Count);
                    }


                    string variableID = row[0];
                    string variableAlias = (row.Count > 1) ? row[1] : row[0];
                    //string marginErrorAlias = Utilities.EnsureMaxLength(Settings.MoEPrefix + variableAlias, 10);
                    if (variableAlias.Length > 10)
                    {
                        _log.WarnFormat("Line {0}: \"{1}\" name was too long, truncating to 10 characters", line, variableAlias);
                        variableAlias = Utilities.EnsureMaxLength(variableAlias, 10);
                    }

                    //////Track duplicates
                    //AddIntToDict(columnConflicts, line, variableAlias, marginErrorAlias);
                    //IfSetAddIntToDict(reservedColumns, reservedConflicts, line, variableAlias, marginErrorAlias);

                    //uniqueColumnNames.Add(variableAlias);
                    //uniqueColumnNames.Add(marginErrorAlias);


                    dt.Rows.Add(variableID, variableAlias);//, marginErrorAlias, line);
                }
            }
            catch (Exception ex)
            {
                _log.Error("ReadVariablesFile: Exception thrown!", ex);
                _log.ErrorFormat("Variable Import made it to line {0}:{1}", filename, line);
            }

            return dt;
        }


        protected DataTable SetupTable(DbConnection conn, IDataClient client, string tablename)
        {
            tempTableName = tablename;

            client.GetCommand(string.Format(CreateTableSQL, tablename), conn).ExecuteNonQuery();
            string selectAllSQL = string.Format("select * from \"{0}\"", tablename);
            return DataClient.GetMagicTable(conn, client, selectAllSQL);
        }

        protected bool SaveTable(DbConnection conn, IDataClient client, DataTable dt)
        {
            string selectAllSQL = string.Format("select * from \"{0}\"", tempTableName);

            _log.Debug("Saving...");
            var adapter = DataClient.GetMagicAdapter(conn, client, selectAllSQL);
            adapter.Update(dt);
            dt.AcceptChanges();

            return true;
        }

       


        



        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="client"></param>
        /// <param name="filename"></param>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public bool ImportDesiredVariables(DbConnection conn, IDataClient client, string filename, string tablename)
        {
            if ((string.IsNullOrEmpty(filename)) || (!File.Exists(filename)))
            {
                _log.DebugFormat("ImportDesiredVariables failed: provided filename was empty or did not exist \"{0}\" ", filename);
                return false;
            }

            try
            {

                //empty/create our temporary table
                DataTable dt = SetupTable(conn, client, tablename);

                //get a list of the columns they wanted
                dt = ReadVariablesFile(filename, dt);


                //check all our error scenarios
                TableChecker[] fnList = new TableChecker[] {
                    CheckForMaxColumns,
                    CheckForMinColumns,
                    CheckForDuplicates,
                    CheckForMOEDuplicates,
                    CheckForReserved
                };

                bool noErrors = true;
                foreach (var errCheckFn in fnList)
                {
                    string msg = errCheckFn(dt);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        noErrors = false;
                        _log.Error(msg);
                    }
                }

                if (!noErrors)
                {
                    return false;
                }

                SaveTable(conn, client, dt);

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Variable Import Failed", ex);
                RemoveTemporaryTable(conn, client);
            }

            return false;
        }

        private string CheckForMinColumns(DataTable dt)
        {
            if ((dt == null) || (dt.Rows.Count == 0))
            {
                return "No variable names provided!";
            }
            return string.Empty;
        }


        private string CheckForMaxColumns(DataTable dt)
        {
            if (dt.Rows.Count > 100)
            {
                return string.Format(@"This file contained {0} variables. \n "
                 + "A maximum of 100 variables are allowed per export job. \n"
                 + "Please split your requested variables into multiple files and try again.",
                    dt.Rows.Count);
            }
            return string.Empty;
        }


        private string CheckForReserved(DataTable dt)
        {
            StringBuilder errSB = new StringBuilder(512);
            var reservedColumns = Settings.ReservedColumnNames;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var row = dt.Rows[i];
                string columnName = (row[1] as string).ToLower();

                if (reservedColumns.Contains(columnName))
                {
                    errSB.AppendFormat("The column \"{0}\" on line {1} is reserved, please rename this column.\r\n",
                        columnName, i + 1);
                }
                //_log.ErrorFormat("The File {0} contained reserved column names that cannot be used.\r\nPlease remove these conflicts to continue:\r\n{1} ", filename, reservedSB.ToString());
                //_log.Debug("For reference, the following columns are reserved: " + Settings.ReservedColumnNamesString);                    

            }
            return errSB.ToString();
        }

        /// <summary>
        /// Generates an error if a generated column conflicts with another generated column, or a real column.
        /// </summary>
        /// <param name="dt"></param>
        private string CheckForMOEDuplicates(DataTable dt)
        {
            var orignalColumns = new Dictionary<string, int>();
            var errorColumns = new Dictionary<string, List<int>>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var original = (dt.Rows[i][1] as string).ToLower();
                orignalColumns[original] = i;

                string errorColumn = Utilities.EnsureMaxLength(Settings.MoEPrefix + original, 10);
                if (!errorColumns.ContainsKey(errorColumn))
                    errorColumns[errorColumn] = new List<int>();
                errorColumns[errorColumn].Add(i);
            }



            StringBuilder orig = new StringBuilder();
            StringBuilder dupsSB = new StringBuilder(512);
            //dupsSB.AppendFormat("The File {0} contained duplicate or conflicting columns.{1}Please resolve these conflicts to continue:{1}", this._filename, Environment.NewLine);
            //dupsSB.AppendFormat("Note that all variables include a Margin of Error column named \"{0}\" + [column name] which can cause duplicates to be created.", Settings.MoEPrefix);

            foreach (string column in errorColumns.Keys)
            {
                if (orignalColumns.ContainsKey(column))
                {
                    orig.AppendFormat("error column \"{0}\" for line {1} conflicts with original column \"{2}\" on line {3}",
                        column, errorColumns[column][0] + 1,
                        orig, orignalColumns[column] + 1);

                    orig.Append(Environment.NewLine);
                }

                var lines = errorColumns[column];
                if (lines.Count > 1)
                {
                    dupsSB.AppendFormat("error column \"{0}\" for line {1} conflicts with others from line(s):",
                       column, errorColumns[column][0] + 1);

                    for (int i = 1; i < lines.Count; i++) { if (i > 1) { dupsSB.Append(","); } dupsSB.Append(lines[i] + 1); }
                    dupsSB.Append(Environment.NewLine);
                }
            }

            if (dupsSB.Length > 0)
            {
                if (orig.Length > 0)
                    orig.Append(Environment.NewLine);

                orig.Append(dupsSB.ToString());
            }
            return orig.ToString();
        }

        private string CheckForDuplicates(DataTable dt)
        {
            var columnConflicts = new Dictionary<string, List<int>>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var row = dt.Rows[i];
                string columnName = (row[1] as string).ToLower();

                if (!columnConflicts.ContainsKey(columnName))
                    columnConflicts[columnName] = new List<int>();

                columnConflicts[columnName].Add(i);
            }

            StringBuilder dupsSB = new StringBuilder(512);
            foreach (string column in columnConflicts.Keys)
            {
                var lines = columnConflicts[column];
                if (lines.Count == 1)
                    continue;

                if (dupsSB.Length == 0)
                {
                    dupsSB.AppendFormat("The File {0} contained duplicate or conflicting columns.{1}Please resolve these conflicts to continue:{1}",
                        this._filename, Environment.NewLine);

                    dupsSB.AppendFormat("Note that all variables include a Margin of Error column named \"{0}\" + [column name] which can cause duplicates to be created.{1}",
                        Settings.MoEPrefix, Environment.NewLine);
                }

                dupsSB.Append("Column ").Append(column).Append(" appears on line(s): ");
                for (int i = 0; i < lines.Count; i++) { if (i > 0) { dupsSB.Append(","); } dupsSB.Append(lines[i] + 1); }
                dupsSB.Append(Environment.NewLine);
            }

            //if string was non-empty, there were errors
            return dupsSB.ToString();
        }




        
    }
}
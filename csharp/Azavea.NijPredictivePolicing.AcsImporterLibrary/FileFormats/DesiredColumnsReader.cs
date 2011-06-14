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

        public string tempTableName;

        public const string DropTableSQL = "DROP TABLE IF EXISTS {0};";
        public const string CreateTableSQL = 
            @"DROP TABLE IF EXISTS {0}; CREATE TABLE {0} (
            CENSUS_TABLE_ID VARCHAR(32) NOT NULL PRIMARY KEY,
            CUSTOM_COLUMN_NAME VARCHAR(10));";

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
            int line = 0;

            if ((string.IsNullOrEmpty(filename)) || (!File.Exists(filename)))
            {
                _log.DebugFormat("ImportDesiredVariables failed: provided filename was empty or did not exist \"{0}\" ", filename);
                return false;
            }

            try
            {
                //empty/create our temporary table
                tempTableName = tablename;
                client.GetCommand(string.Format(CreateTableSQL, tempTableName), conn).ExecuteNonQuery();

                string selectAllSQL = string.Format("select * from {0}", tempTableName);
                var dt = DataClient.GetMagicTable(conn, client, selectAllSQL);
                dt.Columns.Add("errorColumn", typeof(string));    //TEMP
                dt.Columns.Add("Line", typeof(int));    //TEMP

                if (string.IsNullOrEmpty(_filename))
                {
                    this.LoadFile(filename);
                }

                var reservedColumns = Settings.ReservedColumnNames;
                var uniqueColumnNames = new HashSet<string>();
                var columnConflicts = new Dictionary<string, List<int>>();
                var reservedConflicts = new Dictionary<string, List<int>>();

                
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
                    string marginErrorAlias = Utilities.EnsureMaxLength(Settings.MoEPrefix + variableAlias, 10);
                    if (variableAlias.Length > 10)
                    {
                        _log.WarnFormat("Line {0}: \"{1}\" name was too long, truncating to 10 characters", line, variableAlias);
                        variableAlias = Utilities.EnsureMaxLength(variableAlias, 10);
                    }

                    ////Track duplicates
                    AddIntToDict(columnConflicts, line, variableAlias, marginErrorAlias);
                    IfSetAddIntToDict(reservedColumns, reservedConflicts, line, variableAlias, marginErrorAlias);

                    //uniqueColumnNames.Add(variableAlias);
                    //uniqueColumnNames.Add(marginErrorAlias);


                    dt.Rows.Add(variableID, variableAlias);//, marginErrorAlias, line);
                }

                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    _log.Error("No variable names provided!");
                    return false;
                }



                bool noConflicts = true;

                StringBuilder dupsSB = new StringBuilder();
                foreach (string column in columnConflicts.Keys)
                {
                    var lines = columnConflicts[column];
                    if (lines.Count > 1)
                    {                        
                        dupsSB.Append(column).Append(": line(s) ");
                        for (int i = 0; i < lines.Count; i++) { if (i > 0) { dupsSB.Append(","); } dupsSB.Append(lines[i]); }
                        dupsSB.Append(Environment.NewLine);
                    }
                }
                if (dupsSB.Length > 0)
                {
                    _log.ErrorFormat("The File {0} contained duplicate or conflicting columns.\r\nPlease resolve these conflicts to continue:\r\n{1}", filename, dupsSB.ToString());                                        
                    _log.ErrorFormat("Note that all variables include a Margin of Error column named \"{0}\" + [column name] which can cause duplicates to be created.", Settings.MoEPrefix);
                    noConflicts = false;
                }

                StringBuilder reservedSB = new StringBuilder();
                foreach (string column in reservedConflicts.Keys)
                {
                    var lines = reservedConflicts[column];
                    if (lines.Count >= 1)
                    {
                        reservedSB.Append(column).Append(": line(s) ");
                        for (int i = 0; i < lines.Count; i++) { if (i > 0) { reservedSB.Append(","); } reservedSB.Append(lines[i]); }
                        reservedSB.Append(Environment.NewLine);
                    }
                }
                if (reservedSB.Length > 0)
                {
                    _log.ErrorFormat("The File {0} contained reserved column names that cannot be used.\r\nPlease remove these conflicts to continue:\r\n{1} ", filename, reservedSB.ToString());
                    _log.Debug("For reference, the following columns are reserved: " + Settings.ReservedColumnNamesString);                    
                    noConflicts = false;
                }

                if (dt.Rows.Count > 100)
                {
                    _log.ErrorFormat(@"This file contained {0} variables. \n "
                     + "A maximum of 100 variables are allowed per export job. \n"
                     + "Please split your requested variables into multiple files and try again.",
                        dt.Rows.Count);

                    noConflicts = false;
                }

                if (noConflicts)
                {
                    _log.Debug("Saving...");
                    var adapter = DataClient.GetMagicAdapter(conn, client, selectAllSQL);
                    adapter.Update(dt);
                    dt.AcceptChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _log.Error("Variable Import Failed", ex);
                _log.ErrorFormat("Variable Import made it to line {0}:{1}", filename, line);
                RemoveTemporaryTable(conn, client);
            }

            return false;
        }




        
    }
}

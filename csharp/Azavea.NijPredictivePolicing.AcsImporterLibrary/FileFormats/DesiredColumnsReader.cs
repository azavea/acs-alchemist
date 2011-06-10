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

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats
{
    /// <summary>
    /// Input should be a csv list of (CENSUS_TABLE_ID, Name) pairs, where CENSUS_TABLE_ID is a foreign key into columnMappings.CENSUS_TABLE_ID.  Name is it's description, and is optional.
    /// </summary>
    public class DesiredColumnsReader : CommaSeparatedValueReader
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string tempTableName;

        public const string DropTableSQL = "DROP TABLE IF EXISTS {0};";
        public const string CreateTableSQL = @"DROP TABLE IF EXISTS {0}; CREATE TABLE {0} (
            CENSUS_TABLE_ID VARCHAR(32) NOT NULL PRIMARY KEY,
            CUSTOM_COLUMN_NAME VARCHAR(10));";

        //public string GetCreateTableSQL(string tablename)
        //{
        //    return string.Format(CreateTableSQL, tablename);
        //}


        public bool ImportDesiredVariables(DbConnection conn, IDataClient client, string filename, string tablename)
        {
            if ((string.IsNullOrEmpty(filename)) || (!File.Exists(filename)))
            {
                _log.Debug("No Variable File Found");
                return false;
            }

            int line = 0;

            try
            {
                tempTableName = tablename;
                var duplicateLines = new Dictionary<string, List<int>>(256);
                var reservedLines = new List<int>(256);
                var duplicateInputAliases = new HashSet<string>();
                var duplicateMoeAliases = new HashSet<string>();

                //empty/create our temporary table
                client.GetCommand(string.Format(CreateTableSQL, tempTableName), conn).ExecuteNonQuery();

                string selectAllSQL = string.Format("select * from {0}", tempTableName);
                var dt = DataClient.GetMagicTable(conn, client, selectAllSQL);

                //Constraints will help us catch errors (Also: Iron helps us play.)
                //dt.Constraints.Add("CENSUS_TABLE_ID Primary Key", dt.Columns["CENSUS_TABLE_ID"], true);
                //dt.Constraints.Add("CUSTOM_COLUMN_NAME Unique", dt.Columns["CUSTOM_COLUMN_NAME"], false);
               
               if (string.IsNullOrEmpty(_filename))
                {
                    this.LoadFile(filename);
                }


                foreach (List<string> row in this)
                {
                    line++;

                    if ((row.Count < 1) || (string.IsNullOrEmpty(row[0])))
                        continue;
                    if (row.Count > 2)
                        _log.WarnFormat("Line {0} has more than two fields, all fields after the first two will be ignored", line);


                    string varName = row[0];
                    string varRealAlias = (row.Count > 1) ? row[1] : row[0];
                    string varTruncAlias = varRealAlias;
                    string mvarTruncAlias = Utilities.EnsureMaxLength(Settings.MoEPrefix + varTruncAlias, 10);
                    duplicateMoeAliases.Add(mvarTruncAlias);

                    if (varRealAlias.Length > 10)
                    {
                        //Shapefiles have a 10 character column name limit :(
                        _log.WarnFormat("Line {0}: \"{1}\" name was too long, truncating to 10 characters", line, varRealAlias);
                        varTruncAlias = Utilities.EnsureMaxLength(varRealAlias, 10);
                    }


                    if (!duplicateLines.ContainsKey(varTruncAlias))
                    {
                        duplicateLines.Add(varTruncAlias, new List<int>(4));
                        duplicateLines[varTruncAlias].Add(line);
                    }
                    else
                    {
                        duplicateLines[varTruncAlias].Add(line);
                        duplicateInputAliases.Add(varTruncAlias);
                    }


                    //Make sure m + name isn't here either, b/c we create it later.
                    if (!duplicateLines.ContainsKey(mvarTruncAlias))
                    {
                        duplicateLines.Add(mvarTruncAlias, new List<int>(4));
                        duplicateLines[mvarTruncAlias].Add(line);
                    }
                    else
                    {
                        duplicateLines[mvarTruncAlias].Add(line);
                    }

                    if (Settings.ReservedColumnNames.Contains(varTruncAlias)
                        || Settings.ReservedColumnNames.Contains(mvarTruncAlias))
                    {
                        reservedLines.Add(line);
                    }

                    dt.Rows.Add(varName, varTruncAlias);
                }

                if ((dt == null) || (dt.Rows.Count == 0))
                {
                    _log.Error("No variables imported!");
                    return false;
                }

                bool success = true;
                StringBuilder error = new StringBuilder(1024);
                error.AppendFormat("Some errors were encountered while reading file {0}\n", filename);

                //PRE: duplicateMoeAliases contains the name of every MoE column generated
                //POST: duplicateMoeAliases contains only the names of duplicated MoE columns
                duplicateMoeAliases.IntersectWith(duplicateInputAliases);

                //PRE: duplicateInputAliases contains the name of every duplicated column
                //POST: duplicateInputAliases contains the name of every duplicated column 
                //  except those that were also duplicated MoE columns
                duplicateInputAliases.ExceptWith(duplicateMoeAliases);

                if (duplicateInputAliases.Count > 0)
                {
                    error.Append("The following duplicate variables were found:\n");
                    foreach (string name in duplicateInputAliases.OrderBy(x => x))
                    {
                        error.Append(name).Append(" (lines ");
                        foreach (int myLine in duplicateLines[name])
                        {
                            error.Append(myLine).Append(", ");
                        }
                        error.Remove(error.Length - 2, 2);
                        error.Append(")\n");
                    }
                    error.Append("\n");

                    success = false;
                }


                if (duplicateMoeAliases.Count > 0)
                {
                    error.Append("The following variables were automatically generated and resulted in duplicates:\n");
                    foreach (string name in duplicateMoeAliases.OrderBy(x => x))
                    {
                        error.Append(name).Append(" duplicated on lines (");
                        foreach (int myLine in duplicateLines[name])
                        {
                            error.Append(myLine).Append(", ");
                        }
                        error.Remove(error.Length - 2, 2);
                        error.Append(")\n");
                    }
                    error.Append("\n");

                    success = false;
                }

                if (reservedLines.Count > 0)
                {
                    error.Append("The following lines contained names that are reserved by the importer (names that are reserved include ");

                    foreach (string name in Settings.ReservedColumnNames.OrderBy(x => x))
                    {
                        error.Append(name).Append(", ");
                    }
                    error.Remove(error.Length - 2, 2);
                    error.Append("):\n");

                    foreach (int myLine in reservedLines)
                    {
                        error.Append(myLine).Append(", ");
                    }
                    error.Remove(error.Length - 2, 2);
                    error.Append("\n\n");

                    success = false;
                }

                if (!success)
                {
                    error.AppendFormat("Please modify your column specification file to remove duplicates.  Note that all columns with a given name have a corresponding Margin of Error column named \"{0}\" + [column name] which can cause duplicates to be created.\n", Settings.MoEPrefix);
                }

                if (dt.Rows.Count > 100)
                {
                    error.AppendFormat("The maximum number of variables this utility can use is 100, but this file contained {0} entries.  Please split your requested variables into multiple files and try again.\n", dt.Rows.Count);
                    success = false;
                }

                if (success == false)
                {
                    _log.Error(error.ToString());
                    return false;
                }

                _log.Debug("Saving...");
                var adapter = DataClient.GetMagicAdapter(conn, client, selectAllSQL);
                adapter.Update(dt);
                dt.AcceptChanges();

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Variable Import Failed", ex);
                _log.ErrorFormat("Variable Import made it to line {0}:{1}", filename, line);
                RemoveTemporaryTable(conn, client);
            }

            return false;
        }


        public void RemoveTemporaryTable(DbConnection conn, IDataClient client)
        {
            client.GetCommand(string.Format(DropTableSQL, tempTableName), conn).ExecuteNonQuery();
        }

        
    }
}

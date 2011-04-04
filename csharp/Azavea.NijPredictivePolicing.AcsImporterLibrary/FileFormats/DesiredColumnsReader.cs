using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using Azavea.NijPredictivePolicing.Common.Data;
using System.Data.Common;
using Azavea.NijPredictivePolicing.Common.DB;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats
{
    /// <summary>
    /// Input should be a csv list of (CENSUS_TABLE_ID, Name) pairs, where CENSUS_TABLE_ID is a foreign key into columnMappings.CENSUS_TABLE_ID.  Name is it's description, and is optional.
    /// </summary>
    public class DesiredColumnsReader : CommaSeparatedValueReader
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string TempTableName;

        public const string DropTableSQL = "DROP TABLE IF EXISTS {0};";
        public const string CreateTableSQL = @"DROP TABLE IF EXISTS {0}; CREATE TABLE {0} (
            CENSUS_TABLE_ID VARCHAR(32) NOT NULL PRIMARY KEY,
            CUSTOM_COLUMN_NAME VARCHAR(10));";

        //public string GetCreateTableSQL(string tablename)
        //{
        //    return string.Format(CreateTableSQL, tablename);
        //}


        public bool CreateTemporaryTable(DbConnection conn, IDataClient client, string filename, string tablename)
        {
            if ((string.IsNullOrEmpty(filename)) || (!File.Exists(filename)))
            {
                _log.Debug("No Variable File Found");
                return false;
            }
            TempTableName = tablename;
            int line = 0;

            try
            {
                //empty/create our temporary table
                client.GetCommand(string.Format(CreateTableSQL, TempTableName), conn).ExecuteNonQuery();

                string selectAllSQL = string.Format("select * from {0}", TempTableName);
                var dt = DataClient.GetMagicTable(conn, client, selectAllSQL);

                //Constraints will help us catch errors (Also: Iron helps us play.)
                dt.Constraints.Add("CENSUS_TABLE_ID Primary Key", dt.Columns["CENSUS_TABLE_ID"], true);
                dt.Constraints.Add("CUSTOM_COLUMN_NAME Unique", dt.Columns["CUSTOM_COLUMN_NAME"], false);
               
                //we're enumerable!

               if (string.IsNullOrEmpty(_filename))
                {
                    this.LoadFile(filename);
                }


                foreach (List<string> row in this)
                {
                    line++;

                    if ((row.Count < 1) || (string.IsNullOrEmpty(row[0])))
                        continue;

                    var varName = row[0];
                    var varAlias = (row.Count > 1) ? row[1] : row[0];                    
                    if (varAlias.Length > 10)
                    {
                        //Shapefiles have a 10 character column name limit :(
                        _log.WarnFormat("Line:{0}, \"{1}\" name was too long, max 10 characters", line, varAlias);
                        varAlias = varAlias.Substring(0, 10);
                    }

                    dt.Rows.Add(varName, varAlias);
                }

                if ((dt == null) && (dt.Rows.Count == 0))
                {
                    _log.Error("No variables imported!");
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
            }

            return false;
        }


        public void RemoveTemporaryTable(DbConnection conn, IDataClient client)
        {
            client.GetCommand(string.Format(CreateTableSQL, TempTableName), conn).ExecuteNonQuery();
        }

        
    }
}

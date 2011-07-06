using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.OleDb;
using System.Data;
using System.Data.SqlClient;
using Azavea.NijPredictivePolicing.Common.Data;
using log4net;

namespace Azavea.NijPredictivePolicing.Common.DB
{
    /// <summary>
    /// Generic helper class for managing database connections and objects
    /// </summary>
    public static class DataClient
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Get an instance of the default overridding database type
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public static IDataClient GetDefaultClient(string hostname)
        {            
            return new SqliteDataClient(hostname);
        }

        /// <summary>
        /// Convenience function to generate SQL to create a table matching the provided fields
        /// </summary>
        /// <param name="tablename">the name for the new table</param>
        /// <param name="columns">a collection of FixedWidthField objects</param>
        /// <returns></returns>
        public static string GenerateTableSQLFromFields(string tablename, List<FixedWidthField> columns)
        {
            return SqliteDataClient.GenerateTableSQLFromFields(tablename, columns);
        }

        /// <summary>
        /// Uses a select statement and the ADO.NET CommandBuilder 
        /// to generate Insert,Update, and Delete statements, and load them onto an adapter
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="client"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DbDataAdapter GetMagicAdapter(DbConnection conn, IDataClient client, string sql)
        {
            var cmd = client.GetCommand(sql, conn);
            var dba = client.GetDataAdapter(cmd);
            var builder = client.GetCommandBuilder(dba);

            dba.InsertCommand = builder.GetInsertCommand(true);
            dba.DeleteCommand = builder.GetDeleteCommand(true);
            dba.UpdateCommand = builder.GetUpdateCommand(true);

            if (dba.InsertCommand != null)
                dba.InsertCommand.CommandTimeout = client.QueryTimeout;

            if (dba.DeleteCommand != null)
                dba.DeleteCommand.CommandTimeout = client.QueryTimeout;

            if (dba.UpdateCommand != null)
                dba.UpdateCommand.CommandTimeout = client.QueryTimeout;

            return dba;
        }

        /// <summary>
        /// 'Magically' populates a DataTable object with the contents of your sql query 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="client"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataTable GetMagicTable(DbConnection conn, IDataClient client, string sql)
        {
            DataTable dt = new DataTable();

            try
            {
                var cmd = client.GetCommand(sql, conn);
                var dba = client.GetDataAdapter(cmd);

                dba.Fill(dt);

                return dt;
            }
            catch (Exception ex)
            {
                
                _log.Error("error during select", ex);
            }

            return null;
        }

        /// <summary>
        /// Checks the schema of the database to see if the given table is present
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="client"></param>
        /// <param name="tablename"></param>
        /// <returns></returns>
        public static bool HasTable(DbConnection conn, IDataClient client, string tablename)
        {
            try
            {
                var dt = conn.GetSchema("Tables");
                foreach (DataRow row in dt.Rows)
                {
                    if ((row["TABLE_NAME"] as string) == (string)tablename)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error while looking for table", ex);
            }
            return false;
        }

    }
}

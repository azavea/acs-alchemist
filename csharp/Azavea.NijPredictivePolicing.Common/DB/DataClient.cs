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
    public static class DataClient
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static IDataClient GetDefaultClient(string hostname)
        {            
            return new SqliteDataClient(hostname);
        }

        public static string GenerateTableSQLFromFields(string tablename, List<FixedWidthField> columns)
        {
            return SqliteDataClient.GenerateTableSQLFromFields(tablename, columns);
        }

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

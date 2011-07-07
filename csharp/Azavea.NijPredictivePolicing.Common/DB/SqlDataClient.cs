using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;

namespace Azavea.NijPredictivePolicing.Common.DB
{
    /// <summary>
    /// See 'IDataClient' for descriptions
    /// </summary>
    public class SqlDataClient : IDataClient
    {
        protected string _connectionString;
        protected int _queryTimeout = 30;
        public int _connsOpened = 0, _connsClosed = 0;



        public SqlDataClient(string hostname)
        {
            _connectionString = string.Format("Data Source={0}", hostname);
        }



        /// <summary>
        /// Close your connections when you're done with them, so they can go back into the pool.
        /// http://msdn.microsoft.com/en-us/library/8xx3tyca.aspx
        /// </summary>
        public DbConnection GetConnection()
        {
            DbConnection conn = new SqlConnection(_connectionString);
            conn.Open();
            _connsOpened++;

            return conn;
        }

        public DbConnection GetConnection(string connString)
        {
            DbConnection conn = new SqlConnection(connString);
            conn.Open();
            _connsOpened++;

            return conn;
        }

        public bool TestDatabaseConnection()
        {
            try
            {
                using (DbCommand cmd = GetCommand("select 1 + 1 as two"))
                {
                    int two = (int)cmd.ExecuteScalar();
                    return (two == 2);
                }
            }
            catch { }
            return false;
        }


        public DbCommand GetCommand()
        {
            DbConnection conn = GetConnection();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandTimeout = _queryTimeout;
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;     //this is the default, isn't it?

            return cmd;
        }

        public DbCommand GetCommand(string sql)
        {
            DbConnection conn = GetConnection();
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandTimeout = _queryTimeout;
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;     //this is the default, isn't it?
            cmd.CommandText = sql;

            return cmd;
        }


        public DbCommand GetCommand(string sql, DbConnection conn)
        {
            var cmd = this.GetCommand(conn);
            cmd.CommandText = sql;
            return cmd;
        }

        public DbCommand GetCommand(string sql, string connString)
        {
            DbConnection conn = GetConnection(connString);
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandTimeout = _queryTimeout;
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;     //this is the default, isn't it?
            cmd.CommandText = sql;

            return cmd;
        }

        public DbCommand GetCommand(DbConnection conn)
        {
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandTimeout = _queryTimeout;
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;     //this is the default, isn't it?

            return cmd;
        }


        public DbCommandBuilder GetCommandBuilder()
        {
            return new SqlCommandBuilder();
        }

        public DbCommandBuilder GetCommandBuilder(DbDataAdapter dba)
        {
            return new SqlCommandBuilder((SqlDataAdapter)dba);
        }

        public DbDataAdapter GetDataAdapter()
        {
            //return new OleDbDataAdapter();
            return new SqlDataAdapter();
        }

        public DbDataAdapter GetDataAdapter(string sql)
        {
            //DbDataAdapter dba = new OleDbDataAdapter(sql, (OleDbConnection)GetConnection());
            DbDataAdapter dba = new SqlDataAdapter(sql, (SqlConnection)GetConnection());

            dba.SelectCommand.CommandTimeout = _queryTimeout;
            return dba;
        }

        public DbDataAdapter GetDataAdapter(string sql, string connString)
        {
            DbDataAdapter dba = new SqlDataAdapter(sql, (SqlConnection)GetConnection(connString));
            dba.SelectCommand.CommandTimeout = _queryTimeout;
            return dba;
        }

        public DbDataAdapter GetDataAdapter(DbCommand selectCmd)
        {
            ///don't set the connection here (it's assumed the incoming command will have one already)
            DbDataAdapter dba = new SqlDataAdapter((SqlCommand)selectCmd);
            dba.SelectCommand.CommandTimeout = _queryTimeout;
            return dba;
        }

        public DbParameter GetParameter(string name, DbType type, int size)
        {
            //return new OleDbParameter(name, type, size);
            return new SqlParameter(name, (SqlDbType)type, size);
        }

        public DbParameter GetParameter(string name, DbType type, int size, string srcColumn)
        {
            var p = new SqlParameter(name, (SqlDbType)type, size);
            p.SourceColumn = srcColumn;
            p.SourceVersion = DataRowVersion.Current;
            return p;
        }


        public DbParameter AddParameter(DbCommand selectCmd, string name, object value)
        {
            return ((SqlCommand)selectCmd).Parameters.AddWithValue(name, value);
        }


        public string AddParameterList<T>(DbCommand cmd, string prefix, IEnumerable<T> someList)
        {
            int i = 0;
            string paramPrefix = "id";

            StringBuilder ps = new StringBuilder();
            ps.Append("(");
            foreach (T s in someList)
            {
                string pName = paramPrefix + i.ToString();
                AddParameter(cmd, pName, s);

                if (i > 0)
                    ps.Append(", ");

                ps.Append("@" + pName);

                i++;
            }
            ps.Append(")");
            return ps.ToString();
        }

        public int QueryTimeout { get { return _queryTimeout; } }
    }

}

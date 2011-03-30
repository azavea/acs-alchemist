using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Data.SQLite;
using Azavea.NijPredictivePolicing.Common.Data;

namespace Azavea.NijPredictivePolicing.Common.DB
{
    public class SqliteDataClient : IDataClient
    {
        protected string _connectionString;
        protected int _queryTimeout = 30;
        public int _connsOpened = 0, _connsClosed = 0;



        public SqliteDataClient(string filename)
        {
            //Synchronous=Full
            _connectionString = string.Format("Synchronous=Off;Cache Size=64000000;Max Pool Size=100;Data Source={0};", filename);
        }



        /// <summary>
        /// Close your connections when you're done with them, so they can go back into the pool.
        /// http://msdn.microsoft.com/en-us/library/8xx3tyca.aspx
        /// </summary>
        public DbConnection GetConnection()
        {
            DbConnection conn = new SQLiteConnection(_connectionString);
            conn.Open();
            _connsOpened++;

            return conn;
        }

        public DbConnection GetConnection(string connString)
        {
            DbConnection conn = new SQLiteConnection(connString);
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
                    int two = Utilities.GetAs<int>(cmd.ExecuteScalar(), -1);
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
            var cmd = this.GetCommand();
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
            return new SQLiteCommandBuilder();
        }

        public DbCommandBuilder GetCommandBuilder(DbDataAdapter dba)
        {
            return new SQLiteCommandBuilder((SQLiteDataAdapter)dba);
        }

        public DbDataAdapter GetDataAdapter()
        {
            //return new OleDbDataAdapter();
            return new SQLiteDataAdapter();
        }

        public DbDataAdapter GetDataAdapter(string sql)
        {
            //DbDataAdapter dba = new OleDbDataAdapter(sql, (OleDbConnection)GetConnection());
            DbDataAdapter dba = new SQLiteDataAdapter(sql, (SQLiteConnection)GetConnection());

            dba.SelectCommand.CommandTimeout = _queryTimeout;
            return dba;
        }

        public DbDataAdapter GetDataAdapter(string sql, string connString)
        {
            DbDataAdapter dba = new SQLiteDataAdapter(sql, (SQLiteConnection)GetConnection(connString));
            dba.SelectCommand.CommandTimeout = _queryTimeout;
            return dba;
        }

        public DbDataAdapter GetDataAdapter(DbCommand selectCmd)
        {
            ///don't set the connection here (it's assumed the incoming command will have one already)
            DbDataAdapter dba = new SQLiteDataAdapter((SQLiteCommand)selectCmd);
            dba.SelectCommand.CommandTimeout = _queryTimeout;
            return dba;
        }

        public DbParameter GetParameter(string name, DbType type, int size)
        {
            //return new OleDbParameter(name, type, size);
            return new SQLiteParameter(name, (DbType)type, size);
        }

        public DbParameter GetParameter(string name, DbType type, int size, string srcColumn)
        {
            var p = new SQLiteParameter(name, (DbType)type, size);
            p.SourceColumn = srcColumn;
            p.SourceVersion = DataRowVersion.Current;
            return p;
        }


        public DbParameter AddParameter(DbCommand selectCmd, string name, object value)
        {
            return ((SQLiteCommand)selectCmd).Parameters.AddWithValue(name, value);
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

        public static string GenerateTableSQLFromFields(string tablename, List<FixedWidthField> columns)
        {
            StringBuilder sql = new StringBuilder(512);
            sql.AppendFormat("CREATE TABLE {0} ( ", tablename);
            sql.Append("ixid INTEGER NOT NULL PRIMARY KEY, ");

            for (int i = 0; i < columns.Count; i++)
            {
                var col = columns[i];
                if (i != 0)
                    sql.Append(", ");

                string dataType;
                switch (col.Type)
                {
                    case FixedWidthTypes.DOUBLE:
                    case FixedWidthTypes.DECIMAL:
                    case FixedWidthTypes.FLOAT:
                        dataType = string.Format("REAL({0})", col.End);
                        break;

                    case FixedWidthTypes.INT:
                    case FixedWidthTypes.LONG:
                        dataType = string.Format("INTEGER({0})", col.End);
                        break;

                    case FixedWidthTypes.STRING:
                    default:
                        dataType = string.Format("TEXT({0})", col.End);
                        break;
                }


                sql.AppendFormat("{0} {1}", col.ColumnName, dataType);
            }


            sql.Append(" ); ");
            return sql.ToString();
        }


        public int QueryTimeout { get { return _queryTimeout; } }
    }

}

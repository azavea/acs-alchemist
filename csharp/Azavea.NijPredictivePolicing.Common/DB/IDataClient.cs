using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace Azavea.NijPredictivePolicing.Common.DB
{
    public interface IDataClient
    {
        bool TestDatabaseConnection();
        DbConnection GetConnection();
        DbConnection GetConnection(string connString);
        DbCommand GetCommand();
        DbCommand GetCommand(string sql);
        DbCommand GetCommand(string sql, DbConnection conn);
        DbCommand GetCommand(string sql, string connString);
        DbCommand GetCommand(DbConnection conn);
        DbCommandBuilder GetCommandBuilder();
        DbCommandBuilder GetCommandBuilder(DbDataAdapter dba);
        DbDataAdapter GetDataAdapter();
        DbDataAdapter GetDataAdapter(string sql);
        DbDataAdapter GetDataAdapter(string sql, string connString);
        DbDataAdapter GetDataAdapter(DbCommand selectCmd);
        DbParameter GetParameter(string name, DbType type, int size);
        DbParameter GetParameter(string name, DbType type, int size, string srcColumn);
        DbParameter AddParameter(DbCommand selectCmd, string name, object value);
        string AddParameterList<T>(DbCommand cmd, string prefix, IEnumerable<T> someList);

        int QueryTimeout { get; }
    }

}

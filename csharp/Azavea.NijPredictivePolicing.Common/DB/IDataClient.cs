/*
  Copyright (c) 2011 Azavea, Inc.
 
  This file is part of _SOLUTIONNAME_.

  _SOLUTIONNAME_ is free software: you can redistribute it and/or modify
  it under the terms of the GNU Lesser General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  _SOLUTIONNAME_ is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Lesser General Public License for more details.

  You should have received a copy of the GNU Lesser General Public License
  along with _SOLUTIONNAME_.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace Azavea.NijPredictivePolicing.Common.DB
{
    /// <summary>
    /// An interface to use when building a new DataClient
    /// </summary>
    public interface IDataClient
    {
        /// <summary>
        /// Tests to make sure the database connection is working
        /// </summary>
        /// <returns></returns>
        bool TestDatabaseConnection();

        /// <summary>
        /// Builds a new default connection
        /// </summary>
        /// <returns></returns>
        DbConnection GetConnection();

        /// <summary>
        /// Opens a new connection using the provided connString
        /// </summary>
        /// <param name="connString"></param>
        /// <returns></returns>
        DbConnection GetConnection(string connString);

        /// <summary>
        /// Builds a default command
        /// </summary>
        /// <returns></returns>
        DbCommand GetCommand();

        /// <summary>
        /// Builds a new command using the given sql
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        DbCommand GetCommand(string sql);

        /// <summary>
        /// Builds a new command using the given sql, and the provided connection
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        DbCommand GetCommand(string sql, DbConnection conn);

        /// <summary>
        /// Builds a new command using the given sql and a new connection using connString
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="connString"></param>
        /// <returns></returns>
        DbCommand GetCommand(string sql, string connString);

        /// <summary>
        /// Builds a new command using the provided connection
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        DbCommand GetCommand(DbConnection conn);

        /// <summary>
        /// Builds a default CommandBuilder
        /// </summary>
        /// <returns></returns>
        DbCommandBuilder GetCommandBuilder();

        /// <summary>
        /// Builds a CommandBuilder using the given DataAdapter
        /// </summary>
        /// <param name="dba"></param>
        /// <returns></returns>
        DbCommandBuilder GetCommandBuilder(DbDataAdapter dba);

        /// <summary>
        /// Builds a DataAdapter
        /// </summary>
        /// <returns></returns>
        DbDataAdapter GetDataAdapter();

        /// <summary>
        /// Builds a DataAdapter using the given sql, and the default connection string
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        DbDataAdapter GetDataAdapter(string sql);

        /// <summary>
        /// Builds a DataAdapter using the given sql and a new connection using connString
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="connString"></param>
        /// <returns></returns>
        DbDataAdapter GetDataAdapter(string sql, string connString);

        /// <summary>
        /// Builds a DataAdapter using the provided command
        /// </summary>
        /// <param name="selectCmd"></param>
        /// <returns></returns>
        DbDataAdapter GetDataAdapter(DbCommand selectCmd);

        /// <summary>
        /// Helper function for building a parameter of a given type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        DbParameter GetParameter(string name, DbType type, int size);

        /// <summary>
        /// Helper function for building a parameter from a DataTable column
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <param name="srcColumn"></param>
        /// <returns></returns>
        DbParameter GetParameter(string name, DbType type, int size, string srcColumn);

        /// <summary>
        /// Adds a parameter to the DbCommand
        /// </summary>
        /// <param name="selectCmd"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        DbParameter AddParameter(DbCommand selectCmd, string name, object value);

        /// <summary>
        /// Adds a collection of values of a given type to both the sql, and the DbCommand object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="prefix"></param>
        /// <param name="someList"></param>
        /// <returns></returns>
        string AddParameterList<T>(DbCommand cmd, string prefix, IEnumerable<T> someList);

        /// <summary>
        /// Controls the maximum allowed duration of a database operation
        /// </summary>
        int QueryTimeout { get; }
    }

}

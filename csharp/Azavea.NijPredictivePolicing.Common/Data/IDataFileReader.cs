/*
  Copyright (c) 2012 Azavea, Inc.
 
  This file is part of _SOLUTIONNAME_.

  _SOLUTIONNAME_ is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  _SOLUTIONNAME_ is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with _SOLUTIONNAME_.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Azavea.NijPredictivePolicing.Common.Data
{
    /// <summary>
    /// This helps us more easily abstract away interacting with our data sources
    /// (be they excel, access, tsv, etc...)
    /// </summary>
    public interface IDataFileReader
    {
        /// <summary>
        /// Load the specified input file
        /// </summary>
        bool LoadFile(string filename);

        /// <summary>
        /// Closes the current data source
        /// </summary>
        void Close();

        /// <summary>
        /// Gets a list of the tables contained by the data source
        /// </summary>
        /// <returns></returns>
        List<string> TableNames();

        /// <summary>
        /// Sets the current tablename
        /// </summary>
        /// <param name="tableName"></param>
        void SetTablename(string tableName);
    }
}

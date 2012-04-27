/*
  Copyright (c) 2012 Azavea, Inc.
 
  This file is part of ACS Alchemist.

  ACS Alchemist is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  ACS Alchemist is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with ACS Alchemist.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Azavea.NijPredictivePolicing.Common.Data
{
    /// <summary>
    /// This helps us more easily abstract away writing to our data sources
    /// (be they excel, access, tsv, etc...)
    /// </summary>
    public interface IDataFileWriter
    {
        /// <summary>
        /// Set the output filename
        /// </summary>
        bool SetWriteFile(string filename);

        //bool SetWriteStream(Stream input);

        /// <summary>
        /// Write values to the output stream
        /// </summary>
        bool WriteLine(IEnumerable<string> values);

        /// <summary>
        /// Change the current tablename
        /// </summary>
        void SetTablename(string tableName);

        /// <summary>
        /// Create a new output table
        /// </summary>
        bool CreateTable(string tablename, IEnumerable<string> columns);

        /// <summary>
        /// Flush the output stream
        /// </summary>
        /// <returns></returns>
        bool Flush();

        /// <summary>
        /// Close the output stream
        /// </summary>
        /// <returns></returns>
        bool Close();
    }
}

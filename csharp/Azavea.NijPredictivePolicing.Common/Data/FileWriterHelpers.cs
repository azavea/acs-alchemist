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
using System.Data;
using log4net;

namespace Azavea.NijPredictivePolicing.Common.Data
{
    /// <summary>
    /// Helper class for writing a DataTable object to a IDataFileWriter object
    /// </summary>
    public static class FileWriterHelpers
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Helper function for writing a datatable to a file
        /// </summary>
        /// <param name="writer">The writer to write to</param>
        /// <param name="dt">The datatable to write</param>
        /// <returns>True on success, false on failure</returns>
        public static bool WriteDataTable(IDataFileWriter writer, DataTable dt)
        {
            try
            {
                int colCount = dt.Columns.Count;
                List<string> colNames = new List<string>(colCount);
                foreach (DataColumn col in dt.Columns)
                {
                    colNames.Add(col.ColumnName);
                }

                writer.CreateTable(string.Empty, colNames);

                string[] rowValues = new string[colCount];
                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < colCount; i++)
                    {
                        rowValues[i] = row[i] + string.Empty;
                    }

                    writer.WriteLine(rowValues);
                }
                writer.Flush();

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error writing table to writer", ex);
            }
            return false;
        }

    }
}

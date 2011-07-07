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
using Excel;
using log4net;
using System.IO;
using Azavea.NijPredictivePolicing.Common.Data;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats
{
    /// <summary>
    /// This class contains the column definitions for the ACS Sequence file
    /// </summary>
    public class SequenceFileReader : IDisposable
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        protected ExcelBinaryReader _reader;
        protected FileStream _input;
        public bool HasFile = false;

        public SequenceFileReader(string filename)
        {
            this.HasFile = File.Exists(filename);
            if(HasFile)
            {
                _input = new FileStream(filename, FileMode.Open, FileAccess.Read);
                _reader = ExcelReaderFactory.CreateBinaryReader(_input) as ExcelBinaryReader;
            }
        }

        public ExcelBinaryReader GetReader()
        {
            return _reader;
        }


        private static List<FixedWidthField> _columns;

        /// <summary>
        /// List of columns for the columnMappings table.  If you change these, make sure you also look at
        /// AcsDataManager.CreateColumnMappingsTable()
        /// </summary>
        public static List<FixedWidthField> Columns
        {
            get
            {
                if (_columns == null)
                {
                    //We're not actually using FixedWidthFields for this, but we have a GenerateTableSQLFromFields()
                    //function, so might as well use it
                    _columns = new List<FixedWidthField>(new FixedWidthField[] {
                        new FixedWidthField("CENSUS_TABLE_ID", "Census Table ID", 0, 0),
                        new FixedWidthField("COLNAME", "Census Variable Name", 0, 0),
                        new FixedWidthField("COLNO", "Column Number (1 indexed)", 0, 0),
                        new FixedWidthField("SEQNO", "Sequence Number", 0, 0)
                        });
                }
                return _columns;
            }
        }

        public void Dispose()
        {
            _reader.Close();

            if (_input != null)
            {
                _input.Close();
                _input.Dispose();
            }
            _columns = null;
        }
    }
}

/*
  Copyright (c) 2011 Azavea, Inc.
 
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
using log4net;


namespace Azavea.NijPredictivePolicing.Common.Data
{
    /// <summary>
    /// a basic implementation of the IDataWriter interface, so we can simply export tab separated value files
    /// </summary>
    public class TabSeparatedValueWriter : IDataFileWriter
    {
        private readonly ILog _log = LogManager.GetLogger(new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);


        /// <summary>
        /// the tab, for the tab-separated part
        /// </summary>
        protected char[] _splitChars = new char[] { '\t' };

        /// <summary>
        /// an internal copy of our filename
        /// </summary>
        protected string _filename;

        /// <summary>
        /// our wrapper around any provided stream
        /// </summary>
        protected System.IO.StreamWriter _outputStream;

        /// <summary>
        /// our buffer for constructing lines
        /// </summary>
        protected StringBuilder _line = new StringBuilder(1024);

        /// <summary>
        /// construct a new blank writer
        /// (any writes will throw exceptions if you don't set a stream or a file!)
        /// </summary>
        public TabSeparatedValueWriter() { }

        /// <summary>
        /// construct a new writer to append to the given file
        /// </summary>
        /// <param name="filename"></param>
        public TabSeparatedValueWriter(string filename)
        {
            SetWriteFile(filename);
        }

        #region IDataWriter Members

        /// <summary>
        /// specify an output file for APPENDing to, and sharing reading.
        /// (if you don't like it, send in your own filestream :)
        /// </summary>
        public bool SetWriteFile(string filename)
        {
            try
            {
                _filename = filename;
                FileStream fs = new FileStream(filename, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read);
                _outputStream = new StreamWriter(fs);
                return _outputStream.BaseStream.CanWrite;
            }
            catch (Exception ex)
            {
                _log.Error("SetWriteFile", ex);
            }
            return false;
        }

        /// <summary>
        /// changes the currently active table, does nothing for this type of file, since it only contains one table.
        /// </summary>
        public void SetTablename(string tableName)
        {
            //_tablename = tableName;
        }

        /// <summary>
        /// simply appends the column row.  call this first!
        /// (you would have to call this first anyway.)
        /// </summary>
        public bool CreateTable(string tablename, IEnumerable<string> columns)
        {
            return WriteLine(columns);
        }



        ///// <summary>
        ///// allows you to use any functional writable stream
        ///// </summary>
        //public bool SetWriteStream(Stream output)
        //{
        //    _filename = null;
        //    _outputStream = new StreamWriter(output);
        //    return _outputStream.BaseStream.CanWrite;
        //}

        /// <summary>
        /// build a column line (first line in a delim-separated value file)
        /// </summary>
        public string MakeColumnLine(List<string> columns)
        {
            StringBuilder colLine = new StringBuilder(512);
            for (int i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                    colLine.Append(_splitChars);

                colLine.Append(columns[i]);
            }

            return colLine.ToString();
        }

        /// <summary>
        /// generate a String.Format line based on our column count
        /// </summary>
        public string MakeFormatLine(List<string> columns)
        {
            StringBuilder formatLine = new StringBuilder(512);
            for (int i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                    formatLine.Append(_splitChars);

                formatLine.Append("{" + i + "}");
            }

            return formatLine.ToString();
        }

        /// <summary>
        /// Write a line directly to our output
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public bool WriteLine(string line)
        {
            _outputStream.WriteLine(line);
            return true;
        }

        //public bool WriteLine(IEnumerable<object> values)
        //{
        //    return WriteLine((IEnumerable<string>)values);
        //}


        /// <summary>
        /// Make sure you have a value for every column!
        /// This is really intentionally not thread safe, do not share this class across threads.
        /// really, don't write one of these files across multiple threads at the same time.
        /// </summary>
        public bool WriteLine(IEnumerable<string> values)
        {
            if ((_outputStream == null) || (!_outputStream.BaseStream.CanWrite))
                return false;

            _line.Length = 0;
            if (values != null)
            {
                bool onceThru = false;
                foreach (string s in values)
                {
                    if (onceThru)
                        _line.Append(this._splitChars);

                    _line.Append(s);
                    onceThru = true;
                }
            }

            _outputStream.WriteLine(_line.ToString());
            _line.Length = 0;

            return true;
        }

        /// <summary>
        /// flushes the stream
        /// </summary>
        public bool Flush()
        {
            _outputStream.Flush();
            return true;
        }

        /// <summary>
        /// closes the underlying stream
        /// </summary>
        public bool Close()
        {
            _outputStream.Flush();
            _outputStream.Close();
            return true;
        }

        #endregion

    }
}

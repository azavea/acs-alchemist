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
using log4net;
using System.Collections;


namespace Azavea.NijPredictivePolicing.Common.Data
{
    /// <summary>
    /// a basic implementation of the IDataFileReader interface, so we can simply export tab separated value files
    /// </summary>
    public class TabSeparatedValueReader : IDataFileReader, IEnumerable<List<string>>
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
        /// our input stream
        /// </summary>
        protected System.IO.Stream _dataStream;

        public TabSeparatedValueReader() { }
        public TabSeparatedValueReader(string filename)
        {
            LoadFile(filename);
        }


        #region IDataReader Members

        /// <summary>
        /// Very gingerly open the provided file
        /// </summary>
        public bool LoadFile(string filename)
        {
            _filename = filename;
            if (!System.IO.File.Exists(filename))
                return false;

            //open the file, for reading only, and allow read sharing.
            try
            {
                _dataStream = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("LoadFile", ex);
            }
            return false;
        }

        /// <summary>
        /// here for the interface
        /// </summary>
        /// <returns></returns>
        public List<string> TableNames()
        {
            //here for the interface
            return null;
        }

        /// <summary>
        /// here for the interface
        /// </summary>
        /// <param name="tableName"></param>
        public void SetTablename(string tableName)
        {
            //here for the interface
        }

        /// <summary>
        /// returns an TabSeparatedValueFileEnumerator
        /// </summary>
        IEnumerator<List<string>> System.Collections.Generic.IEnumerable<List<string>>.GetEnumerator()
        {
            return new TabSeparatedValueFileEnumerator(this);
        }

        /// <summary>
        /// returns an TabSeparatedValueFileEnumerator
        /// </summary>
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new TabSeparatedValueFileEnumerator(this);
        }

        /// <summary>
        /// Resets the input stream to the beginning, which is a very good place to start.
        /// </summary>
        protected void ResetStream()
        {
            _dataStream.Seek(0, System.IO.SeekOrigin.Begin);
        }

        /// <summary>
        /// Closes the input stream
        /// </summary>
        public void Close()
        {
            if (_dataStream != null)
                _dataStream.Close();
        }

        #endregion





        /// <summary>
        /// Allows anyone to enumerate over a tab-separated-value file easily line by line
        /// 
        /// NOTE:
        /// This is not a thread safe way to access a TabSeparatedValueReader.
        /// (don't share a TabSeparatedValueReader, and really don't enumerate over it concurrently.
        /// you wouldn't share a stream, would you?)
        /// </summary>
        public class TabSeparatedValueFileEnumerator : IRowEnumerator
        {
            /// <summary>
            /// our pointer back to the collection
            /// </summary>
            TabSeparatedValueReader _parent;

            /// <summary>
            /// an easier way to parse our tsv stream...
            /// </summary>
            StreamReader _data;

            /// <summary>
            /// the last line we read from our datastream (during MoveNext)
            /// </summary>            
            protected string _currentLine = null;

            /// <summary>
            /// the internal array of column names for this file
            /// </summary>
            protected List<string> _columnNames = null;

            /// <summary>
            /// our constructor (would rather have this protected, but it wasn't having it...)
            /// </summary>
            public TabSeparatedValueFileEnumerator(TabSeparatedValueReader parent)
            {
                _parent = parent;
                _parent.ResetStream();
                
                _data = new StreamReader(_parent._dataStream);
                Reset();
                GetColumns();
            }


            #region IEnumerator<string[]> Members

            /// <summary>
            /// IEnumerator...
            /// </summary>
            public List<string> Current
            {
                get { return new List<string>(
                    _currentLine.Split(_parent._splitChars, StringSplitOptions.None)); }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                _currentLine = null;
                _data = null;
                _parent = null;
            }

            #endregion

            #region IEnumerator Members

            /// <summary>
            /// IEnumerator...
            /// </summary>
            public bool MoveNext()
            {
                if (_data.EndOfStream)
                    return false;

                //this is why we like StreamReaders...               
                _currentLine = _data.ReadLine();
                return true;
            }


            /// <summary>
            /// IEnumerator...
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return _currentLine.Split(_parent._splitChars, StringSplitOptions.None); }
            }

            /// <summary>
            /// IEnumerator...
            /// </summary>
            public void Reset()
            {
                //move back to before the data
                _data.BaseStream.Seek(0, SeekOrigin.Begin);
                _currentLine = null;
                _columnNames = null;
            }

            #endregion

            #region RowEnumerator Members

            public List<string> GetColumns()
            {
                if (_columnNames != null)
                    return _columnNames;

                if (!MoveNext())
                    return null;

                _columnNames = Current;
                return _columnNames;
            }

            #endregion
        }



    }
}

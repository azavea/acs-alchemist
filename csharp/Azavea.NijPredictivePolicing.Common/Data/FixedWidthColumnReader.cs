using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.IO;
using System.Collections;

namespace Azavea.NijPredictivePolicing.Common.Data
{
    /// <summary>
    /// Used primarily for reading binary files with constant-sized field entries.  Assumes rows are terminated with newlines.
    /// </summary>
    public class FixedWidthColumnReader : IDataFileReader, IEnumerable<List<string>>
    {
        
        private readonly ILog _log = LogManager.GetLogger(new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);

        /// <summary>
        /// an internal copy of our filename
        /// </summary>
        protected string _filename;

        /// <summary>
        /// our input stream
        /// </summary>
        protected System.IO.Stream _dataStream;

        /// <summary>
        /// List of columns for each row.  Columns are evaluated in order (this matters for elements with Field.Seeker = Field.Positions.FROM_CURRENT).  User is allowed to change between rows.
        /// </summary>
        public List<FixedWidthField> Columns;

        /// <summary>
        /// Set to true to skip the first line of the data file, and treat it as column names
        /// </summary>
        public bool FirstLineOfFileIsColumnNames = false;

        public FixedWidthColumnReader() { }

        public FixedWidthColumnReader(bool hasColumns) { FirstLineOfFileIsColumnNames = hasColumns; }

        public FixedWidthColumnReader(string filename, List<FixedWidthField> columns)
        {
            Columns = columns;
            FirstLineOfFileIsColumnNames = false;
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
                _dataStream = new System.IO.FileStream(
                    filename, 
                    System.IO.FileMode.Open, 
                    System.IO.FileAccess.Read, 
                    System.IO.FileShare.Read
                    );

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
        public List<string> TableNames()
        {
            //here for the interface
            return null;
        }

        /// <summary>
        /// here for the interface
        /// </summary>
        public void SetTablename(string tableName)
        {
            //here for the interface
        }

        /// <summary>
        /// returns an NullSeparatedValueReaderEnumerator
        /// </summary>
        IEnumerator<List<string>> System.Collections.Generic.IEnumerable<List<string>>.GetEnumerator()
        {
            return new FixedWidthColumnReaderEnumerator(this);
        }

        /// <summary>
        /// returns an NullSeparatedValueReaderEnumerator
        /// </summary>
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new FixedWidthColumnReaderEnumerator(this);
        }

        /// <summary>
        ///  Resets the input stream to the beginning, which is a very good place to start.
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
        public class FixedWidthColumnReaderEnumerator : IRowEnumerator
        {
            /// <summary>
            /// our pointer back to the collection
            /// </summary>
            FixedWidthColumnReader _parent;

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
            public FixedWidthColumnReaderEnumerator(FixedWidthColumnReader parent)
            {
                _parent = parent;
                _parent.ResetStream();
                
                _data = new StreamReader(_parent._dataStream);
                Reset();
                GetColumns();
            }


            #region IEnumerator<List<string>> Members

            /// <summary>
            /// IEnumerator...
            /// </summary>
            public List<string> Current
            {
                get 
                { 
                    var result = new List<string>(_parent.Columns.Count);
                    int currentPos = 0;
                    foreach(var field in _parent.Columns)
                    {
                        result.Add(field.GetFieldData(_currentLine, ref currentPos));
                    }

                    return result;
                }
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
                get 
                { 
                    var result = new List<object>(_parent.Columns.Count);
                    int currentPos = 0;
                    foreach(var field in _parent.Columns)
                    {
                        result.Add(field.GetFieldObject(_currentLine, ref currentPos));
                    }

                    return result;
                }
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
                if (!_parent.FirstLineOfFileIsColumnNames)
                {
                    if ((_parent.Columns != null) && (_parent.Columns.Count > 0))
                    {
                        var cols = _parent.Columns;
                        var result = new List<string>(cols.Count);
                        foreach (var col in cols)
                        {
                            result.Add(col.ColumnName);
                        }
                        return result;
                    }

                    return null;
                }

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

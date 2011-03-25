using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;


namespace Azavea.NijPredictivePolicing.Parsers
{
    public class CommaSeparatedValueReader : IDataFileReader
    {
        private readonly ILog _log = LogManager.GetLogger(new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);


        /// <summary>
        /// the tab, for the tab-separated part
        /// </summary>
        protected char[] _splitChars = new char[] { ',' };

        /// <summary>
        /// an internal copy of our filename
        /// </summary>
        protected string _filename;

        /// <summary>
        /// our input stream
        /// </summary>
        protected System.IO.Stream _dataStream;

        public CommaSeparatedValueReader() { }
        public CommaSeparatedValueReader(string filename)
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
        /// Creates a TabSeparatedValueFileEnumerator around this reader, which supports the RowEnumerator interface
        /// </summary>
        /// <returns></returns>
        public IRowEnumerator GetEnumerator()
        {
            return new CommaSeparatedValueFileEnumerator(this);
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
        public class CommaSeparatedValueFileEnumerator : IRowEnumerator
        {
            /// <summary>
            /// our pointer back to the collection
            /// </summary>
            CommaSeparatedValueReader _parent;

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
            protected string[] _columnNames = null;

            /// <summary>
            /// our constructor (would rather have this protected, but it wasn't having it...)
            /// </summary>
            public CommaSeparatedValueFileEnumerator(CommaSeparatedValueReader parent)
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
            public string[] Current
            {
                get { return _currentLine.Split(_parent._splitChars, StringSplitOptions.None); }
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

            public string[] GetColumns()
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

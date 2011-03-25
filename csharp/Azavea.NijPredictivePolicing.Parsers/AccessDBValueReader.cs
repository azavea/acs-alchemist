using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;

namespace Azavea.NijPredictivePolicing.Parsers
{
    public class AccessDBValueReader : IDataFileReader
    {
        /// <summary>
        /// the ms access database filename
        /// </summary>
        protected string _filename;

        /// <summary>
        /// the currently selected table name in the ms access database
        /// </summary>
        protected string _tablename;

        /// <summary>
        /// an open connection to the database (opened when constructed, or from LoadFile)
        /// </summary>
        protected System.Data.Common.DbConnection _conn;

        /// <summary>
        /// a one-time cached of tables available in the database
        /// </summary>
        protected List<string> _tablenames = null;

        /// <summary>
        /// The Default Constructor
        /// </summary>
        public AccessDBValueReader() { }

        /// <summary>
        /// Constructs a reader, and opens the data file
        /// </summary>
        /// <param name="filename"></param>
        public AccessDBValueReader(string filename)
        {
            LoadFile(filename);
        }


        #region IDataReader Members
        
        /// <summary>
        /// opens a DBConnection to the data file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool LoadFile(string filename)
        {
            _filename = filename;
            if (!System.IO.File.Exists(filename))
                return false;

            string connString = String.Format("Provider={0}; Data Source={1};", "Microsoft.Jet.OLEDB.4.0", _filename);
            _conn = new System.Data.OleDb.OleDbConnection(connString);
            _conn.Open();

            if ((_conn == null) || (_conn.State != System.Data.ConnectionState.Open))
                return false;

            return true;
        }

        //public bool LoadFile(Stream input)
        //{
        //    return false;
        //}

        /// <summary>
        /// returns true if the database connection is open and ready
        /// </summary>
        /// <returns></returns>
        public bool IsOpen()
        {
            return ((_conn != null) && (_conn.State == System.Data.ConnectionState.Open));
        }

        /// <summary>
        /// pulls a list of datatable names from the database and caches them.
        /// (this list does not refresh, because this reader is read-only)
        /// </summary>
        /// <returns></returns>
        public List<string> TableNames()
        {
            if (!IsOpen())
                return null;

            if ((_tablenames != null) && (_tablenames.Count > 0))
                return _tablenames;

            //general schema...
           // DataTable dt = _conn.GetSchema();

            DataTable dtTables = _conn.GetSchema("Tables");
            if ((dtTables != null) && (dtTables.Rows.Count > 0))
            {
                _tablenames = new List<string>();
                foreach (DataRow row in dtTables.Rows)
                {
                    string name = (row["TABLE_NAME"] as string);
                    if (name.StartsWith("MSys"))
                        continue;

                    _tablenames.Add(name);
                }
                return _tablenames;
            }

            return null;
        }

        //public string[] ColumnNames()
        //{
        //    return null;
        //}

        /// <summary>
        /// sets the selected table
        /// </summary>
        public void SetTablename(string tableName)
        {
            _tablename = tableName;
        }

        /// <summary>
        /// grabs an enumerator that will walk the rows in the selected table
        /// </summary>
        /// <returns></returns>
        public IRowEnumerator GetEnumerator()
        {
            return new AccessDBEnumerator(this);
        }

        /// <summary>
        /// Creates a local DataReader against the source db connection / datatable
        /// </summary>
        protected DbDataReader StartStream()
        {
            if (!IsOpen())
                return null;

            DbCommand cmd = _conn.CreateCommand();
            cmd.CommandText = String.Format("Select * from {0}", _tablename);
            cmd.CommandType = CommandType.Text;
       
            return cmd.ExecuteReader();            
        }

        /// <summary>
        /// Closes the data source
        /// </summary>
        public void Close()
        {
            if (IsOpen())
                _conn.Close();
            _conn = null;
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
        public class AccessDBEnumerator : IRowEnumerator
        {
            /// <summary>
            /// our pointer back to the collection
            /// </summary>
            protected AccessDBValueReader _parent;

            /// <summary>
            /// an easier way to parse our tsv stream...
            /// </summary>
            protected DbDataReader _data;

            /// <summary>
            /// the last line we read from our datastream (during MoveNext)
            /// </summary>
            protected string[] _currentLine = null;

            /// <summary>
            /// the internal array of column names for this file
            /// </summary>
            protected string[] _columnNames = null;

            /// <summary>
            /// our constructor (would rather have this protected, but it wasn't having it...)
            /// </summary>
            public AccessDBEnumerator(AccessDBValueReader parent)
            {
                _parent = parent;

                _data = _parent.StartStream();
                //Reset();
            }


            #region IEnumerator<string[]> Members

            /// <summary>
            /// IEnumerator...
            /// </summary>
            public string[] Current
            {
                get  { return _internalGetCurrent(); }
            }

            /// <summary>
            /// Reads a row of data from the AccessDB using our current data reader
            /// </summary>
            /// <returns></returns>
            protected string[] _internalGetCurrent()
            {
                if (_currentLine == null)
                    _currentLine = new string[_data.FieldCount];

                for (int i = 0, max = _data.FieldCount; i < max; i++)                
                    _currentLine[i] = String.Empty + _data.GetValue(i);
                
                return _currentLine;
            }

            #endregion

            #region IDisposable Members

            /// <summary>
            /// Disposes the current enumerator
            /// </summary>
            public void Dispose()
            {
                _currentLine = null;
                _data.Close();
                _data = null;
                _parent = null;
            }

            #endregion

            #region IEnumerator Members

            /// <summary>
            /// IEnumerator...
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return _internalGetCurrent(); }
            }

            /// <summary>
            /// IEnumerator...
            /// </summary>
            public bool MoveNext()
            {
                return _data.Read();
            }

            /// <summary>
            /// IEnumerator...
            /// </summary>
            public void Reset()
            {
                if (_data != null)
                {
                    _data.Close();
                }
                _data = _parent.StartStream();
            }

            #endregion

            #region RowEnumerator Members

            /// <summary>
            /// Gets a list of columns from the Access database for the current table
            /// </summary>
            public string[] GetColumns()
            {
                if (_columnNames != null)
                    return _columnNames;

                DataTable schemaDT = _data.GetSchemaTable();
                if ((schemaDT != null) && (schemaDT.Rows.Count > 0))
                {
                    _columnNames = new string[schemaDT.Rows.Count];
                    for (int i = 0; i < schemaDT.Rows.Count; i++)
                    {
                        _columnNames[i] = (schemaDT.Rows[i][0] as string);
                    }
                }
                return _columnNames;
            }

            #endregion
        }



    }
}


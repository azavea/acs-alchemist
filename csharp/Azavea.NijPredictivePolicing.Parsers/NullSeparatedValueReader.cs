using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.IO;

namespace Azavea.NijPredictivePolicing.Parsers
{
    /// <summary>
    /// Used primarily for reading binary files with constant-sized field entries.  Assumes rows are terminated with newlines.
    /// </summary>
    class NullSeparatedValueReader : IDataFileReader
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
        public List<Field> Columns;


        public NullSeparatedValueReader() { }

        public NullSeparatedValueReader(string filename, List<Field> columns)
        {
            Columns = columns;
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
        /// returns an GenericSeparatedValueFileEnumerator
        /// </summary>
        public IRowEnumerator GetEnumerator()
        {
            return new NullSeparatedValueReaderEnumerator(this);
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
        /// Represents a column, and contains data about where that column is located in a row and what type of data it contains.  This class contains some error checking, but it can be used in some very weird ways (for example, it doesn't care if columns overlap with each other).  In general, it's your responsibility to use it sensibly.
        /// </summary>
        public class Field
        {
            public Field() { }

            public Field(int start, int end, Types type, Positions seeker, Terminators terminator)
            {
                Start = start;
                End = end;
                Type = type;
                Seeker = seeker;
                Terminator = terminator;
            }

            public Field(int start, int end, Types type, Positions seeker, Terminators terminator, 
                object defaultValue, bool strict) : 
                    this(start, end, type, seeker, terminator)
            {
                DefaultValue = defaultValue;
                Strict = strict;
            }


            /// <summary>
            /// Determines where the record starts
            /// </summary>
            public int Start = -1;


            /// <summary>
            /// Determines where the record ends
            /// </summary>
            public int End = -1;

            /// <summary>
            /// What type is this field?
            /// </summary>
            public Types Type = Types.STRING;

            /// <summary>
            /// Determines how we interpret Start
            /// </summary>
            public Positions Seeker = Positions.FROM_START;

            /// <summary>
            /// Determines how we interpret End
            /// </summary>
            public Terminators Terminator = Terminators.LENGTH;

            /// <summary>
            /// If we fail to retrieve a value for this field, what should we use instead?  Note this isn't checked against Type, so if you set this to one type and Type to something else, it's your fault when you get type errors.
            /// </summary>
            public object DefaultValue = null;

            /// <summary>
            /// If true, throws exceptions on error.  If false, silently uses DefaultValue and continues.
            /// </summary>
            public bool Strict = true;

            /// <summary>
            /// Determines how the field is parsed and what type of object is returned
            /// </summary>
            public enum Types
            {
                NULL = 0,
                STRING,
                INT,
                LONG,
                FLOAT,
                DOUBLE,
                DECIMAL,
                DATETIME
            }


            /// <summary>
            /// Determines where to start reading an entry, or how Start is interpreted
            /// </summary>
            public enum Positions
            {
                /// <summary>
                /// Start is number of characters from start of row.  Start must be positive.
                /// </summary>
                FROM_START = 0,

                /// <summary>
                /// Start is number of characters from end of row.  Start must be positive.
                /// </summary>
                FROM_END,

                /// <summary>
                /// Start is number of characters from current position.  
                /// Start may be positive or negative to denote direction.
                /// </summary>
                FROM_CURRENT
            }

            /// <summary>
            /// Determines where to stop reading an entry, or how End is interpreted
            /// </summary>
            public enum Terminators
            {
                /// <summary>
                /// Field ends with newline, End is ignored
                /// </summary>
                NEWLINE,

                /// <summary>
                /// Field has a static length, End denotes the size of the field
                /// </summary>
                LENGTH,

                /// <summary>
                /// End is a offset index from the start of the line corresponding to the last character to read for this field.  If End &lt; Start, an exception is thrown.
                /// </summary>
                INDEX
            }

            /// <summary>
            /// Gets the raw data for this field from a row and updates the current position.  Does not do any parsing.  Throws errors if Strict is false.
            /// </summary>
            /// <param name="row">The row to read from</param>
            /// <param name="currentPos">The current position in the row</param>
            /// <returns>Field data in string form</returns>
            public string GetFieldData(string row, ref int currentPos)
            {
                try
                {
                    int myStart = 0;
                    int myEnd = 0;
                    GetFieldRange(row, currentPos, out myStart, out myEnd);
                    currentPos = myEnd + 1;

                    return row.Substring(myStart, myEnd - myStart);
                }
                catch (Exception ex)
                {
                    if (Strict)
                        throw ex;
                    else
                        return DefaultValue.ToString();
                }
            }

            /// <summary>
            /// Gets the parsed object for this field from a row and updates the current position, as determined by Type.
            /// </summary>
            /// <param name="row">The row to read from</param>
            /// <param name="currentPos">The current position in the row</param>
            /// <returns>Field data in an appropriate object</returns>
            public object GetFieldObject(string row, ref int currentPos)
            {
                try
                {
                    string data = GetFieldData(row, ref currentPos);
                    object result = null;
                    switch (this.Type)
                    {
                        case Types.NULL:
                            break;

                        case Types.STRING:
                            result = data;
                            break;

                        case Types.INT:
                            result = int.Parse(data);
                            break;

                        case Types.LONG:
                            result = long.Parse(data);
                            break;

                        case Types.FLOAT:
                            result = float.Parse(data);
                            break;

                        case Types.DOUBLE:
                            result = double.Parse(data);
                            break;

                        case Types.DECIMAL:
                            result = decimal.Parse(data);
                            break;

                        case Types.DATETIME:
                            result = DateTime.Parse(data);
                            break;

                        default:
                            throw new NotImplementedException("Invalid value for Type");
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    if (Strict)
                        throw ex;
                    else
                        return DefaultValue;
                }
            }

            private void GetFieldRange(string row, int currentPos, out int start, out int end)
            {
                if (string.IsNullOrEmpty(row))
                {
                    start = end = 0;
                    return;
                }

                int rowSize = row.Length;

                switch (this.Seeker)
                {
                    case Positions.FROM_START:
                        if (Start < 0)
                            throw new ArgumentOutOfRangeException("Start", Start, "Seeker is FROM_START, but Start < 0");
                        start = Start;
                        if (start >= rowSize)
                            throw new ArgumentOutOfRangeException("Start", Start, "Seeker is FROM_START, but value of Start results in out of range index");
                        break;

                    case Positions.FROM_END:
                        if (Start < 0)
                            throw new ArgumentOutOfRangeException("Start", Start, "Seeker is FROM_END, but Start < 0");
                        start = rowSize - 1 - Start;
                        if (start >= rowSize)
                            throw new ArgumentOutOfRangeException("Start", Start, "Seeker is FROM_END, but value of Start results in out of range index");
                        break;

                    case Positions.FROM_CURRENT:
                        start = currentPos + Start;
                        if (start < 0 || start >= rowSize)
                            throw new ArgumentOutOfRangeException("Start", Start, "Seeker is FROM_CURRENT, but value of current position and Start results in out of range index");
                        break;

                    default:
                        throw new NotImplementedException("Invalid Seeker value");
                }

                switch (this.Terminator)
                {
                    case Terminators.NEWLINE:
                        end = rowSize - 1;
                        break;

                    case Terminators.LENGTH:
                        if (End < 0)
                            throw new ArgumentOutOfRangeException("End", End, "Terminator is LENGTH, but End < 0");
                        end = start + End;
                        if (end >= rowSize)
                            throw new ArgumentOutOfRangeException("End", End, "Terminator is LENGTH, but value of current position results in out of range index");
                        break;

                    case Terminators.INDEX:
                        if (End < 0 || End >= rowSize)
                            throw new ArgumentOutOfRangeException("End", End, "Terminator is INDEX, but End is out of range");
                        else if (End < start)
                            throw new ArgumentOutOfRangeException("End", End, "Terminator is INDEX, but End < Start");
                        else
                            end = End;
                        break;

                    default:
                        throw new NotImplementedException("Invalid Terminator Value");
                }
            }
        }


        /// <summary>
        /// Allows anyone to enumerate over a tab-separated-value file easily line by line
        /// 
        /// NOTE:
        /// This is not a thread safe way to access a TabSeparatedValueReader.
        /// (don't share a TabSeparatedValueReader, and really don't enumerate over it concurrently.
        /// you wouldn't share a stream, would you?)
        /// </summary>
        public class NullSeparatedValueReaderEnumerator : IRowEnumerator
        {
            /// <summary>
            /// our pointer back to the collection
            /// </summary>
            NullSeparatedValueReader _parent;

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
            public NullSeparatedValueReaderEnumerator(NullSeparatedValueReader parent)
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

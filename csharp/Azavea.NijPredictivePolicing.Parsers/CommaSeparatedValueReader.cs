using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using System.Collections;


namespace Azavea.NijPredictivePolicing.Parsers
{
    public class CommaSeparatedValueReader : IDataFileReader, IEnumerable<List<string>>
    {
        private readonly ILog _log = LogManager.GetLogger(new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().DeclaringType.Namespace);


        /// <summary>
        /// Some countries use other characters such as ;
        /// </summary>
        protected const char Comma = ',';

        /// <summary>
        /// an internal copy of our filename
        /// </summary>
        protected string _filename;

        /// <summary>
        /// our input stream
        /// </summary>
        protected System.IO.Stream _dataStream;

        public bool HasColumns = false;

        public CommaSeparatedValueReader() { }

        public CommaSeparatedValueReader(bool hasColumns) { HasColumns = hasColumns; }

        public CommaSeparatedValueReader(string filename, bool hasColumns)
        {
            LoadFile(filename);
            HasColumns = hasColumns;
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
        /// returns an CommaSeparatedValueFileEnumerator
        /// </summary>
        IEnumerator<List<string>> System.Collections.Generic.IEnumerable<List<string>>.GetEnumerator()
        {
            return new CommaSeparatedValueFileEnumerator(this);
        }

        /// <summary>
        /// returns an CommaSeparatedValueFileEnumerator
        /// </summary>
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
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
            /// the last line we read from our datastream (during MoveNext)
            /// </summary>            
            protected List<string> _currentRow = null;

            /// <summary>
            /// the internal array of column names for this file
            /// </summary>
            protected List<string> _columnNames = null;

            /// <summary>
            /// our constructor (would rather have this protected, but it wasn't having it...)
            /// </summary>
            public CommaSeparatedValueFileEnumerator(CommaSeparatedValueReader parent)
            {
                _parent = parent;
                _parent.ResetStream();
                
                Reset();
                GetColumns();
            }

            /// <summary>
            /// Used to seek to next comma or newline.  If the current char is a , or a newline, it returns -a value less than 0 and advances to the next field (either 1 or 2 places depending on CRLF).  Otherwise, it advances 1 and returns the character read.
            /// </summary>
            /// <param name="fs">The filestream to read</param>
            /// <returns>-1 on comma encountered, -2 on newline encountered, -3 on EOF encountered, character read otherwise</returns>
            protected static int IsCurrentCommaOrNewLine(FileStream fs)
            {
                int temp = fs.ReadByte();
                char c = (char)temp;

                if (c == Comma)
                    return -1;
                else if (c == '\n')
                    return -2;
                else if (temp == -1)
                    return -3;

                //Could be followed by '\n' (stupid windows) so we have to check
                else if (c == '\r')
                {
                    int next = fs.ReadByte();

                    if ((char)next != '\n')
                    {
                        //Oops, went too far, go back one
                        fs.Seek(-1, SeekOrigin.Current);
                    }

                    return -2;
                }

                return (int)c;
            }

            /// <summary>
            /// Gets the next field from fs
            /// </summary>
            /// <param name="fs">The filestream to read, positioned at the start of the field</param>
            /// <param name="lastFieldInRow">Set to true if this is the last field in the row, false otherwise</param>
            /// <returns></returns>
            public static string GetNextField(FileStream fs, ref bool lastFieldInRow)
            {
                StringBuilder buffer = new StringBuilder(1024);

                int first;
                if (fs.CanRead && (fs.Position < fs.Length))
                    first = IsCurrentCommaOrNewLine(fs);
                //EOF
                else
                {
                    lastFieldInRow = true;
                    return "";
                }

                //Comma
                if (first == -1)
                {
                    lastFieldInRow = false;
                    return "";
                }

                //Newline
                else if (first == -2)
                {
                    lastFieldInRow = true;
                    return "";
                }

                //EOF (Should never get here)
                else if (first == -3)
                {
                    lastFieldInRow = true;
                    return "";
                }

                char c = (char)first;
                if (c == '\"')
                {
                    while (fs.CanRead && (fs.Position < fs.Length))
                    {
                        //Calling IsCurrentCommaOrNewLine allows us to convert all newlines to '\n'
                        int next = IsCurrentCommaOrNewLine(fs);
                        if (next == -1)
                        {
                            buffer.Append(Comma);
                            continue;
                        }
                        else if (next == -2)
                        {
                            buffer.Append('\n');
                            continue;
                        }
                        else if (next < 0) throw new Exception("Error in CSV parser, please debug");

                        c = (char)next;
                        if (c == '\"')
                        {
                            next = fs.ReadByte();
                            c = (char)next;

                            //End of input, we're done
                            if (next < 0)
                            {
                                lastFieldInRow = true;
                                break;
                            }

                            //First " was escaped, add one quote and keep going
                            else if (c == '\"')
                            {
                                buffer.Append('\"');
                                continue;
                            }

                            //End of quoted string, seek until next , and then exit
                            else
                            {
                                //Seek to end of field
                                if (c == Comma)
                                {
                                    lastFieldInRow = false;
                                    break;
                                }
                                else
                                {
                                    //This silently throws away data in malformed CSVs; better way to do it?
                                    while ((next = IsCurrentCommaOrNewLine(fs)) >= 0) ;
                                    //Return true if newline or eof encountered, false if ,
                                    lastFieldInRow = (next == -1) ? false : true;
                                    break;
                                }
                            }
                        }
                        else
                            buffer.Append(c);
                    }
                }
                else
                {
                    buffer.Append(c);
                    int temp;
                    while ((temp = IsCurrentCommaOrNewLine(fs)) >= 0)
                    {
                        buffer.Append((char)temp);
                    }
                    lastFieldInRow = (temp == -1) ? false : true;
                }

                return buffer.ToString();
            }


            protected List<string> GetNextRow(FileStream fs)
            {
                var result = new List<string>((_columnNames != null) ? _columnNames.Count : 32);
                bool lastField = false;
                string currentField;

                while (!lastField)
                {
                    currentField = GetNextField(fs, ref lastField);
                    result.Add(currentField);
                }

                return result;
            }

            #region IEnumerator<List<string>> Members

            /// <summary>
            /// IEnumerator...
            /// </summary>
            public List<string> Current
            {
                get { return _currentRow; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                _currentRow = null;
                _parent = null;
            }

            #endregion

            #region IEnumerator Members

            /// <summary>
            /// IEnumerator...
            /// </summary>
            public bool MoveNext()
            {
                if (_parent._dataStream.Position >= _parent._dataStream.Length || 
                    !_parent._dataStream.CanRead)
                    return false;

                _currentRow = GetNextRow(_parent._dataStream as FileStream);
                return true;
            }


            /// <summary>
            /// IEnumerator...
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get { return _currentRow; }
            }

            /// <summary>
            /// IEnumerator...
            /// </summary>
            public void Reset()
            {
                //move back to before the data
                _parent._dataStream.Seek(0, SeekOrigin.Begin);
                _currentRow = null;
                _columnNames = null;
            }

            #endregion

            #region RowEnumerator Members

            public List<string> GetColumns()
            {
                if (!_parent.HasColumns)
                    return null;

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

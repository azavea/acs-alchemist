using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using ADOX;


namespace Azavea.NijPredictivePolicing.Parsers
{
    /// <summary>
    /// a basic implementation of the IDataWriter interface, so we can simply export tab separated value files
    /// </summary>
    public class AccessDBValueWriter : IDataFileWriter
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
        /// a reusable parameterized insert command to help speed up our writer.
        /// </summary>
        protected DbCommand _insertCmd = null;

        /// <summary>
        /// construct a new blank writer
        /// (any writes will throw exceptions if you don't set a stream or a file!)
        /// </summary>
        public AccessDBValueWriter() { }

        /// <summary>
        /// construct a new writer to append to the given file
        /// </summary>
        /// <param name="filename"></param>
        public AccessDBValueWriter(string filename)
        {
            SetWriteFile(filename);
        }

        #region IDataWriter Members

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


        public void SetTablename(string tableName)
        {
            _tablename = tableName;
        }

        /// <summary>
        /// Opens or creates an MS Access database file for writing
        /// </summary>
        public bool SetWriteFile(string filename)
        {
            _filename = filename;

            string connString = String.Format("Provider={0}; Data Source={1};", "Microsoft.Jet.OLEDB.4.0", _filename);
            if (!System.IO.File.Exists(filename))
            {
                ADOX.Catalog arghCatalog = new ADOX.Catalog();
                arghCatalog.Create(connString);
            }

            //if the file doesn't exist, do I need a different connection string?
            _conn = new System.Data.OleDb.OleDbConnection(connString);
            _conn.Open();

            if ((_conn == null) || (_conn.State != System.Data.ConnectionState.Open))
                return false;

            return true;        
        }

        /// <summary>
        /// returns true if the database connection is open and ready
        /// </summary>
        /// <returns></returns>
        public bool IsOpen()
        {
            return ((_conn != null) && (_conn.State == System.Data.ConnectionState.Open));
        }


        public bool CreateTable(string tablename, IEnumerable<string> columns)
        {
            if (!IsOpen())
                return false;

            TableNames();
            if ((_tablenames != null) && (_tablenames.Contains(tablename)))
                return false;

            StringBuilder nt = new StringBuilder(1024);

            nt.AppendFormat("CREATE TABLE {0} (", tablename);

            bool firstThru = true;
            foreach (string col in columns)
            {
                if (!firstThru)
                    nt.Append(", ");

                //if (col.ToLower().EndsWith("id"))
                //{
                //    nt.Append(col).Append(" INT");
                //}
                //else
                //{
                    nt.Append(col).Append(" VARCHAR(250)");
                //}
          
                firstThru = false;
            }
            nt.Append("); ");

            DbCommand cmd = _conn.CreateCommand();
            cmd.CommandText = nt.ToString();
            int numResults = cmd.ExecuteNonQuery();
            _tablenames.Add(tablename);
            return true;
        }

        

        //public bool WriteLine(IEnumerable<object> values)
        //{
        //    return WriteLine((IEnumerable<string>)values);
        //}

        /// <summary>
        /// Make sure you have a value for every column!
        /// This is really intentionally not thread safe, do not share this class across threads.
        /// </summary>
        public bool WriteLine(IEnumerable<string> values)
        {
            if (!IsOpen())
                return false;

            if (_insertCmd == null)
            {
                DbCommand cmd = _conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM " + _tablename;

                DbDataAdapter dda = new System.Data.OleDb.OleDbDataAdapter((OleDbCommand)cmd);
                System.Data.OleDb.OleDbCommandBuilder odcb = new System.Data.OleDb.OleDbCommandBuilder((OleDbDataAdapter)dda);

                _insertCmd = odcb.GetInsertCommand();
            }

            //double temp = Double.NaN;

            int i = 0;
            foreach (string val in values)
            {
                //if (Double.TryParse(val, out temp))
                //{
                //    //try to parse out integers, float vars (lat/lon), ids, anything numeric
                //    _insertCmd.Parameters[i].Value = temp;
                //}
                //else if (_insertCmd.Parameters[i].SourceColumn.ToLower().EndsWith("id"))
                //{
                //    //if the value is empty, and it HAS to be a number...
                //    _insertCmd.Parameters[i].Value = 0;
                //}
                //else
                //{
                    _insertCmd.Parameters[i].Value = val;
                //}


                i++;
            }

            int rows = _insertCmd.ExecuteNonQuery();
            return (rows == 1);
        }

        /// <summary>
        /// flushes the stream
        /// </summary>
        public bool Flush()
        {            
            return true;
        }

        /// <summary>
        /// closes the underlying stream
        /// </summary>
        public bool Close()
        {
            if (IsOpen())
            {
                _conn.Close();
            }
            return true;
        }

        #endregion



    }
}

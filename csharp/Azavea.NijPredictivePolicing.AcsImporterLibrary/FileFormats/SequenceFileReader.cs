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
        public bool HasFile = false;

        public SequenceFileReader(string filename)
        {
            this.HasFile = File.Exists(filename);
            if(HasFile)
            {
                FileStream input = new FileStream(filename, FileMode.Open, FileAccess.Read);
                _reader = ExcelReaderFactory.CreateBinaryReader(input) as ExcelBinaryReader;
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
            _columns = null;
        }
    }
}

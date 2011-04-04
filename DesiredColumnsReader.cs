using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using Azavea.NijPredictivePolicing.Common.Data;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats
{
    /// <summary>
    /// Input should be a csv list of (CENSUS_TABLE_ID, Name) pairs, where CENSUS_TABLE_ID is a foreign key into columnMappings.CENSUS_TABLE_ID.  Name is it's description, and is optional.
    /// </summary>
    public class DesiredColumnsReader : IDisposable
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected CommaSeparatedValueReader _reader;

        public bool HasFile = false;

        public DesiredColumnsReader(string filename)
        {
            this.HasFile = File.Exists(filename);
            if (HasFile)
            {
                _reader = new CommaSeparatedValueReader(filename, false);
            }
        }

        public CommaSeparatedValueReader GetReader()
        {
            return _reader;
        }

        public string TableName = "desiredColumns";

        private string _tableGenerationTemplate = @"DROP TABLE IF EXISTS {0}; CREATE TABLE {0} (
            CENSUS_TABLE_ID VARCHAR(32) NOT NULL PRIMARY KEY,
            CUSTOM_COLUMN_NAME VARCHAR(10));";

        public string TableGenerationSql
        {
            get
            {
                StringBuilder sql = new StringBuilder(_tableGenerationTemplate.Length + 128);
                return sql.AppendFormat(_tableGenerationTemplate, TableName).ToString();
            }
        }

        public void Dispose()
        {
            _reader.Close();
        }
    }
}

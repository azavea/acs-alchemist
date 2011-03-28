using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Azavea.NijPredictivePolicing.Common.Data
{
    /// <summary>
    /// This helps us more easily abstract away writing to our data sources
    /// (be they excel, access, tsv, etc...)
    /// </summary>
    public interface IDataFileWriter
    {
        /// <summary>
        /// Set the output filename
        /// </summary>
        bool SetWriteFile(string filename);

        //bool SetWriteStream(Stream input);

        /// <summary>
        /// Write values to the output stream
        /// </summary>
        bool WriteLine(IEnumerable<string> values);

        /// <summary>
        /// Change the current tablename
        /// </summary>
        void SetTablename(string tableName);

        /// <summary>
        /// Create a new output table
        /// </summary>
        bool CreateTable(string tablename, IEnumerable<string> columns);

        /// <summary>
        /// Flush the output stream
        /// </summary>
        /// <returns></returns>
        bool Flush();

        /// <summary>
        /// Close the output stream
        /// </summary>
        /// <returns></returns>
        bool Close();
    }
}

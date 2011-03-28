using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Azavea.NijPredictivePolicing.Parsers
{
    /// <summary>
    /// This helps us more easily abstract away interacting with our data sources
    /// (be they excel, access, tsv, etc...)
    /// </summary>
    public interface IDataFileReader
    {
        /// <summary>
        /// Load the specified input file
        /// </summary>
        bool LoadFile(string filename);

        /// <summary>
        /// Closes the current data source
        /// </summary>
        void Close();

        /// <summary>
        /// Gets a list of the tables contained by the data source
        /// </summary>
        /// <returns></returns>
        List<string> TableNames();

        /// <summary>
        /// Sets the current tablename
        /// </summary>
        /// <param name="tableName"></param>
        void SetTablename(string tableName);
    }
}

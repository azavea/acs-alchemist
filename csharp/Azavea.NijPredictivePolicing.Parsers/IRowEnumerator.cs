using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azavea.NijPredictivePolicing.Parsers
{
    /// <summary>
    /// Just like an IEnumerator for an array of strings, except this gives us a list of column names too
    /// </summary>
    public interface IRowEnumerator : IEnumerator<List<string>>
    {
        /// <summary>
        /// Get a list of column names for this enumerator (so we know what order the values are in)
        /// </summary>
        List<string> GetColumns();
    }
}

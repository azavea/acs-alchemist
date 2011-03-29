using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Azavea.NijPredictivePolicing.Test.Helpers
{
    public class Utilities
    {
        /// <summary>
        /// Reads the entire contents of a file into a string.
        /// Pretty inefficient, but don't really care for testing purposes.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string FileToString(string filename)
        {
            FileStream reader = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StringBuilder result = new StringBuilder((int)reader.Length);
            while (reader.CanRead && reader.Position < reader.Length)
            {
                result.Append((char)reader.ReadByte());
            }

            return result.ToString();
        }
    }
}

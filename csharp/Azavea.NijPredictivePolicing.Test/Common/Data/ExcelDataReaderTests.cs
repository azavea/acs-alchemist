/*
  Copyright (c) 2012 Azavea, Inc.
 
  This file is part of ACS Alchemist.

  ACS Alchemist is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  ACS Alchemist is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with ACS Alchemist.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using log4net;
using System.Net;
using System.IO;
using Azavea.NijPredictivePolicing.Common;
using Azavea.NijPredictivePolicing.Test.Helpers;
using Azavea.NijPredictivePolicing.ACSAlchemistLibrary;
using Excel;
using System.Data;
using Azavea.NijPredictivePolicing.Common.Data;

namespace Azavea.NijPredictivePolicing.Test.Common.Data
{
    [TestFixture]
    public class ExcelDataReaderTests
    {
        public const string InputDirectory = @"C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\Azavea.NijPredictivePolicing.Test\TestData\ParserTests";

        public const string OutputDirectory = "output\\";

        public readonly string InputFile = Path.Combine(InputDirectory, "Seq10.xls");

        public readonly string OutputFile = Path.Combine(OutputDirectory, "Seq10.csv");

        public readonly string CheckFile = Path.Combine(InputDirectory, "Seq10.csv");

        private static ILog _log = null;


        [TestFixtureSetUp]
        public void Init()
        {
            _log = LogHelpers.ResetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);

            if (File.Exists(OutputFile))
                File.Delete(OutputFile);
        }

        /// <summary>
        /// Basic proof of concept code for reading excel files
        /// </summary>
        [Test]
        public void ReadExcelFile()
        {
            FileStream input = new FileStream(InputFile, FileMode.Open, FileAccess.Read);
            IExcelDataReader reader = ExcelReaderFactory.CreateBinaryReader(input);
            IDataFileWriter writer = new CommaSeparatedValueWriter(OutputFile);

            reader.IsFirstRowAsColumnNames = false;
            DataSet result = reader.AsDataSet();

            foreach (DataTable t in result.Tables)
            {
                writer.WriteLine(new string[] { t.TableName });
                List<string> columns = new List<string>();

                foreach (DataColumn column in t.Columns)
                {
                    columns.Add(column.ColumnName);
                }

                writer.WriteLine(columns);

                foreach (DataRow row in t.Rows)
                {
                    List<string> fields = new List<string>();
                    foreach (object f in row.ItemArray)
                    {
                        fields.Add(f.ToString());
                    }

                    writer.WriteLine(fields);
                }

                writer.WriteLine(null);
            }

            writer.Close();
            reader.Close();


            Assert.AreEqual(File.ReadAllText(OutputFile), File.ReadAllText(CheckFile));
        }
    }
}

/*
  Copyright (c) 2012 Azavea, Inc.
 
  This file is part of _SOLUTIONNAME_.

  _SOLUTIONNAME_ is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  _SOLUTIONNAME_ is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with _SOLUTIONNAME_.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using Azavea.NijPredictivePolicing.Common.Data;

namespace Azavea.NijPredictivePolicing.Test.Common.Data
{
    [TestFixture]
    public class CommaSeparatedValueTests
    {
        public string InputDirectory = @"..\..\TestData\ParserTests";

        public const string WellFormedCsvFile = "wellFormed.csv";

        [TestFixtureSetUp]
        public void Setup()
        {
            if (!Directory.Exists(InputDirectory))
            {
                InputDirectory = "..\\" + InputDirectory;
                if (!Directory.Exists(InputDirectory))
                    Assert.Fail("Error: input directory not found at {0}", InputDirectory);
            }

            if (!File.Exists(Path.Combine(InputDirectory, WellFormedCsvFile)))
            {
                Assert.Fail("Error: {0} does not exist in {1}", WellFormedCsvFile, InputDirectory);
            }
        }

        [Test]
        public void ReaderTest()
        {
            /* CSV Contents:
                foo,bar,  whitespace  ," quotes "" , and commas "
                "quoted, with
                newline"

                ,
                ,,
                blarg
                blarg,"quoted, with
                newline"
                "quoted, with
                newline",blarg
                blarg,"quoted, with
                newline",blarg
             */

            CommaSeparatedValueReader reader = new CommaSeparatedValueReader();
            string filename = Path.Combine(InputDirectory, WellFormedCsvFile);

            var expected = new List<string[]>(new string[][] {
                new string[] { "foo", "bar", "  whitespace  ", @" quotes "" , and commas " },
                new string[] { "quoted, with\r\nnewline" },
                new string[] { "" },                  //empty line
                new string[] { "", "" },              //single ,
                new string[] { "", "", "" },          //,,
                new string[] { "blarg" },
                new string[] { "blarg", "quoted, with\r\nnewline" },
                new string[] { "quoted, with\r\nnewline", "blarg" },
                new string[] { "blarg", "quoted, with\r\nnewline", "blarg" }
            });
                    
            reader.LoadFile(filename);

            int i = 0;
            foreach (var row in reader)
            {
                IList<string> expectedRow = expected[i] as IList<string>;

                Assert.AreEqual(expectedRow.Count, row.Count, 
                    string.Format("Number of actual fields differs from number of expected at line {0} in file {1}", i, filename));

                for (int j = 0; j < row.Count; j++)
                {
                    Assert.AreEqual(expectedRow[j], row[j]);
                }

                i++;
            }
        }
    }
}

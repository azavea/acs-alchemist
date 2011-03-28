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
    class CommaSeparatedValueTests
    {
        public const string InputDirectory = @"E:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\Azavea.NijPredictivePolicing.Test\TestData\ParserTests";

        public const string WellFormedCsvFile = "wellFormed.csv";

        [TestFixtureSetUp]
        public void Setup()
        {
            if (!Directory.Exists(InputDirectory))
            {
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
            CommaSeparatedValueReader reader = new CommaSeparatedValueReader();
            string filename = Path.Combine(InputDirectory, WellFormedCsvFile);

            var expected = new List<string[]>();
            expected.Add(new string[] { "foo", "bar", "  whitespace  ", @" quotes "" , and commas " });
            expected.Add(new string[] { "quoted, with\nnewline" });
            expected.Add(new string[] { "" });                //empty line
            expected.Add(new string[] { "", "" });          //single ,
            expected.Add(new string[] { "", "", "" });    //,,
            expected.Add(new string[] { "blarg" });
            expected.Add(new string[] { "blarg", "quoted, with\nnewline" });
            expected.Add(new string[] { "quoted, with\nnewline", "blarg" });
            expected.Add(new string[] { "blarg", "quoted, with\nnewline", "blarg" });

            reader.LoadFile(filename);

            int i = 0;
            foreach (var row in reader)
            {
                IList<string> expectedRow = expected[i] as IList<string>;
                Assert.AreEqual(row.Count, expectedRow.Count,
                    string.Format("Number of actual fields differs from number of expected at line {0} in file {1}", i, filename));

                int count = row.Count;
                for (int j = 0; j < count; j++)
                {
                    Assert.AreEqual(row[j], expectedRow[j]);
                }

                i++;
            }
        }
    }
}

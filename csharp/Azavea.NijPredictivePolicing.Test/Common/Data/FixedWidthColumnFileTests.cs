using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using Field = Azavea.NijPredictivePolicing.Parsers.NullSeparatedValueReader.Field;
using Positions = Azavea.NijPredictivePolicing.Parsers.NullSeparatedValueReader.Field.Positions;
using Types = Azavea.NijPredictivePolicing.Parsers.NullSeparatedValueReader.Field.Types;
using Terminators = Azavea.NijPredictivePolicing.Parsers.NullSeparatedValueReader.Field.Terminators;
using System.Collections;

namespace Azavea.NijPredictivePolicing.Test.Common.Data
{
    [TestFixture]
    public class FixedWidthColumnFileTests
    {
        public const string InputDirectory = @"E:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\Azavea.NijPredictivePolicing.Test\TestData\ParserTests";

        public const string InputFile = "fixedWidth.txt";

        [TestFixtureSetUp]
        public void Setup()
        {
            if (!Directory.Exists(InputDirectory))
            {
                Assert.Fail("Error: input directory not found at {0}", InputDirectory);
            }

            if (!File.Exists(Path.Combine(InputDirectory, InputFile)))
            {
                Assert.Fail("Error: {0} does not exist in {1}", InputFile, InputDirectory);
            }
        }

        [Test]
        public void FixedWidthInputTest()
        {
            #region Setup

            NullSeparatedValueReader reader = new NullSeparatedValueReader();
            string filename = Path.Combine(InputDirectory, InputFile);
            reader.LoadFile(filename);

            var expectedStrings = new List<string[]>();
            var expectedObjects = new List<object[]>();
            var fields = new List<List<Field>>();
            List<Field> row;
            Field temp, template;

            /***********************************************************************************
             * First row tests Seeker = FROM_START, Terminator = LENGTH, types int and float
             * Data = "this is some text25      13373.133712/20/18342:32 PM12 November 1978 15:38:24"
             ***********************************************************************************/
            expectedStrings.Add(new string[] { 
                "this is some text25      ", 
                "1337", 
                "3.1337", 
                "12/20/1834", 
                "2:32 PM", 
                "12 November 1978 15:38:24"
                });

            expectedObjects.Add(new object[] { 
                "this is some text25      ",
                (int)1337,
                (float)3.1337,
                DateTime.Parse("12/20/1834"),
                DateTime.Parse("2:32 PM"),
                DateTime.Parse("12 November 1978 15:38:24")
                });

            row = new List<Field>();
            template = temp = new Field(0, 25, Types.STRING, Positions.FROM_START, Terminators.LENGTH);
            row.Add(temp);

            temp = new Field(template);
            temp.Start = 25;
            temp.End = 4;
            temp.Type = Types.INT;
            row.Add(temp);

            temp = new Field(template);
            temp.Start = 29;
            temp.End = 6;
            temp.Type = Types.FLOAT;
            row.Add(temp);

            temp = new Field(template);
            temp.Start = 35;
            temp.End = 10;
            temp.Type = Types.DATETIME;
            row.Add(temp);

            temp = new Field(template);
            temp.Start = 45;
            temp.End = 7;
            temp.Type = Types.DATETIME;
            row.Add(temp);

            temp = new Field(template);
            temp.Start = 52;
            temp.End = 25;
            temp.Type = Types.DATETIME;
            row.Add(temp);

            fields.Add(row);
            row = new List<Field>();

            /****************************************************************************************/


            /***********************************************************************************
             * Second row tests Seeker = FROM_CURRENT, otherwise same as first
             * Data = "this is some text25      13373.133712/20/18342:32 PM12 November 1978 15:38:24"
             **********************************************************************************/
            expectedStrings.Add(expectedStrings[0]);
            expectedObjects.Add(expectedObjects[0]);

            foreach (Field f in fields[0])
            {
                temp = new Field(f);
                temp.Seeker = Positions.FROM_CURRENT;
                temp.Start = 0;
                row.Add(temp);
            }

            fields.Add(row);
            row = new List<Field>();

            /****************************************************************************************/


            /***********************************************************************************
             * Third row tests Seeker = FROM_START, Terminator = INDEX, types long and double
             * Data = "this is some text25      13373.133712/20/18342:32 PM12 November 1978 15:38:24"
             **********************************************************************************/
            expectedStrings.Add(expectedStrings[0]);

            expectedObjects.Add(new object[] { 
                "this is some text25      ",
                (long)1337,
                (double)3.1337,
                DateTime.Parse("12/20/1834"),
                DateTime.Parse("2:32 PM"),
                DateTime.Parse("12 November 1978 15:38:24")
                });

            foreach (Field f in fields[0])
            {
                temp = new Field(f);
                temp.Terminator = Terminators.INDEX;

               //Used to be start index and length, converting to start index and end index
                temp.End = temp.Start + temp.End;
                if (temp.Type == Types.FLOAT) temp.Type = Types.DOUBLE;
                if (temp.Type == Types.INT) temp.Type = Types.LONG;
                row.Add(temp);
            }

            fields.Add(row);
            row = new List<Field>();

            /****************************************************************************************/


            /***********************************************************************************
             * Fourth row tests Seeker = FROM_END, Terminator = NEWLINE, and overlapping records
             * Data = "0123456789"
             **********************************************************************************/

            expectedStrings.Add(new string[] { "34", "0123456789" });
            expectedObjects.Add(expectedStrings[expectedStrings.Count - 1] as object[]);

            row.Add(new Field(7, 2, Types.STRING, Positions.FROM_END, Terminators.LENGTH));
            row.Add(new Field(0, 43847283, Types.STRING, Positions.FROM_START, Terminators.NEWLINE));

            fields.Add(row);
            row = null;

            /****************************************************************************************/

            #endregion Setup




            Assert.AreEqual(expectedObjects.Count, expectedStrings.Count);
            Assert.AreEqual(expectedObjects.Count, fields.Count);

            //eString and eObject refer to the same object!
            var eObject = (reader as IEnumerable).GetEnumerator();
            var eString = eObject as IEnumerator<List<string>>;

            int count = expectedObjects.Count;

            for (int i = 0; i < count; i++)
            {
                if (!eString.MoveNext())
                {
                    Assert.Fail("Ran out of lines in input file {0}", InputFile);
                }
                reader.Columns = fields[i];
                List<string> myRowStrings = eString.Current;
                List<object> myRowObjects = eObject.Current as List<object>;

                //Make sure same number of elements for current row in myRowStrings, myRowObjects, 
                //expectedObjects[i], expectedStrings[i], and fields[i]
                Assert.AreEqual(myRowStrings.Count, myRowObjects.Count);
                Assert.AreEqual(myRowObjects.Count, expectedObjects[i].Count());
                Assert.AreEqual(expectedObjects[i].Count(), expectedStrings[i].Count());
                Assert.AreEqual(expectedStrings[i].Count(), fields[i].Count());

                int fieldCount = myRowStrings.Count;
                for (int j = 0; j < fieldCount; j++)
                {
                    Assert.AreEqual(expectedObjects[i][j], myRowObjects[j]);
                    Assert.AreEqual(expectedStrings[i][j], myRowStrings[j]);
                }
            }
        }
    }
}

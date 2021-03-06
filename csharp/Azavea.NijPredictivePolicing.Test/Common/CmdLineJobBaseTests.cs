﻿/*
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
using Azavea.NijPredictivePolicing.Common;
using Azavea.NijPredictivePolicing.ACSAlchemist;
using Azavea.NijPredictivePolicing.ACSAlchemistLibrary;

namespace Azavea.NijPredictivePolicing.Test.Common
{
    [TestFixture]
    public class CmdLineJobBaseTests
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public class CommandTestObj {
            public int f { get; set; }
            public int fo { get; set; }
            public int foo { get; set; }

            public string str { get; set; }
            public double dbl { get; set; }
            public int it { get; set; }

            public string somefile { get; set; }
            public string another { get; set; }
        };



        /// <summary>
        /// Tests to make sure params with similar names don't kludge eachother
        /// </summary>
        [Test]
        public void TestParamDisambiguation()
        {
            string[] args = "-f 1 -fo 2 -foo 3".Split(' ');

            var dest = new CommandTestObj();
            CmdLineArg[] Arguments = new CmdLineArg[] {
                new CmdLineArg() { Flag = "f", DataType=typeof(int), PropertyName="f"},
                new CmdLineArg() { Flag = "fo", DataType=typeof(int), PropertyName="fo"},
                new CmdLineArg() { Flag = "foo", DataType=typeof(int), PropertyName="foo"},
            };

            CmdLineJobBase cmds = new CmdLineJobBase();
            Assert.IsTrue(cmds.Load(args, Arguments, dest), "Load failed!");

            Assert.AreEqual(1, dest.f, "failed on 'f'");
            Assert.AreEqual(2, dest.fo, "failed on 'fo'");
            Assert.AreEqual(3, dest.foo, "failed on 'foo'");
        }

        /// <summary>
        /// Tests type conversion, and different flag vs. propertyname
        /// </summary>
        [Test]
        public void TestTypeConversion()
        {
            string[] args = "-string abcdef ghijklmno pqrstuvwxyz -double 3.14159265359 -int 8".Split(' ');

            var dest = new CommandTestObj();
            CmdLineArg[] Arguments = new CmdLineArg[] {
                new CmdLineArg() { Flag = "string", DataType=typeof(string), PropertyName="str"},
                new CmdLineArg() { Flag = "double", DataType=typeof(double), PropertyName="dbl"},
                new CmdLineArg() { Flag = "int", DataType=typeof(int), PropertyName="it"},
            };

            CmdLineJobBase cmds = new CmdLineJobBase();
            Assert.IsTrue(cmds.Load(args, Arguments, dest), "Load failed!");

            Assert.AreEqual("abcdef ghijklmno pqrstuvwxyz", dest.str, "failed on string with spaces");
            Assert.AreEqual(3.14159265359, dest.dbl, "failed on double parsing");
            Assert.AreEqual(8, dest.it, "failed on integer parsing");
        }


        /// <summary>
        /// Tests filename with hypens
        /// </summary>
        [Test]
        public void TestHypensInArguments()
        {
            string[] args = "-somefile \"this-is-a-terrible-filename.txt\" -another param ".Split(' ');

            var dest = new CommandTestObj();
            CmdLineArg[] Arguments = new CmdLineArg[] {
                new CmdLineArg() { Flag = "somefile", DataType=typeof(string), PropertyName="somefile"},
                new CmdLineArg() { Flag = "another", DataType=typeof(string), PropertyName="another"}
            };

            CmdLineJobBase cmds = new CmdLineJobBase();
            Assert.IsTrue(cmds.Load(args, Arguments, dest), "Load failed!");

            Assert.AreEqual("param", dest.another, "parameter after filename with hypens was clobbered");
            Assert.AreEqual("this-is-a-terrible-filename.txt", dest.somefile, "failed on filename with hyphens");
        }


        [Test]
        public void TestQuotesInArguments()
        {
            string[] args = "-somefile \"C:\\terrible path\\with spaces\\in-it\\terrible_filename.txt\" -another param ".Split(' ');

            var dest = new CommandTestObj();
            CmdLineArg[] Arguments = new CmdLineArg[] {
                new CmdLineArg() { Flag = "somefile", DataType=typeof(string), PropertyName="somefile"},
                new CmdLineArg() { Flag = "another", DataType=typeof(string), PropertyName="another"}
            };

            CmdLineJobBase cmds = new CmdLineJobBase();
            Assert.IsTrue(cmds.Load(args, Arguments, dest), "Load failed!");

            Assert.AreEqual("param", dest.another, "parameter after filename with hypens was clobbered");
            Assert.AreEqual("C:\\terrible path\\with spaces\\in-it\\terrible_filename.txt", dest.somefile, "failed on filename with quotes");
        }

        [Test]
        public void TestInvalidArguments()
        {
            string[] args = "-realarg abcde -realagr defghi ".Split(' ');

            var dest = new CommandTestObj();
            CmdLineArg[] Arguments = new CmdLineArg[] {
                new CmdLineArg() { Flag = "realarg", DataType=typeof(string), PropertyName="somefile"}                
            };

            CmdLineJobBase cmds = new CmdLineJobBase();

            bool loadDidFail = cmds.Load(args, Arguments, dest);

            //switched this, nunit was being weird
            Assert.IsTrue(loadDidFail == false, "Load Succeeded???");  //assert should pass, load should fail
        }

        /// <summary>
        /// Ensure unicode hyphen (8211) support
        /// </summary>
        [Test]
        public void TestStandardLine()
        {
            var argsList = new string[][]{
                ("-s Wyoming -e 150 -v my-VariablesFile.txt -jobName Test-01 " + (char)8211 + "exportToShape").Split(' '),
                ("-s Wyoming -e 150 " + (char)8211 + "v my-VariablesFile.txt -jobName Test-01 -exportToShape").Split(' '),
                ((char)8211 + "s Wyoming -e 150 -v my-VariablesFile.txt -jobName Test-01 -exportToShape").Split(' ')
            };

            for (int i = 0; i < argsList.Length; i++)
            {
                var args = argsList[i];
                ImportJob job = new ImportJob();
                if (!job.Load(args))
                {
                    Assert.Fail("Couldn't parse standard line for argsList[{0}]", i);
                }

                Assert.AreEqual(AcsState.Wyoming, job.State, "State is wrong for argsList[{0}]", i);
                Assert.AreEqual("my-VariablesFile.txt", job.IncludedVariableFile, "variables file is wrong for argsList[{0}]", i);
                Assert.AreEqual("Test-01", job.JobName, "Job name is wrong for argsList[{0}]", i);
                Assert.AreEqual(true.ToString(), job.ExportToShapefile, true.ToString(), "flag param is wrong for argsList[{0}]", i);
            }
        }

        [Test]
        public void TestFullLine()
        {
            string[] args = "-s Wyoming -y 2010 -outputFolder \"c:\\sandbox\\ACSDataErmine\\\" -workingFolder \"c:\\sandbox\\ACSDataErmine\\Wo-rking\\\" -v test_vars.txt -exportToShape".Split(' ');
            ImportJob job = new ImportJob();
            if (!job.Load(args))
            {
                Assert.Fail("Couldn't parse standard line for full line");
            }


            Assert.AreEqual(AcsState.Wyoming, job.State, "State is wrong for args");
            Assert.AreEqual("test_vars.txt", job.IncludedVariableFile, "variables file is wrong for args");
            Assert.AreEqual("c:\\sandbox\\ACSDataErmine\\", job.OutputFolder, "Output folder is wrong!");
            Assert.AreEqual("c:\\sandbox\\ACSDataErmine\\Wo-rking\\", job.WorkingFolder, "Working folder is wrong!");

            Assert.AreEqual(true.ToString(), job.ExportToShapefile, true.ToString(), "flag param is wrong for args");




        }


    }
}

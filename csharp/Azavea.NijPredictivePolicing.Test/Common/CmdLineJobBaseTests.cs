using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using log4net;
using Azavea.NijPredictivePolicing.Common;

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

            Assert.AreEqual("param", dest.another, "parameter after filename with hypens was klobbered");
            Assert.AreEqual("this-is-a-terrible-filename.txt", dest.somefile, "failed on filename with hyphens");            
        }


        [Test]
        public void TestQuotesInArguments()
        {
            string[] args = "-somefile \"C:\\terrible path\\with spaces\\in it\\terrible_filename.txt\" -another param ".Split(' ');

            var dest = new CommandTestObj();
            CmdLineArg[] Arguments = new CmdLineArg[] {
                new CmdLineArg() { Flag = "somefile", DataType=typeof(string), PropertyName="somefile"},
                new CmdLineArg() { Flag = "another", DataType=typeof(string), PropertyName="another"}
            };

            CmdLineJobBase cmds = new CmdLineJobBase();
            Assert.IsTrue(cmds.Load(args, Arguments, dest), "Load failed!");

            Assert.AreEqual("param", dest.another, "parameter after filename with hypens was klobbered");
            Assert.AreEqual("C:\\terrible path\\with spaces\\in it\\terrible_filename.txt", dest.somefile, "failed on filename with quotes");
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
            Assert.IsFalse(cmds.Load(args, Arguments, dest), "Load Succeeded???");  //assert should pass, load should fail
        }

        //CmdLineJob job = new CmdLineJob();
         //   if ((args != null) && (args.Length > 0))
         //   {
         //       _log.Debug("Loading arguments...");
         //       job.Load(args, CmdLineJob.Arguments, job);

    }
}

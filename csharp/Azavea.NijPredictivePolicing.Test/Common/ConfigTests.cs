using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Azavea.NijPredictivePolicing.Common;
using log4net;
using System.IO;

namespace Azavea.NijPredictivePolicing.Test.Common
{
    [TestFixture]
    public class ConfigTests
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void CreateNewConfig()
        {
            var c = new Config();
            c.Set("foo", "bar");
            c.Set("123", 123);
            c.Set("abc", new string[] { "a", "b", "c" });

            string contents = c.SaveToMemory();
            _log.DebugFormat("Contents were: {0}", contents);
            Assert.IsNotEmpty(contents, "Unable to save contents!");

            string filename = "CreateNewConfig.foo";
            c.Save(filename);
            Assert.IsTrue(File.Exists(filename), "New Config File not found!");


            var fromFile = new Config(filename);
            Assert.AreEqual("bar", fromFile["foo"], "string values out of sync!");
            Assert.AreEqual(c["foo"], fromFile["foo"], "string values out of sync!");

            Assert.AreEqual(123, fromFile["123"], "numeric values out of sync!");
            Assert.AreEqual(c["123"], fromFile["123"], "numeric values out of sync!");

            Assert.IsTrue(AreListsEqual(new string[] { "a", "b", "c" }, fromFile.GetList("abc")), "array values out of sync!");
            Assert.IsTrue(AreListsEqual(c.GetList("abc"), fromFile.GetList("abc")), "array values out of sync!");
        }

        public static bool AreListsEqual(IList<object> left, IList<object> right)
        {
            if (left == right)
                return true;

            if ((left != null) && (right != null) && (left.Count == right.Count))
            {
                for (int i = 0; i < left.Count; i++)
                {
                    var lVal = left[i] as string;
                    var rVal = right[i] as string;

                    if (lVal != rVal)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }


    }
}

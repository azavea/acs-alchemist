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
using Azavea.NijPredictivePolicing.Common;
using log4net;
using System.IO;

namespace Azavea.NijPredictivePolicing.Test.Common
{
    [TestFixture]
    public class ConfigTests : BaseTest
    {
        [Test]
        public void CreateNewConfig()
        {
            string filename = Path.Combine(BaseDir, @"TestData\ParserTests\testconfig.config");
            var c = new Config(filename);

            Assert.AreEqual("bar", c["foo"], "string values out of sync!");
            Assert.AreEqual(c["foo"], c["foo"], "string values out of sync!");

            Assert.AreEqual(123, c["123"], "numeric values out of sync!");
            Assert.AreEqual(c["123"], c["123"], "numeric values out of sync!");

            Assert.IsTrue(AreListsEqual(new string[] { "a", "b", "c" }, c.GetList("abc")), "array values out of sync!");
            Assert.IsTrue(AreListsEqual(c.GetList("abc"), c.GetList("abc")), "array values out of sync!");
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

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
    public class UtilitiesTests
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TrimCommentsTest()
        {
            var args = new object[] {
                new string[]  { "a", "a" },
                new string[]  { "a b c #d", "a b c " },
                new string[]  { "-string abcdef ghijklmno pqrstuvwxyz #-double 3.14159265359 -int 8", "-string abcdef ghijklmno pqrstuvwxyz " },
                new string[]  { "a b c \"#d\" e", "a b c \"#d\" e" },
                new string[]  { " -jobName Test01				#use \"Test01\" as a Job Name", " -jobName Test01				" },
                new string[]  { " -outputFolder C:\\sandbox\\ACS\\	#save the data to a directory", " -outputFolder C:\\sandbox\\ACS\\	" },
            };


            foreach (string[] pair in args)
            {
                string testOutput = Utilities.TrimComments(pair[0], '#');
                Assert.AreEqual(pair[1], testOutput, "Expected output failed for TrimComments!");
            }

        }



    }
}

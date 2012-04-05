/*
  Copyright (c) 2011 Azavea, Inc.
 
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
using Azavea.NijPredictivePolicing.Test.Helpers;
using log4net;
using Azavea.NijPredictivePolicing.Common.Data;
using Azavea.NijPredictivePolicing.Common.DB;

namespace Azavea.NijPredictivePolicing.Test.Common.DB
{
    [TestFixture]
    public class TableCreationTests
    {
        private static ILog _log = null;

        [TestFixtureSetUp]
        public void Init()
        {
            _log = LogHelpers.ResetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        [Test]
        public void TestSqliteGeneration()
        {
            var columns = new List<FixedWidthField>(new FixedWidthField[] {
                new FixedWidthField("FILEID", "File Identification", 6, 1),
                new FixedWidthField("WideText", "State Postal Abbreviation", 200, 7),
                new FixedWidthField("Numeric", "State Postal Abbreviation", 200, 7, FixedWidthTypes.INT),
            });

            string sql = SqliteDataClient.GenerateTableSQLFromFields("test", columns);
            string expectedSql = @"CREATE TABLE test ( ixid INTEGER NOT NULL PRIMARY KEY, FILEID TEXT(6), WideText TEXT(200), Numeric INTEGER(200) ); ";
            _log.Debug("Test generated " + sql);

            Assert.AreEqual(expectedSql, sql);
        }


        //TODO: Test!
        //string createGeographyTable = DataClient.GenerateTableSQLFromFields("geographies", GeographyFileReader.Columns);

        //TODO: add data types to fields
        //TODO: refactor data types to be .net types...
        //TODO: refactor data translation to use getas
        //...

    }
}

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

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
using System.Data.SQLite;
using System.Data;
using NUnit.Framework;
using Azavea.NijPredictivePolicing.AcsImporterLibrary.Transfer;
using Azavea.NijPredictivePolicing.AcsImporterLibrary;
using Azavea.NijPredictivePolicing.Common.DB;
using Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats;
using Azavea.NijPredictivePolicing.Test.Helpers;
using log4net;
using System.Data.Common;
using System.IO;
using Azavea.NijPredictivePolicing.Common;

namespace Azavea.NijPredictivePolicing.Test.Common.DB
{
    [TestFixture]
    public class SpatialiteTests
    {
        private static ILog _log = null;

        [TestFixtureSetUp]
        public void Init()
        {
            _log = LogHelpers.ResetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }



        [Test]
        public void TestSpatialite()
        {
            try
            {
                if (System.IO.File.Exists("test.dat"))
                    System.IO.File.Delete("test.dat");

                SQLiteConnection conn = new SQLiteConnection("Data Source=test.dat");
                conn.Open();

                if (conn.State != System.Data.ConnectionState.Open)
                    return;

                //I've modified the ADO Sqlite wrapper to allow exceptions
                //this can be done by adding a line to the "sqlite3_open_interop" function in interop.c
                //before "return ret;" add:
                //sqlite3_enable_load_extension(*ppdb, 1);      //ENABLE EXTENSIONS for (Spatialite)

                //if the wrapper is getting in the way, you can load the extension directly using the code:
                //sqlite3_load_extension(*ppdb, "libspatialite-2.dll", 0, 0);
                //NOTE:  Spatialite needs all its DLLS (including, sqlite)...
                //libspatialite-2.dll
                //libgeos-3-0-0.dll
                //libgeos_c-1.dll
                //libiconv2.dll
                //libproj-0.dll
                //libsqlite3-1.dll


                ////load Spatialite!
                string spatialitePath = System.IO.Path.Combine(Environment.CurrentDirectory, Settings.SpatialiteDLL);
                nonQueryDB(conn, "SELECT load_extension('" + spatialitePath + "');");

                nonQueryDB(conn, "SELECT InitSpatialMetaData()");

                //-- Spatial Reference System
                nonQueryDB(conn, "INSERT INTO spatial_ref_sys (srid, auth_name, auth_srid, ref_sys_name, proj4text) VALUES(101, 'POSC', 32214, 'WGS 72 / UTM zone 14N', '+proj=utm +zone=14 +ellps=WGS72 +units=m +no_defs');");

                //EXAMPLE:
                //-- Lakes
                nonQueryDB(conn, "CREATE TABLE lakes (fid INTEGER NOT NULL PRIMARY KEY,name VARCHAR(64)); ");
                nonQueryDB(conn, "SELECT AddGeometryColumn('lakes', 'shore', 101, 'POLYGON', 2);");
                nonQueryDB(conn, "INSERT INTO lakes VALUES (101, 'Blue Lake',PolyFromText('POLYGON((52 18,66 23,73 9,48 6,52 18),(59 18,67 18,67 13,59 13,59 18))', 101));");

                //    -- Conformance Item T6
                DataTable dt = queryDB(conn, "SELECT Dimension(shore)  AS 'Conformance Item T6' FROM lakes WHERE name = 'Blue Lake'");
                if ((dt != null) && (dt.Rows.Count > 0))
                {
                    Console.WriteLine("Test Query Results: ");
                    Console.WriteLine(dt.Columns[0]);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Console.WriteLine(dt.Rows[i][0].ToString());
                    }
                }

                Console.WriteLine("Test Completed:");
                conn.Close();

                Assert.IsTrue(true, "Passed!");
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception thrown " + ex.Message);
                Console.WriteLine("Exception Caught (Bailing!): " + ex.Message + " :: " + ex.StackTrace);                
            }
        }

        public static int nonQueryDB(SQLiteConnection conn, string sql)
        {
            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            return cmd.ExecuteNonQuery();
        }

        public static DataTable queryDB(SQLiteConnection conn, string sql)
        {
            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            DataTable dt = new DataTable("results");
            SQLiteDataAdapter sda = new SQLiteDataAdapter(cmd);
            sda.Fill(dt);
            return dt;
        }

        //[Test]
        //public void TestOpenShapefile()
        //{
        //    AcsDataManager manager = new AcsDataManager(AcsState.Wyoming);
        //    string filename = manager.GetLocalShapefileName();
        //    SqliteDataClient client = new SqliteDataClient(filename);

        //    Assert.IsTrue(client.TestDatabaseConnection(), "Couldn't connect to Shapefile");
        //}

        //[Test]
        //public void TestOpenShapefile()
        //{
        //    AcsDataManager manager = new AcsDataManager(AcsState.Wyoming);
        //    string filename = manager.GetLocalShapefileName();

        //    ShapefileHelper helper = new ShapefileHelper();
        //    string tableName = "testShape";
        //    if (helper.OpenShapefile(filename, tableName))
        //    {
        //        using (DbConnection conn = helper.Client.GetConnection())
        //        {
        //            var dt = DataClient.GetMagicTable(conn, helper.Client, "select * from " + tableName);
        //            Assert.IsNotNull(dt, "couldn't get table");
        //            Assert.Greater(dt.Rows.Count, 0, "didn't have any rows!");
        //        }
        //    }
        //}


        //[Test]
        //public void WTF()
        //{
        //    if (System.IO.File.Exists("test.dat"))
        //        System.IO.File.Delete("test.dat");

        //    SQLiteConnection conn = new SQLiteConnection("Data Source=test.dat");
        //    conn.Open();

        //    string spatialitePath = System.IO.Path.Combine(Environment.CurrentDirectory, "libspatialite-2.dll");

        //    int loaded = nonQueryDB(conn, "SELECT load_extension('" + spatialitePath + "');");
        //    _log.Debug("spatialite loaded? " + loaded);

        //    nonQueryDB(conn, "SELECT InitSpatialMetaData()");

        //    string filename = @"C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\Azavea.NijPredictivePolicing.AcsImporter\ACSImporter\Working\Wyoming\shapes\bg56_d00";
        //    string sql = string.Format("CREATE VIRTUAL TABLE test_shape USING VirtualShape('{0}', CP1252, 4269);", filename);
        //    nonQueryDB(conn, sql);

        //    var cmd = conn.CreateCommand();
        //    cmd.CommandText = "select * from test_shape";
        //    var dba = new SQLiteDataAdapter(cmd);
        //    var dt = new DataTable();
        //    dba.Fill(dt);

        //    _log.Debug("here");
        //}

        //[Test]
        //public void WTFTwo()
        //{
        //    if (System.IO.File.Exists("test.dat"))
        //        System.IO.File.Delete("test.dat");

        //    var client = new SqliteDataClient("test.dat");            
        //    using (DbConnection conn = client.GetConnection())
        //    {
        //        //string spatialitePath = System.IO.Path.Combine(Environment.CurrentDirectory, "libspatialite-2.dll");
        //        //int loaded = client.GetCommand("SELECT load_extension('" + spatialitePath + "');", conn).ExecuteNonQuery();
        //        //_log.Debug("spatialite loaded? " + loaded);
        //        //client.GetCommand("SELECT InitSpatialMetaData()", conn).ExecuteNonQuery();

        //        string shapefilename = @"C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\Azavea.NijPredictivePolicing.AcsImporter\ACSImporter\Working\Wyoming\shapes\bg56_d00.shp";
        //        string filename = Path.Combine(Path.GetDirectoryName(shapefilename), Path.GetFileNameWithoutExtension(shapefilename));
        //        string sql = string.Format("CREATE VIRTUAL TABLE test_shape USING VirtualShape('{0}', CP1252, 4269);", filename);
        //        client.GetCommand(sql, conn).ExecuteNonQuery();

        //        var dt = DataClient.GetMagicTable(conn, client, "SELECT * from test_shape");


        //        _log.Debug("here");
        //    }
        //}



    }
}

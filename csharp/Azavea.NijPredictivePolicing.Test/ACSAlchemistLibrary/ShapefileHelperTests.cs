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
using System.Net;
using System.IO;
using Azavea.NijPredictivePolicing.Common;
using Azavea.NijPredictivePolicing.Test.Helpers;
using Azavea.NijPredictivePolicing.ACSAlchemistLibrary;
using System.Data;
using Azavea.NijPredictivePolicing.ACSAlchemistLibrary.Transfer;
using Azavea.NijPredictivePolicing.ACSAlchemistLibrary.FileFormats;

namespace Azavea.NijPredictivePolicing.Test.ACSAlchemistLibrary
{
    [TestFixture]
    public class ShapefileHelperTests
    {
        private static ILog _log = null;

        /// <summary>
        /// Place to dump files generated by tests
        /// </summary>
        protected const string OutputDir = "output\\";


        [TestFixtureSetUp]
        public void Init()
        {
            _log = LogHelpers.ResetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            if (!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
            }
        }

        [Ignore("These values change based on what year we've loaded, and I don't feel like fixing them.")]
        [Test]
        public void ForbiddenNames()
        {
            Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName(null), true);
            Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName(""), true);

            Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName("bg00_d00.shp"), true);
            Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName("tr99_d00"), true);
            Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName("cs55_d00.shp"), true);

            // Right now we don't have the settings to test these, and they aren't used by the app currently anyway
            //Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName("z327_d00"), true);
            //Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName("zt00_d00.shp"), true);
            //Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName("vt00_d00"), true);
            //Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName("co00_d00.shp"), true);
            //Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName("co00_d00"), true);

            Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName("co00_d0a_shp.zip"), false);
            Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName("co00_da0_shp.zip"), false);
            Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName("co00_daa_shp.zip"), false);
            Assert.AreEqual(ShapefileHelper.IsForbiddenShapefileName("co00_d00_shp.zip"), false);
        }

    }
}

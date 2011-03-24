﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using log4net;
using System.Net;
using System.IO;
using Azavea.NijPredictivePolicing.Common;
using Azavea.NijPredictivePolicing.Test.Helpers;
using Azavea.NijPredictivePolicing.AcsImporterLibrary;

namespace Azavea.NijPredictivePolicing.Test
{
    [TestFixture]
    public class FileRetrievalTests
    {
        /// <summary>
        /// Place to dump files generated by tests
        /// </summary>
        protected const string OutputDir = "output\\";

        private static ILog _log = null;

        

        [TestFixtureSetUp]
        public void Init()
        {
            _log = LogHelpers.ResetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            if (!Directory.Exists(OutputDir))
                Directory.CreateDirectory(OutputDir);
        }


        [Test]
        public void CheckAllStateFiles()
        {
            bool fail = false;
            foreach (StateList state in Enum.GetValues(typeof(StateList)))
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                        Settings.GetStateBlockGroupFileUrl(state));

                    request.Credentials = CredentialCache.DefaultCredentials;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    response.Close();
                    _log.DebugFormat("{0} still exists", States.StateToCensusName(state));
                }
                catch
                {
                    _log.DebugFormat("{0} is missing!", States.StateToCensusName(state));
                    fail = true;
                }
            }

            Assert.IsFalse(fail);
        }

        [Test]
        public void CheckDownloaderUtilities()
        {   
            string dummy;
            //Wyoming has smallest file to download
            string err = AreaDownloader.GetStateBlockGroupFile(StateList.Wyoming, out dummy);
            if (!string.IsNullOrEmpty(err))
            {
                Assert.Fail("Error: Message was: {0}", err);
            }
        }
    }
}

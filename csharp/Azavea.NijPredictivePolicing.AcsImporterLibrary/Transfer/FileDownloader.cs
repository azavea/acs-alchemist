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
using System.Net;
using System.IO;
using log4net;
using Azavea.NijPredictivePolicing.Common;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.Transfer
{
    /// <summary>
    /// This class contains the logic for caching, and downloading a url
    /// </summary>
    public static class FileDownloader
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static DateTime _lastQuery = DateTime.Now - new TimeSpan(0, 0, 0, 0, WaitTimeMs);

        /// <summary>
        /// Time to sleep between Http Requests
        /// </summary>
        public const int WaitTimeMs = 2000;

        /// <summary>
        /// Attempts to download the desiredURL, and save it to filePath.  Does not attempt to download the file
        /// unless the file creation time on the server is more recent than any existing copy, and the file hasn't been
        /// checked for over 7 days.
        /// </summary>
        /// <param name="desiredURL"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool GetFileByURL(string desiredURL, string filePath)
        {
            bool preExists = false;
            if (File.Exists(filePath))
            {
                //don't keep harassing the server if the file is less than one week old, for Pete's sake.
                TimeSpan elapsed = (DateTime.Now - File.GetCreationTime(filePath));
                preExists = true;
                if (elapsed.TotalDays < 7)
                {
                    _log.DebugFormat("File {0} is less than 7 days old, skipping", Path.GetFileName(filePath));
                    System.Threading.Thread.Sleep(100); //give the disk a chance to catch up
                    return true;
                }
            }

            int retries = 0;

            while ((!File.Exists(filePath) || preExists) && retries < 4)
            {
                try
                {

                    _lastQuery = DateTime.Now;
                    System.Threading.Thread.Sleep(250); //just a little pre-nap so we don't hammer the server

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(desiredURL);
                    request.KeepAlive = false;  //We're only doing this once
                    request.Credentials = CredentialCache.DefaultCredentials;
                    request.Timeout = Settings.TimeOutMs;
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {

                        Stream downloadStream = response.GetResponseStream();
                        long expectedLength = response.ContentLength;
                        DateTime lastModified = response.LastModified;


                        if (File.Exists(filePath))
                        {
                            string srcDate = response.LastModified.ToShortDateString();
                            string localDate = File.GetLastWriteTime(filePath).ToShortDateString();
                            if (localDate == srcDate)
                            {
                                _log.DebugFormat("File {0} already exists, and date stamps match, skipping",
                                    Path.GetFileName(filePath));

                                FileUtilities.TryChangeCreationTime(filePath, DateTime.Now);
                                return true;
                            }
                        }

                        FileStream output = new FileStream(filePath, FileMode.Create);
                        Utilities.CopyToWithProgress(downloadStream, expectedLength, output);

                        downloadStream.Close();
                        output.Close();
                        response.Close();
                        request.Abort();

                        FileUtilities.TryChangeLastWriteTime(filePath, response.LastModified);
                        _log.DebugFormat("Downloaded of {0} was successful", Path.GetFileName(filePath));

                        if (Settings.ShowFilePaths)
                        {
                            _log.InfoFormat("Downloaded File {0} saved to {1}", Path.GetFileName(filePath), filePath);
                        }
                    }

                    return true;
                }
                catch (UnauthorizedAccessException cantWriteEx)
                {
                    _log.Error("The importer couldn't save the file, please run this application as administrator, or set the output directory.");
                    _log.Fatal("The importer cannot continue.  Exiting...");
                    Environment.Exit(-1);
                }
                catch (Exception ex)
                {
                    _log.Error("Error downloading file, retrying", ex);

                    //This is to avoid the server blocking too many connection requests
                    if ((DateTime.Now - _lastQuery).Milliseconds < WaitTimeMs)
                    {
                        int nap = (int)Math.Pow(2, retries) * WaitTimeMs - (DateTime.Now - _lastQuery).Milliseconds;
                        if (nap > 0)
                        {
                            _log.DebugFormat("Sleeping for {0}ms before starting download", nap);
                            System.Threading.Thread.Sleep(nap);
                        }
                    }
                }

                retries++;
            }

            _log.FatalFormat("Could not download file at {0} to location {1} after {2} retries.  Please re-run the program to try again.  If this problem persists, you can try manually downloading the file and copying it to the above path.", desiredURL, filePath, retries);

            return false;
        }

    }
}

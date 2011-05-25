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
    public static class FileDownloader
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static DateTime _lastQuery = DateTime.Now - new TimeSpan(0, 0, 0, 0, WaitTimeMs);

        /// <summary>
        /// Time to sleep between Http Requests
        /// </summary>
        public const int WaitTimeMs = 2000;

        public static bool GetFileByURL(string desiredURL, string filePath)
        {
            if (File.Exists(filePath))
            {
                //don't keep harassing the server if the file is less than one week old, for Pete's sake.
                TimeSpan elapsed = (DateTime.Now - File.GetCreationTime(filePath));
                if (elapsed.TotalDays < 7)
                {
                    _log.DebugFormat("File {0} is less than 7 days old, skipping", Path.GetFileName(filePath));
                    File.SetCreationTime(filePath, DateTime.Now);
                    return true;
                }
            }

            int retries = 0;

            while (!File.Exists(filePath) && retries < 4)
            {
                try
                {
                    //This is to avoid the server blocking too many connection requests
                    if ((DateTime.Now - _lastQuery).Milliseconds < WaitTimeMs)
                    {
                        int nap = (int)Math.Pow(2, retries) * WaitTimeMs - (DateTime.Now - _lastQuery).Milliseconds;
                        if (nap > 0)
                        {
                            _log.DebugFormat("Sleeping for {0} ms before attempting to download next file", nap);
                            System.Threading.Thread.Sleep(nap);
                        }
                    }
                    _lastQuery = DateTime.Now;


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
                                File.SetCreationTime(filePath, DateTime.Now);
                                return true;
                            }
                        }

                        FileStream output = new FileStream(filePath, FileMode.Create);
                        Utilities.CopyToWithProgress(downloadStream, expectedLength, output);

                        downloadStream.Close();
                        output.Close();
                        response.Close();
                        request.Abort();

                        File.SetLastWriteTime(filePath, response.LastModified);
                        _log.DebugFormat("Downloaded {0} successfully", Path.GetFileName(filePath));
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    _log.Error("Error downloading file, retrying", ex);
                }

                retries++;
            }

            _log.FatalFormat("Could not download file at {0} to location {1} after {2} retries.  Please re-run the program to try again.  If this problem persists, you can try manually downloading the file and copying it to the above path.", desiredURL, filePath, retries);

            return false;
        }

    }
}

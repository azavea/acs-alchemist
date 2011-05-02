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

        private static DateTime _lastQuery = DateTime.Now;

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


            try
            {
                //This is to avoid the server blocking too many connection requests
                if ((DateTime.Now - _lastQuery).Milliseconds < WaitTimeMs)
                {
                    System.Threading.Thread.Sleep(
                        WaitTimeMs - Math.Max((DateTime.Now - _lastQuery).Milliseconds, 0));
                }
                _lastQuery = DateTime.Now;


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(desiredURL);
                request.KeepAlive = false;  //We're only doing this once
                request.Credentials = CredentialCache.DefaultCredentials;
                //TODO: Add this to config file somewhere
                request.Timeout = 10000;
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
                _log.Error("Error downloading block group file", ex);
            }
            return (File.Exists(filePath));
        }

    }
}

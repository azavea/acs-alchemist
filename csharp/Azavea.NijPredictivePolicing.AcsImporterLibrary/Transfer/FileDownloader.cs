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

        public static bool GetFileByURL(string desiredURL, string filePath)
        {
            if (File.Exists(filePath))
            {
                //don't keep harassing the server if the file is less than one week old, for Pete's sake.
                TimeSpan elapsed = (DateTime.Now - File.GetCreationTime(filePath));
                if (elapsed.TotalDays < 7)
                    return true;
            }


            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(desiredURL);

                request.KeepAlive = false;  //We're only doing this once
                request.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream downloadStream = response.GetResponseStream();
                long expectedLength = response.ContentLength;
                DateTime lastModified = response.LastModified;


                if (File.Exists(filePath))
                {
                    string srcDate = response.LastModified.ToShortDateString();
                    string localDate = File.GetLastWriteTime(filePath).ToShortDateString();
                    if (localDate == srcDate)
                    {
                        _log.Debug("Requested File already exists, and date stamps match");
                        return true;
                    }
                }


                FileStream output = new FileStream(filePath, FileMode.Create);
                Utilities.CopyToWithProgress(downloadStream, expectedLength, output);


                downloadStream.Close();
                output.Close();
                response.Close();


                File.SetLastWriteTime(filePath, response.LastModified);

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

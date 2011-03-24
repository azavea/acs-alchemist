using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Azavea.NijPredictivePolicing.Common;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary
{
    /// <summary>
    /// Provides utilities for downloading data from the ACS website
    /// </summary>
    public class AreaDownloader
    {
        /// <summary>
        /// Given a state, downloads it's block group file to the default location and returns the path to it
        /// </summary>
        /// <param name="state">The name of the state to get the file for</param>
        /// <param name="path">The path where the file was stored locally</param>
        /// <returns>null on success, error message on failure</returns>
        public static string GetStateBlockGroupFile(StateList state, out string path)
        {
            path = Path.GetFullPath(
                Path.Combine(Settings.LocalTempDirectory, Settings.GetStateBlockGroupFileName(state)));
            string result = GetStateBlockGroupFile(state, path);
            if (!string.IsNullOrEmpty(result)) path = null;
            return result;
        }

        /// <summary>
        /// Given a state and a filepath, downloads it's block group file and stores it at that filepath
        /// </summary>
        /// <param name="state">The name of the state to get the file for</param>
        /// <param name="path">Where to write the file</param>
        /// <returns>null on success, error message on failure</returns>
        public static string GetStateBlockGroupFile(StateList state, string path)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                    Settings.GetStateBlockGroupFileUrl(state));
                request.KeepAlive = false;  //We're only doing this once

                request.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream data = response.GetResponseStream();
                FileStream writeme = new FileStream(path.ToString(), FileMode.Create);

                Utilities.CopyTo(data, writeme);

                data.Close();
                writeme.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }
    }
}

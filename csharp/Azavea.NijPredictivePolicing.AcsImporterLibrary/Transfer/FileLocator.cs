using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using Azavea.NijPredictivePolicing.Common;
using log4net;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary
{
    /// <summary>
    /// Provides utilities for downloading data from the ACS website
    /// </summary>
    public class FileLocator
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string TempPath = FileUtilities.SafePathEnsure(Settings.AppTempPath, "Downloads");


        /// <summary>
        /// Given a state, returns the URL to the file containing its block group data
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string GetStateBlockGroupFileUrl(AcsState state)
        {
            return string.Concat(
                Settings.CurrentAcsAllStateTablesUrl,
                '/', GetStateBlockGroupFileName(state));
        }

        /// <summary>
        /// Given a state, returns the name to the file containing its block group data
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string GetStateBlockGroupFileName(AcsState state)
        {
            return States.StateToCensusName(state) + Settings.BlockGroupsDataTableSuffix + Settings.BlockGroupsFileTypeExtension;
        }




        ///// <summary>
        ///// Given a state, downloads it's block group file to the default location and returns the path to it
        ///// </summary>
        ///// <param name="state">The name of the state to get the file for</param>
        ///// <param name="path">The path where the file was stored locally</param>
        ///// <returns>null on success, error message on failure</returns>
        //public static string GetStateBlockGroupFile(StateList state, out string path)
        //{
        //    path = Path.GetFullPath(Path.Combine(Settings.LocalTempDirectory, Settings.GetStateBlockGroupFileName(state)));
        //    string result = GetStateBlockGroupFile(state, path);
        //    if (!string.IsNullOrEmpty(result)) path = null;
        //    return result;
        //}




        public static string GetLocalFilename(AcsState state)
        {
            string basePath = FileUtilities.PathEnsure(FileLocator.TempPath, Settings.CurrentAcsDirectory);
            return Path.Combine(basePath, state.ToString() + Settings.BlockGroupsFileTypeExtension);
        }


        /// <summary>
        /// Downloads current block group file for a given state
        /// </summary>
        /// <param name="state">desired state</param>
        /// <returns></returns>
        public static bool GetStateBlockGroupFile(AcsState state)
        {
            return GetStateBlockGroupFile(state, string.Empty);
        }


        /// <summary>
        /// Downloads current block group file for a given state
        /// </summary>
        /// <param name="state">desired state</param>
        /// <param name="path">Optional location to save the file</param>
        /// <returns></returns>
        public static bool GetStateBlockGroupFile(AcsState state, string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = GetLocalFilename(state);
                }

                //if (File.Exists(filePath))
                //{
                //    _log.DebugFormat("Requested File for State {0} already exists", state.ToString());
                //    return true;
                //}



                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                    FileLocator.GetStateBlockGroupFileUrl(state));                

                request.KeepAlive = false;  //We're only doing this once
                request.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream downloadStream = response.GetResponseStream();
                long expectedLength = response.ContentLength;


                DateTime lastModified = response.LastModified;



                if (File.Exists(filePath))
                {
                    _log.DebugFormat("Requested File for State {0} already exists", state.ToString());
                    return true;
                }



                FileStream output = new FileStream(filePath, FileMode.Create);
                Utilities.CopyToWithProgress(downloadStream, expectedLength, output);



                downloadStream.Close();
                output.Close();
                response.Close();

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error downloading block group file", ex);
            }
            return false;
        }

        public static string GetStateBlockGroupDataDir(AcsState state)
        {
            string filePath = FileLocator.GetLocalFilename(state);
            string basePath = Path.GetDirectoryName(filePath);

            //IMPORTANT!  Don't create this directory on disk, just the path string!
            return FileUtilities.PathCombine(basePath, state.ToString());
        }


        public static string GetStateBlockGroupGeographyFilename(AcsState state)
        {
            string basePath = GetStateBlockGroupDataDir(state);

            var files = Directory.GetFiles(basePath, "g*.txt");
            if ((files != null) && (files.Length > 0))
            {
                return files[0];
            }
            return string.Empty;
            //return FileUtilities.PathCombine(basePath, );
        }




        public static bool ExpandStateBlockGroupFile(AcsState state)
        {
            try
            {
                string filePath = FileLocator.GetLocalFilename(state);
                string newPath = GetStateBlockGroupDataDir(state);

                if (Directory.Exists(newPath))
                {
                    _log.Debug("State file is already expanded");
                    return true;
                }

                return FileUtilities.UnzipFileTo(newPath, filePath);


                //return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error encountered while decompressing file", ex);
            }
            return false;
        }



    }
}

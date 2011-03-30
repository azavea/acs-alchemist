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

        public static readonly string TempPath = FileUtilities.SafePathEnsure(Settings.AppTempPath);


        /// <summary>
        /// Given a state, returns the URL to the file containing its block group data
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string GetStateBlockGroupUrl(AcsState state)
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
            return States.StateToCensusName(state) + Settings.BlockGroupsDataTableSuffix + 
                Settings.BlockGroupsFileTypeExtension;
        }

        /// <summary>
        /// Gets the local path of the zip file containing all the raw blockgroup/tract data
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string GetStateBlockGroupDataFilePath(AcsState state)
        {
            string basePath = FileUtilities.PathEnsure(FileLocator.TempPath, Settings.CurrentAcsDirectory);
            return Path.Combine(basePath, state.ToString() + Settings.BlockGroupsFileTypeExtension);
        }

        /// <summary>
        /// Returns the path to the directory where the raw data files for a given should be extracted/read from
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string GetStateBlockGroupDataDir(AcsState state)
        {
            string filePath = FileLocator.GetStateBlockGroupDataFilePath(state);
            string basePath = Path.GetDirectoryName(filePath);

            //IMPORTANT!  Don't create this directory on disk, just the path string!
            return FileUtilities.PathCombine(basePath, state.ToString());
        }


        /// <summary>
        /// Gets the working directory for a given state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string GetStateWorkingDir(AcsState state)
        {
            return FileUtilities.PathEnsure(FileLocator.TempPath, "Working", state.ToString());
        }

        /// <summary>
        /// Returns the path to the local copy of the geography file for a given state.  
        /// Returns string.Empty if the file doesn't exist.
        /// </summary>
        /// <param name="dataDirectory"></param>
        /// <returns></returns>
        public static string GetStateBlockGroupGeographyFilename(string dataDirectory)
        {
            var files = Directory.GetFiles(dataDirectory, "g*.txt");
            if ((files != null) && (files.Length > 0))
            {
                return files[0];
            }
            return string.Empty;
            //return FileUtilities.PathCombine(basePath, );
        }



        public static bool ExpandZipFile(string sourceFile, string destPath)
        {
            try
            {
                if (Directory.Exists(destPath))
                {
                    var files = Directory.GetFiles(destPath);
                    if ((files != null) && (files.Length > 0))
                    {
                        _log.Debug("State file is already expanded");
                        return true;
                    }
                }

                return FileUtilities.UnzipFileTo(destPath, sourceFile);
            }
            catch (Exception ex)
            {
                _log.Error("Error encountered while decompressing file", ex);
            }
            return false;
        }



    }
}

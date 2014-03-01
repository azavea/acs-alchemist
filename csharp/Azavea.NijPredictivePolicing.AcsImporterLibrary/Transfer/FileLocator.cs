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
using System.IO;
using System.Net;
using Azavea.NijPredictivePolicing.Common;
using log4net;

namespace Azavea.NijPredictivePolicing.ACSAlchemistLibrary
{
    /// <summary>
    /// Provides utilities for downloading data from the ACS website
    /// </summary>
    public class FileLocator
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //public static readonly string TempPath = FileUtilities.SafePathEnsure(Settings.AppTempPath);


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
            //TODO:
            return States.StateToCensusName(state) + Settings.BlockGroupsDataTableSuffix 
                + Settings.DataFileTypeExtension;
        }

        /// <summary>
        /// Given a state, returns the URL to the file containing its block group data
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string GetStateAllGeometryUrl(AcsState state)
        {
            //TODO:
            return string.Concat(
                Settings.CurrentAcsAllStateTablesUrl,
                '/', GetStateAllGeometryFileName(state));
        }

        /// <summary>
        /// Given a state, returns the name to the file containing its block group data
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string GetStateAllGeometryFileName(AcsState state)
        {
            return States.StateToCensusName(state) + Settings.AllGeographiesDataTableSuffix
                + Settings.DataFileTypeExtension;
        }

        /// <summary>
        /// Gets the local path of the zip file containing all the raw blockgroup/tract data
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string GetStateBlockGroupDataFilePath(AcsState state)
        {
            string basePath = FileUtilities.PathEnsure(Settings.AppDataDirectory, Settings.CurrentAcsDirectory);
            return Path.Combine(basePath, state.ToString() + Settings.DataFileTypeExtension);
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
            return FileUtilities.PathEnsure(Settings.AppDataDirectory, "Working", state.ToString());
        }

        /// <summary>
        /// Returns the path to the local copy of the geography file for a given state.  
        /// Returns string.Empty if the file doesn't exist.
        /// </summary>
        /// <param name="dataDirectory"></param>
        /// <returns></returns>
        public static string GetAggregateDataGeographyFilename(string dataDirectory)
        {
            var files = Directory.GetFiles(dataDirectory, "g*.txt");
            if ((files != null) && (files.Length > 0))
            {
                return files[0];
            }
            return string.Empty;
            //return FileUtilities.PathCombine(basePath, );
        }


        /// <summary>
        /// wrapper function for unzipping a file to a destination
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destPath"></param>
        /// <returns></returns>
        public static bool ExpandZipFile(string sourceFile, string destPath)
        {
            try
            {
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

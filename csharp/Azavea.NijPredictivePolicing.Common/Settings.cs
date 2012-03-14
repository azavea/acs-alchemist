/*
  Copyright (c) 2011 Azavea, Inc.
 
  This file is part of _SOLUTIONNAME_.

  _SOLUTIONNAME_ is free software: you can redistribute it and/or modify
  it under the terms of the GNU Lesser General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  _SOLUTIONNAME_ is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Lesser General Public License for more details.

  You should have received a copy of the GNU Lesser General Public License
  along with _SOLUTIONNAME_.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;
using System.Reflection;

namespace Azavea.NijPredictivePolicing.Common
{
    /// <summary>
    /// Used to store global settings.  Add as necessary
    /// </summary>
    public static class Settings
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        

        /// <summary>
        /// Path to the ACS prj file for the ACS shapefiles
        /// </summary>
        public static readonly string AcsPrjFilePath; 

        static Settings()
        {
            try
            {
                //Under normal operation, we want entry assembly path
                //However, when running with NUnit, this throws an exception, whereas GetCallingAssembly 
                //works fine.  We therefore try both.

                Assembly a = Assembly.GetEntryAssembly();
                if (a == null)
                {
                    a = Assembly.GetCallingAssembly();
                }

                if (a != null)
                {
                    Settings.ApplicationPath = Path.GetDirectoryName(a.GetName().CodeBase).Replace("file:\\", "");
                }
                else
                {
                    Settings.ApplicationPath = Environment.CurrentDirectory;
                }
            }
            catch
            {
                if (string.IsNullOrEmpty(Settings.ApplicationPath))
                    throw new Exception("Could not find application path!");
            }

            //These MUST be in the constructor, because static class fields are initialized before the constructor
            //is run, which would mean ApplicationPath would be empty
            //_tempPath = FileUtilities.SafePathEnsure(Settings.ApplicationPath, "Data");


            //_tempPath = FileUtilities.SafePathEnsure(
            //    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            //    "Data");
            //_log.InfoFormat("Settings Initialized, default data path is {0}", _tempPath);


            AcsPrjFilePath = Path.Combine(Settings.ApplicationPath, "WGS84NAD83.prj"); 
        }



        public static Config ConfigFile;

        /// <summary>
        /// Prefix for Margin of Error column names
        /// </summary>
        public const string MoEPrefix = "m";

        /// <summary>
        /// Application Name -- also name of our temp folder
        /// </summary>
        public const string ApplicationName = "ACSImporter";

        /// <summary>
        /// Path to the application home folder
        /// </summary>
        public static readonly string ApplicationPath;

        /// <summary>
        /// 
        /// </summary>
        private static string _AppDataPath;


        /// <summary>
        /// Gets (and creates) the path to the Applications Temporary Folder
        /// Local path to store working files
        /// </summary>
        public static string AppDataDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_AppDataPath))
                {
                    _AppDataPath = FileUtilities.SafePathEnsure(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        Settings.ApplicationName,
                        "Data");
                    _log.InfoFormat("AppDataPath Initialized, default data path is {0}", _AppDataPath);

                    //deprecated
                    //_AppDataPath = FileUtilities.PathEnsure(Path.GetTempPath(), ApplicationName);
                }
                return _AppDataPath;
            }
            set { _AppDataPath = value; }
        }

        public static bool ShowFilePaths = true;


        private static T Get<T>(string key, T ifEmpty)
        {
            if (Settings.ConfigFile == null)
            {
                Settings.ConfigFile = new Config("defaults.config");
                Settings.RestoreDefaults();
            }
            return Settings.ConfigFile.Get<T>(key, ifEmpty);
        }


        /*
         * 
         * General Application Settings
         * 
         */

        /// <summary>
        /// URL for the US Census FTP site root
        /// </summary>
        public static string CensusFtpRoot { get { return Settings.Get("CensusFtpRoot", string.Empty); } }

        /// <summary>
        /// Directory containing the current ACS multi-year predictive data, relative to CensusFtpRoot
        /// </summary>
        public static string CurrentAcsDirectory { get { return Settings.Get("CurrentAcsDirectory", string.Empty); } }

        /// <summary>
        /// Directory containing the summary files, relative to CurrentAcsDirectory
        /// </summary>
        public static string SummaryFileDirectory { get { return Settings.Get("SummaryFileDirectory", string.Empty); } }

        /// <summary>
        /// Directory containing the zip file with files mapping column names to sequence numbers, relative to SummaryFileDirectory
        /// </summary>
        public static string ColumnMappingsFileDirectory { get { return Settings.Get("ColumnMappingsFileDirectory", string.Empty); } }

        /// <summary>
        /// The name of the zip file containing files mapping column names to sequence numbers
        /// </summary>
        public static string ColumnMappingsFileName { get { return Settings.Get("ColumnMappingsFileName", string.Empty); } }

        public static string ColumnMappingsFileExtension { get { return Settings.Get("ColumnMappingsFileExtension", ".zip"); } }

        /// <summary>
        /// Directory containing the raw data tables by state, relative to SummaryFileDirectory
        /// </summary>
        public static string CurrentAcsAllStateTablesDirectory { get { return Settings.Get("CurrentAcsAllStateTablesDirectory", string.Empty); } }


        /// <summary>
        /// Full URL of the zip file containing files mapping column names to sequence numbers
        /// </summary>
        public static string CurrentColumnMappingsFileUrl 
        {            
            get
            {
                var url = Settings.Get("CurrentColumnMappingsFileUrl", string.Empty);
                            return FillInTokenString(url);
            }
        }

        /// <summary>
        /// URL pointing to the folder containing all the ACS state tables
        /// </summary>
        public static string CurrentAcsAllStateTablesUrl
        {
            get
            {
                var url = Settings.Get("CurrentAcsAllStateTablesUrl", string.Empty);
                return FillInTokenString(url);
            }
        }

        /// <summary>
        /// Assumes a string contains one or more {configKey} tokens, and replaces them with relevant values from the config
        /// </summary>
        /// <param name="formatStr"></param>
        /// <returns></returns>
        public static string FillInTokenString(string formatStr)
        {
            var keys = Settings.ConfigFile.Keys;
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    formatStr = formatStr.Replace(string.Concat("{", key, "}"), Settings.ConfigFile[key] + string.Empty);
                }
            }
            return formatStr;
        }




        /// <summary>
        /// Currently the files in CurrentAcsAllStateTablesDirectory are named by the convention [state name] + BlockGroupsDataTableSuffix
        /// </summary>
        public static string BlockGroupsDataTableSuffix { get { return Settings.Get("BlockGroupsDataTableSuffix", string.Empty); } }

        /// <summary>
        /// Currently the files in CurrentAcsAllStateTablesDirectory are named by the convention [state name] + BlockGroupsDataTableSuffix
        /// </summary>
        public static string BlockGroupsFileTypeExtension { get { return Settings.Get("BlockGroupsFileTypeExtension", ".zip"); } }

        /// <summary>
        /// Used in the ShapeFile*Filename variables as a placeholder for the state fips code
        /// </summary>
        public static string FipsPlaceholder { get { return Settings.Get("FipsPlaceholder", "{FIPS-code}"); } }

        public static string ShapeFileBlockGroupURL { get { return Settings.Get("ShapeFileBlockGroupURL", string.Empty); } }
        public static string ShapeFileBlockGroupFilename { get { return Settings.Get("ShapeFileBlockGroupFilename", string.Empty); } }

        public static string ShapeFileTractURL { get { return Settings.Get("ShapeFileTractURL", string.Empty); } }
        public static string ShapeFileTractFilename { get { return Settings.Get("ShapeFileTractFilename", string.Empty); } }

        public static string ShapeFileCountySubdivisionsURL { get { return Settings.Get("ShapeFileCountySubdivisionsURL", string.Empty); } }
        public static string ShapeFileCountySubdivisionsFilename { get { return Settings.Get("ShapeFileCountySubdivisionsFilename", string.Empty); } }

        //3 digit zips
        public static string ShapeFileThreeDigitZipsURL { get { return Settings.Get("ShapeFileThreeDigitZipsURL", string.Empty); } }
        public static string ShapeFileThreeDigitZipsFilename { get { return Settings.Get("ShapeFileThreeDigitZipsFilename", string.Empty); } }

        //5 digit zips
        public static string ShapeFileFiveDigitZipsURL { get { return Settings.Get("ShapeFileFiveDigitZipsURL", string.Empty); } }
        public static string ShapeFileFiveDigitZipsFilename { get { return Settings.Get("ShapeFileFiveDigitZipsFilename", string.Empty); } }

        //voting
        public static string ShapeFileVotingURL { get { return Settings.Get("ShapeFileVotingURL", string.Empty); } }
        public static string ShapeFileVotingFilename { get { return Settings.Get("ShapeFileVotingFilename", string.Empty); } }

        //counties
        public static string ShapeFileCountiesURL { get { return Settings.Get("ShapeFileCountiesURL", string.Empty); } }
        public static string ShapeFileCountiesFilename { get { return Settings.Get("ShapeFileCountiesFilename", string.Empty); } }



        /// <summary>
        /// Default projection to use if AcsPrjFilePath is missing or invalid
        /// </summary>
        public static string DefaultPrj { get { return Settings.Get("DefaultPrj", string.Empty); } }

        /// <summary>
        /// Time to wait in milliseconds for a net connection request to timeout before giving up
        /// </summary>        
        public static int TimeOutMs { get { return Settings.Get("TimeOutMs", 10000); } }

        /// <summary>
        /// Prefix in front of ACS GEOIDs
        /// </summary>
        public static string GeoidPrefix { get { return Settings.Get("GeoidPrefix", "15000US"); } }

        /// <summary>
        /// set of reserved shapefile column names
        /// </summary>
        public static HashSet<string> ReservedColumnNames
        {
            get
            {
                return new HashSet<string>(Settings.ReservedColumnNamesString.ToLower().Split(','));
            }
        }

        /// <summary>
        /// list of reserved shapefile column names
        /// </summary>
        public static string ReservedColumnNamesString
        {
            get
            {
                return Settings.Get("ReservedColumnNames", "GEOID,GEOID_STRP");                
            }
        }

        


        public static void RestoreDefaults()
        {
            var c = Settings.ConfigFile;

            c.Set("AppDataDirectory", string.Empty);

            c.Set("CensusFtpRoot", "http://www2.census.gov");
            c.Set("CurrentAcsDirectory", "acs2005_2009_5yr");
            c.Set("CurrentAcsAllStateTablesDirectory", "2005-2009_ACSSF_By_State_All_Tables");
            c.Set("ColumnMappingsFileName", "2005-2009_SummaryFileXLS");

            c.Set("SummaryFileDirectory", "summaryfile");
            c.Set("ColumnMappingsFileDirectory", "UserTools");
            c.Set("ColumnMappingsFileExtension", ".zip");
            c.Set("BlockGroupsDataTableSuffix", "_Tracts_Block_Groups_Only");
            c.Set("BlockGroupsFileTypeExtension", ".zip");

            c.Set("CurrentColumnMappingsFileUrl", "{CensusFtpRoot}/{CurrentAcsDirectory}/{SummaryFileDirectory}/{ColumnMappingsFileDirectory}/{ColumnMappingsFileName}{ColumnMappingsFileExtension}");
            c.Set("CurrentAcsAllStateTablesUrl", "{CensusFtpRoot}/{CurrentAcsDirectory}/{SummaryFileDirectory}/{CurrentAcsAllStateTablesDirectory}");

            c.Set("FipsPlaceholder", "{FIPS-code}");
            c.Set("DefaultPrj", "GEOGCS[\"GCS_North_American_1983\",DATUM[\"D_North_American_1983\",SPHEROID[\"GRS_1980\",6378137,298.257222101]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]]");
            c.Set("TimeOutMs", 10000);
            c.Set("GeoidPrefix", "15000US");
            c.Set("ReservedColumnNames", "GEOID,GEOID_STRP");

            c.Set("ShapeFileBlockGroupURL", "http://www.census.gov/geo/cob/bdy/bg/bg00shp/");
            c.Set("ShapeFileBlockGroupFilename", "bg{FIPS-code}_d00_shp.zip");

            c.Set("ShapeFileTractURL", "http://www.census.gov/geo/cob/bdy/tr/tr00shp/");
            c.Set("ShapeFileTractFilename", "tr{FIPS-code}_d00_shp.zip");

            c.Set("ShapeFileCountySubdivisionsURL", "http://www.census.gov/geo/cob/bdy/cs/cs00shp/");
            c.Set("ShapeFileCountySubdivisionsFilename", "cs{FIPS-code}_d00_shp.zip");

            c.Set("ShapeFileThreeDigitZipsURL", "http://www.census.gov/geo/cob/bdy/zt/z300shp/");
            c.Set("ShapeFileThreeDigitZipsFilename", "z3{FIPS-code}_d00_shp.zip");

            c.Set("ShapeFileFiveDigitZipsURL", "http://www.census.gov/geo/cob/bdy/zt/z500shp/");
            c.Set("ShapeFileFiveDigitZipsFilename", "zt{FIPS-code}_d00_shp.zip");

            c.Set("ShapeFileVotingURL", "http://www.census.gov/geo/cob/bdy/vt/vt00shp/");
            c.Set("ShapeFileVotingFilename", "vt{FIPS-code}_d00_shp.zip");

            c.Set("ShapeFileCountiesURL", "http://www.census.gov/geo/cob/bdy/co/co00shp/");
            c.Set("ShapeFileCountiesFilename", "co{FIPS-code}_d00_shp.zip");

            c.Save();
        }

    }
}
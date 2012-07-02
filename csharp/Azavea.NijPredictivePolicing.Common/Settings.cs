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


        public static string RequestedYear = "2009";


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
            RequestedYear = "2009";
        }



        public static Config ConfigFile;

        /// <summary>
        /// Prefix for Margin of Error column names
        /// </summary>
        public const string MoEPrefix = "m";

        /// <summary>
        /// Application Name -- also name of our temp folder
        /// </summary>
        public const string ApplicationName = "ACSAlchemist";

        /// <summary>
        /// Path to the application home folder
        /// </summary>
        public static readonly string ApplicationPath;

        /// <summary>
        /// This doesn't work yet, virtualshape extension crashes / fails
        /// </summary>
        //public static string SpatialiteDLL = "libspatialite-4.dll";

        public static string SpatialiteDLL = "libspatialite-2.dll";

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
                        "Data"
                        );
                    //_log.InfoFormat("AppDataPath Initialized, default data path is {0}", _AppDataPath);

                    //deprecated
                    //_AppDataPath = FileUtilities.PathEnsure(Path.GetTempPath(), ApplicationName);
                }
                return _AppDataPath;
            }
            set { _AppDataPath = value; }
        }

        public static bool ShowFilePaths = true;

        /// <summary>
        /// Helper function that calls "Get" against the currently loaded app configuration file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="ifEmpty"></param>
        /// <returns></returns>
        private static T Get<T>(string key, T ifEmpty)
        {
            if (Settings.ConfigFile == null)
            {
                Settings.ConfigFile = new Config("defaults.config");
                Settings.RestoreDefaults();                
            }

            return Settings.ConfigFile.Get<T>(key, ifEmpty);
        }

        /// <summary>
        /// Wrapper that calls "Get" against the "Year" config specified by the "RequestedYear"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="ifEmpty"></param>
        /// <returns></returns>
        private static T GetYear<T>(string key, T ifEmpty)
        {
            return Get(Settings.RequestedYear, key, ifEmpty);
        }
        
        /// <summary>
        /// Wrapper that calls "Get" against the "Year" config specified in the params
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="year"></param>
        /// <param name="key"></param>
        /// <param name="ifEmpty"></param>
        /// <returns></returns>
        private static T Get<T>(string year, string key, T ifEmpty)
        {
            var years = LoadYearConfigs();

            if (!years.ContainsKey(year))
            {
                _log.ErrorFormat("The importer couldn't find/read the \"AcsAlchemist.json.{0}.config\" file, the importer cannot continue", year);
                _log.FatalFormat("The importer couldn't find/read the \"AcsAlchemist.json.{0}.config\" file, the importer cannot continue", year);
                Environment.Exit(-1);
            }

            return years[year].Get<T>(key, ifEmpty);
        }


        private static Dictionary<string, Config> _yearConfigs;


        /// <summary>
        /// Provides a mapping between a year or census query name like "2009" or "acs2010_1yr" etc, and a
        /// config file which describes how to talk to that service
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Config> LoadYearConfigs()
        {
            if (_yearConfigs == null)
            {
                Dictionary<string, string> foundConfigs = new Dictionary<string, string>();

                //try really hard to find these files, files found later on are assumed to be more authoritative, 
                //this is because appPath is the default, and the next two would be user overrides
                string[] paths = new string[] {
                    Settings.ApplicationPath,                    
                    Settings.AppDataDirectory,
                    Environment.CurrentDirectory
                };
                foreach (string path in paths)
                {
                    string[] files = Directory.GetFiles(path, "AcsAlchemist.json.*.config");
                    if ((files != null) && (files.Length > 0))
                    {
                        foreach (string filename in files)
                        {
                            //2009, 2010, acs2010_3yr
                            string key = Path.GetFileNameWithoutExtension(filename);

                            key = key.Replace("AcsAlchemist.json.", string.Empty);
                            if (key == "json")
                                continue;

                            foundConfigs[key] = filename;
                        }
                    }
                }

                //Settings.RestoreYear2009();
                //Settings.RestoreDefaults();

                _yearConfigs = new Dictionary<string, Config>();

                if (!foundConfigs.ContainsKey("2009"))
                {
                    Config c = new Config(Path.Combine(Settings.ApplicationPath, "AcsAlchemist.json.2009.config"));
                    Settings.RestoreYear2009(c);
                    _yearConfigs["2009"] = c;
                }

                if (!foundConfigs.ContainsKey("2010"))
                {
                    Config c = new Config(Path.Combine(Settings.ApplicationPath, "AcsAlchemist.json.2010.config"));
                    Settings.RestoreYear2010(c);
                    _yearConfigs["2010"] = c;
                }

                foreach (string key in foundConfigs.Keys)
                {
                    if (key == "json")
                        continue;

                    _yearConfigs[key] = new Config(foundConfigs[key]);
                }
            }

            return _yearConfigs;
        }



        /*
         * 
         * General Application Settings
         * 
         */

        /// <summary>
        /// URL for the US Census FTP site root
        /// </summary>
        public static string CensusFtpRoot { get { return Settings.GetYear("CensusFtpRoot", string.Empty); } }

        /// <summary>
        /// Directory containing the current ACS multi-year predictive data, relative to CensusFtpRoot
        /// </summary>
        public static string CurrentAcsDirectory { get { return Settings.GetYear("CurrentAcsDirectory", string.Empty); } }

        /// <summary>
        /// Directory containing the summary files, relative to CurrentAcsDirectory
        /// </summary>
        public static string SummaryFileDirectory { get { return Settings.GetYear("SummaryFileDirectory", string.Empty); } }

        /// <summary>
        /// Directory containing the zip file with files mapping column names to sequence numbers, relative to SummaryFileDirectory
        /// </summary>
        public static string ColumnMappingsFileDirectory { get { return Settings.GetYear("ColumnMappingsFileDirectory", string.Empty); } }

        /// <summary>
        /// The name of the zip file containing files mapping column names to sequence numbers
        /// </summary>
        public static string ColumnMappingsFileName { get { return Settings.GetYear("ColumnMappingsFileName", string.Empty); } }

        public static string ColumnMappingsFileExtension { get { return Settings.GetYear("ColumnMappingsFileExtension", ".zip"); } }

        /// <summary>
        /// Directory containing the raw data tables by state, relative to SummaryFileDirectory
        /// </summary>
        public static string CurrentAcsAllStateTablesDirectory { get { return Settings.GetYear("CurrentAcsAllStateTablesDirectory", string.Empty); } }


        /// <summary>
        /// Full URL of the zip file containing files mapping column names to sequence numbers
        /// </summary>
        public static string CurrentColumnMappingsFileUrl 
        {            
            get
            {
                var url = Settings.GetYear("CurrentColumnMappingsFileUrl", string.Empty);
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
                var url = Settings.GetYear("CurrentAcsAllStateTablesUrl", string.Empty);
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
            var years = LoadYearConfigs();
            var config = years[Settings.RequestedYear];
            var keys = config.Keys;
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    formatStr = formatStr.Replace(string.Concat("{", key, "}"), config[key] + string.Empty);
                }
            }
            return formatStr;
        }




        /// <summary>
        /// Currently the files in CurrentAcsAllStateTablesDirectory are named by the convention [state name] + BlockGroupsDataTableSuffix
        /// </summary>
        public static string BlockGroupsDataTableSuffix { get { return Settings.GetYear("BlockGroupsDataTableSuffix", string.Empty); } }

        /// <summary>
        /// non tract/block-group files in CurrentAcsAllStateTablesDirectory are named by the convention [state name] + AllGeographiesDataTableSuffix
        /// </summary>
        public static string AllGeographiesDataTableSuffix { get { return Settings.GetYear("AllGeographiesDataTableSuffix", string.Empty); } }        

        /// <summary>
        /// Currently the files in CurrentAcsAllStateTablesDirectory are named by the convention [state name] + BlockGroupsDataTableSuffix
        /// </summary>
        public static string DataFileTypeExtension { get { return Settings.GetYear("DataFileTypeExtension", ".zip"); } }

        /// <summary>
        /// Used in the ShapeFile*Filename variables as a placeholder for the state fips code
        /// </summary>
        public static string FipsPlaceholder { get { return Settings.GetYear("FipsPlaceholder", "{FIPS-code}"); } }

        public static string ShapeFileBlockGroupURL { get { return Settings.GetYear("ShapeFileBlockGroupURL", string.Empty); } }
        public static string ShapeFileBlockGroupFilename { get { return Settings.GetYear("ShapeFileBlockGroupFilename", string.Empty); } }

        public static string ShapeFileTractURL { get { return Settings.GetYear("ShapeFileTractURL", string.Empty); } }
        public static string ShapeFileTractFilename { get { return Settings.GetYear("ShapeFileTractFilename", string.Empty); } }

        public static string ShapeFileCountySubdivisionsURL { get { return Settings.GetYear("ShapeFileCountySubdivisionsURL", string.Empty); } }
        public static string ShapeFileCountySubdivisionsFilename { get { return Settings.GetYear("ShapeFileCountySubdivisionsFilename", string.Empty); } }

        //3 digit zips
        public static string ShapeFileThreeDigitZipsURL { get { return Settings.GetYear("ShapeFileThreeDigitZipsURL", string.Empty); } }
        public static string ShapeFileThreeDigitZipsFilename { get { return Settings.GetYear("ShapeFileThreeDigitZipsFilename", string.Empty); } }

        //5 digit zips
        public static string ShapeFileFiveDigitZipsURL { get { return Settings.GetYear("ShapeFileFiveDigitZipsURL", string.Empty); } }
        public static string ShapeFileFiveDigitZipsFilename { get { return Settings.GetYear("ShapeFileFiveDigitZipsFilename", string.Empty); } }

        //voting
        public static string ShapeFileVotingURL { get { return Settings.GetYear("ShapeFileVotingURL", string.Empty); } }
        public static string ShapeFileVotingFilename { get { return Settings.GetYear("ShapeFileVotingFilename", string.Empty); } }

        //counties
        public static string ShapeFileCountiesURL { get { return Settings.GetYear("ShapeFileCountiesURL", string.Empty); } }
        public static string ShapeFileCountiesFilename { get { return Settings.GetYear("ShapeFileCountiesFilename", string.Empty); } }



        /// <summary>
        /// Default projection to use if AcsPrjFilePath is missing or invalid
        /// </summary>
        public static string DefaultPrj { get { return Settings.GetYear("DefaultPrj", string.Empty); } }

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
                return Settings.GetYear("ReservedColumnNames", "GEOID,GEOID_STRP");                
            }
        }

        public static void RestoreDefaults()
        {
            var c = Settings.ConfigFile;
            c.Set("AppDataDirectory", string.Empty);
            c.Set("TimeOutMs", 10000);
            c.Set("GeoidPrefix", "15000US");
            c.Save();
        }


        public static void RestoreYear2009(Config c)
        {
            RestoreCensusDefaults(c);
            c.Set("CensusFtpRoot", "http://www2.census.gov");
            c.Set("CurrentAcsDirectory", "acs2005_2009_5yr");
            c.Set("CurrentAcsAllStateTablesDirectory", "2005-2009_ACSSF_By_State_All_Tables");
            c.Set("ColumnMappingsFileName", "2005-2009_SummaryFileXLS");
            c.Save();
        }

        public static void RestoreYear2010(Config c)
        {
            //SEE: http://www2.census.gov/acs2010_5yr/summaryfile/UserTools/ACS_2006-2010_SF_Tech_Doc.pdf
            RestoreCensusDefaults(c);
            c.Set("CensusFtpRoot", "http://www2.census.gov");
            c.Set("CurrentAcsDirectory", "acs2010_5yr");
            c.Set("CurrentAcsAllStateTablesDirectory", "2006-2010_ACSSF_By_State_All_Tables");
            c.Set("ColumnMappingsFileName", "2010_SummaryFileTemplates");


            c.Set("ShapeFileBlockGroupURL", "http://www2.census.gov/geo/tiger/GENZ2010/");
            c.Set("ShapeFileBlockGroupFilename", "gz_2010_{FIPS-code}_150_00_500k.zip");

            c.Set("ShapeFileTractURL", "http://www2.census.gov/geo/tiger/GENZ2010/");
            c.Set("ShapeFileTractFilename", "gz_2010_{FIPS-code}_140_00_500k.zip");

            c.Set("ShapeFileCountySubdivisionsURL", "http://www2.census.gov/geo/tiger/GENZ2010/");
            c.Set("ShapeFileCountySubdivisionsFilename", "gz_2010_{FIPS-code}_060_00_500k.zip");


            //upper / lower voting districts
            //http://www.census.gov/geo/www/cob/cbf_states.html


            //did not find here: http://www.census.gov/geo/www/cob/index.html
            //c.Set("ShapeFileThreeDigitZipsURL", string.Empty);
            //c.Set("ShapeFileThreeDigitZipsFilename", string.Empty);

            //did not find here: http://www.census.gov/geo/www/cob/index.html
            //c.Set("ShapeFileFiveDigitZipsURL", "http://www.census.gov/geo/cob/bdy/zt/z500shp/");
            //c.Set("ShapeFileFiveDigitZipsFilename", "zt{FIPS-code}_d00_shp.zip");


            // COUNTY SHAPES NOT UPDATED PER: http://www.census.gov/geo/www/cob/cbf_counties.html
            c.Set("ShapeFileCountiesURL", "http://www.census.gov/geo/cob/bdy/co/co00shp/");
            c.Set("ShapeFileCountiesFilename", "co{FIPS-code}_d00_shp.zip");

            //c.Set("ShapeFileVotingURL", "http://www.census.gov/geo/cob/bdy/vt/vt00shp/");
            //c.Set("ShapeFileVotingFilename", "vt{FIPS-code}_d00_shp.zip");

            
            c.Save();
        }

        public static void RestoreCensusDefaults(Config c)
        {
            c.Set("SummaryFileDirectory", "summaryfile");
            c.Set("ColumnMappingsFileDirectory", "UserTools");
            c.Set("ColumnMappingsFileExtension", ".zip");

            c.Set("BlockGroupsDataTableSuffix", "_Tracts_Block_Groups_Only");
            c.Set("AllGeographiesDataTableSuffix", "_All_Geographies_Not_Tracts_Block_Groups");
            c.Set("DataFileTypeExtension", ".zip");


            c.Set("CurrentColumnMappingsFileUrl", "{CensusFtpRoot}/{CurrentAcsDirectory}/{SummaryFileDirectory}/{ColumnMappingsFileDirectory}/{ColumnMappingsFileName}{ColumnMappingsFileExtension}");
            c.Set("CurrentAcsAllStateTablesUrl", "{CensusFtpRoot}/{CurrentAcsDirectory}/{SummaryFileDirectory}/{CurrentAcsAllStateTablesDirectory}");

            c.Set("FipsPlaceholder", "{FIPS-code}");
            c.Set("DefaultPrj", "GEOGCS[\"GCS_North_American_1983\",DATUM[\"D_North_American_1983\",SPHEROID[\"GRS_1980\",6378137,298.257222101]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]]");

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
        }

    }
}
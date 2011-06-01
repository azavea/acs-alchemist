using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Azavea.NijPredictivePolicing.Common
{
    /// <summary>
    /// Used to store global settings.  Add as necessary
    /// </summary>
    public static class Settings
    {
        public static Config ConfigFile;

        /// <summary>
        /// Application Name -- also name of our temp folder
        /// </summary>
        public const string ApplicationName = "ACSImporter";

        /// <summary>
        /// Path to the application home folder
        /// </summary>
        public static readonly string ApplicationPath = Path.GetDirectoryName(
            System.Reflection.Assembly.GetEntryAssembly().GetName().CodeBase);


//#if DEBUG
//        private static string _tempPath = @"C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\Azavea.NijPredictivePolicing.AcsImporter\ACSImporter";
//#else
        private static string _tempPath = FileUtilities.SafePathEnsure(Environment.CurrentDirectory, "Data");
//#endif

        /// <summary>
        /// Gets (and creates) the path to the Applications Temporary Folder
        /// </summary>
        public static string AppTempPath
        {
            get
            {
                if (string.IsNullOrEmpty(_tempPath))
                {
                    _tempPath = FileUtilities.PathEnsure(Path.GetTempPath(), ApplicationName);
                }
                return _tempPath;
            }
        }


        /*
         * 
         * General Application Settings
         * 
         */

        /// <summary>
        /// URL for the US Census FTP site root
        /// </summary>
        public const string CensusFtpRoot = "http://www2.census.gov";

        /// <summary>
        /// Directory containing the current ACS multi-year predictive data, relative to CensusFtpRoot
        /// </summary>
        public const string CurrentAcsDirectory = "acs2005_2009_5yr";

        /// <summary>
        /// Directory containing the summary files, relative to CurrentAcsDirectory
        /// </summary>
        public const string SummaryFileDirectory = "summaryfile";

        /// <summary>
        /// Directory containing the zip file with files mapping column names to sequence numbers, relative to SummaryFileDirectory
        /// </summary>
        public const string ColumnMappingsFileDirectory = "UserTools";

        /// <summary>
        /// The name of the zip file containing files mapping column names to sequence numbers
        /// </summary>
        public const string ColumnMappingsFileName = "2005-2009_SummaryFileXLS";

        public const string ColumnMappingsFileExtension = ".zip";

        /// <summary>
        /// Full URL of the zip file containing files mapping column names to sequence numbers
        /// </summary>
        public const string CurrentColumnMappingsFileUrl =
            CensusFtpRoot + "/" +
            CurrentAcsDirectory + "/" +
            SummaryFileDirectory + "/" +
            ColumnMappingsFileDirectory + "/" +
            ColumnMappingsFileName + ColumnMappingsFileExtension;

        /// <summary>
        /// Directory containing the raw data tables by state, relative to SummaryFileDirectory
        /// </summary>
        public const string CurrentAcsAllStateTablesDirectory = "2005-2009_ACSSF_By_State_All_Tables";


        /// <summary>
        /// URL pointing to the folder containing all the ACS state tables
        /// </summary>
        public static string CurrentAcsAllStateTablesUrl
        {
            get
            {
                return string.Concat(CensusFtpRoot,
                '/', CurrentAcsDirectory,
                '/', SummaryFileDirectory,
                '/', CurrentAcsAllStateTablesDirectory
                    );
            }
        }

        /// <summary>
        /// Currently the files in CurrentAcsAllStateTablesDirectory are named by the convention [state name] + BlockGroupsDataTableSuffix
        /// </summary>
        public const string BlockGroupsDataTableSuffix = "_Tracts_Block_Groups_Only";

        /// <summary>
        /// Currently the files in CurrentAcsAllStateTablesDirectory are named by the convention [state name] + BlockGroupsDataTableSuffix
        /// </summary>
        public const string BlockGroupsFileTypeExtension = ".zip";

        /// <summary>
        /// Used in the ShapeFile*Filename variables as a placeholder for the state fips code
        /// </summary>
        public const string FipsPlaceholder = "{FIPS-code}";

        public const string ShapeFileBlockGroupURL = "http://www.census.gov/geo/cob/bdy/bg/bg00shp/";
        public const string ShapeFileBlockGroupFilename = "bg{FIPS-code}_d00_shp.zip";

        public const string ShapeFileTractURL = "http://www.census.gov/geo/cob/bdy/tr/tr00shp/";
        public const string ShapeFileTractFilename = "tr{FIPS-code}_d00_shp.zip";

        public const string ShapeFileCountySubdivisionsURL = "http://www.census.gov/geo/cob/bdy/cs/cs00shp/";
        public const string ShapeFileCountySubdivisionsFilename = "cs{FIPS-code}_d00_shp.zip";

        //3 digit zips
        public const string ShapeFileThreeDigitZipsURL = "http://www.census.gov/geo/cob/bdy/zt/z300shp/";
        public const string ShapeFileThreeDigitZipsFilename = "z3{FIPS-code}_d00_shp.zip";

        //5 digit zips
        public const string ShapeFileFiveDigitZipsURL = "http://www.census.gov/geo/cob/bdy/zt/z500shp/";
        public const string ShapeFileFiveDigitZipsFilename = "zt{FIPS-code}_d00_shp.zip";

        //voting
        public const string ShapeFileVotingURL = "http://www.census.gov/geo/cob/bdy/vt/vt00shp/";
        public const string ShapeFileVotingFilename = "vt{FIPS-code}_d00_shp.zip";

        //counties
        public const string ShapeFileCountiesURL = "http://www.census.gov/geo/cob/bdy/co/co00shp/";
        public const string ShapeFileCountiesFilename = "co{FIPS-code}_d00_shp.zip";


        /// <summary>
        /// Path to the ACS prj file for the ACS shapefiles
        /// </summary>
        public static readonly string AcsPrjFilePath = Path.Combine(Settings.ApplicationPath, "WGS84NAD83.prj"); 
        
        /// <summary>
        /// Default projection to use if AcsPrjFilePath is missing or invalid
        /// </summary>
        public const string DefaultPrj = "GEOGCS[\"GCS_North_American_1983\",DATUM[\"D_North_American_1983\",SPHEROID[\"GRS_1980\",6378137,298.257222101]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]]";

        /// <summary>
        /// Time to wait in milliseconds for a net connection request to timeout before giving up
        /// </summary>
        public const int TimeOutMs = 10000;
    }
}

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
    public class Settings
    {
        public static Config ConfigFile;

        /// <summary>
        /// Application Name -- also name of our temp folder
        /// </summary>
        public const string ApplicationName = "ACSImporter";


#if DEBUG
        private static string _tempPath = @"C:\projects\Temple_Univ_NIJ_Predictive_Policing\csharp\Azavea.NijPredictivePolicing.AcsImporter\ACSImporter";
#else
        private static string _tempPath;
#endif

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


        public const string StateBlockGroupShapefileRootURL = "http://www.census.gov/geo/cob/bdy/bg/bg00shp/";
        public const string StateBlockGroupShapefileFormatURL = "bg{FIPS-code}_d00_shp.zip";



    }
}

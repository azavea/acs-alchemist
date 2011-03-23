using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azavea.NijPredictivePolicing.Common
{
    /// <summary>
    /// Used to store global settings.  Add as necessary
    /// </summary>
    public class Settings
    {
        #region Properties

        /*
         * 
         * General Application Settings
         * 
         */

        /// <summary>
        /// URL for the US Census FTP site root
        /// </summary>
        public const string CensusFtpRoot = "http://www2.census.gov/";

        /// <summary>
        /// Directory containing the current ACS multi-year predictive data, relative to CensusFtpRoot
        /// </summary>
        public const string CurrentAcsDirectory = "acs2005_2009_5yr/";

        /// <summary>
        /// Directory containing the summary files, relative to CurrentAcsDirectory
        /// </summary>
        public const string SummaryFileDirectory = "summaryfile/";

        /// <summary>
        /// Directory containing the raw data tables by state, relative to SummaryFileDirectory
        /// </summary>
        public const string CurrentAcsAllStateTablesDirectory = "2005-2009_ACSSF_By_State_All_Tables/";

        /// <summary>
        /// URL pointing to the folder containing all the ACS state tables
        /// </summary>
        public const string CurrentAcsAllStateTablesUrl = CensusFtpRoot + CurrentAcsDirectory + 
            SummaryFileDirectory + CurrentAcsAllStateTablesDirectory;

        /// <summary>
        /// Currently the files in CurrentAcsAllStateTablesDirectory are named by the convention [state name] + BlockGroupsDataTableSuffix
        /// </summary>
        public const string BlockGroupsDataTableSuffix = "_Tracts_Block_Groups_Only.zip";

        //Might want to move this out of Settings at a later date, but for now not sure where else to put it
        /// <summary>
        /// Given a state, returns the URL to the file containing its block group data
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string GetStateBlockGroupFileUrl(StateList state)
        {
            return CurrentAcsAllStateTablesUrl + States.StateToCensusName(state);
        }


        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azavea.NijPredictivePolicing.Common;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary
{
    public enum AcsState
    {
        Alabama = 0,
        Alaska,
        Arizona,
        Arkansas,
        California,
        Colorado,
        Connecticut,
        Delaware,
        DistrictofColumbia,
        Florida,
        Georgia,
        Hawaii,
        Idaho,
        Illinois,
        Indiana,
        Iowa,
        Kansas,
        Kentucky,
        Louisiana,
        Maine,
        Maryland,
        Massachusetts,
        Michigan,
        Minnesota,
        Mississippi,
        Missouri,
        Montana,
        Nebraska,
        Nevada,
        NewHampshire,
        NewJersey,
        NewMexico,
        NewYork,
        NorthCarolina,
        NorthDakota,
        Ohio,
        Oklahoma,
        Oregon,
        Pennsylvania,
        PuertoRico,
        RhodeIsland,
        SouthCarolina,
        SouthDakota,
        Tennessee,
        Texas,
        Utah,
        Vermont,
        Virginia,
        Washington,
        WestVirginia,
        Wisconsin,
        Wyoming,
        UnitedStates,        //Not really a state, but ACS includes it as an option
        None
    }

    public class States
    {
        /// <summary>
        /// The Census uses it's own naming convention for states (currently PascalCase without spaces)
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string StateToCensusName(AcsState state)
        {
            return state.ToString();
        }

        ///// <summary>
        ///// Shows how to use the settings
        ///// </summary>
        ///// <param name="state"></param>
        ///// <returns></returns>
        //public static string StateToCurrentBlockGroupFilename(AcsState state)
        //{
        //    return string.Concat(state.ToString(), Settings.BlockGroupsDataTableSuffix);
        //}


        /// <summary>
        /// Get's a pretty name for a state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string StateToRealName(AcsState state)
        {
            throw new NotImplementedException("This function will be implemented at a later date");
        }
    }
}

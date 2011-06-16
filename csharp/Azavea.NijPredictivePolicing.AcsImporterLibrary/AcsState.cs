using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azavea.NijPredictivePolicing.Common;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary
{
    /// <summary>
    /// Contains an enumeration of the State fips codes,
    /// as defined here: http://www.itl.nist.gov/fipspubs/fip5-2.htm
    /// </summary>
    public enum AcsState
    {
        Alabama = 1,
        Alaska = 2,
        Arizona = 4,
        Arkansas = 5,
        California = 6,
        Colorado = 08,
        Connecticut = 09,
        Delaware = 10,
        DistrictofColumbia = 11,
        Florida = 12,
        Georgia = 13,
        Hawaii = 15,
        Idaho = 16,
        Illinois = 17,
        Indiana = 18,
        Iowa = 19,
        Kansas = 20,
        Kentucky = 21,
        Louisiana = 22,
        Maine = 23,
        Maryland = 24,
        Massachusetts = 25,
        Michigan = 26,
        Minnesota = 27,
        Mississippi = 28,
        Missouri = 29,
        Montana = 30,
        Nebraska = 31,
        Nevada = 32,
        NewHampshire = 33,
        NewJersey = 34,
        NewMexico = 35,
        NewYork = 36,
        NorthCarolina = 37,
        NorthDakota = 38,
        Ohio = 39,
        Oklahoma = 40,
        Oregon = 41,
        Pennsylvania = 42,       
        RhodeIsland = 44,
        SouthCarolina = 45,
        SouthDakota = 46,
        Tennessee = 47,
        Texas = 48,
        Utah = 49,
        Vermont = 50,
        Virginia = 51,
        Washington = 53,
        WestVirginia = 54,
        Wisconsin = 55,
        Wyoming = 56,
        UnitedStates = 00,        //Not really a state, but ACS includes it as an option

        PuertoRico = 72,
        None = -1
    }

    /// <summary>
    /// Encapsulates common operations with the States enum
    /// </summary>
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azavea.NijPredictivePolicing.Common
{
    public enum StateList
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
        UnitedStates        //Not really a state, but ACS includes it as an option
    }

    public class States
    {
        /// <summary>
        /// The Census uses it's own naming convention for states (currently PascalCase without spaces)
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string StateToCensusName(StateList state)
        {
            return state.ToString();
        }

        /// <summary>
        /// Get's a pretty name for a state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static string StateToRealName(StateList state)
        {
            throw new NotImplementedException("This function will be implemented at a later date");
        }
    }
}

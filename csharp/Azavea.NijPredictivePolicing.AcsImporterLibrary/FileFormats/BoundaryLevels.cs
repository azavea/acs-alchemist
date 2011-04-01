using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats
{
    public enum BoundaryLevels
    {
        [DescriptionAttribute("Census Block Groups by State 2000 ")]
        census_blockgroups,

        [DescriptionAttribute("Census Tracts by State 2000 ")]
        census_tracts,

        [DescriptionAttribute("County Subdivisions by State 2000 ")]
        county_subdivisions,

        [DescriptionAttribute("Voting Districts by State 2000 ")]
        voting,

        [DescriptionAttribute("3-Digit ZIP Code Tabulation Areas (ZCTAs) 2000 by State")]
        zipthree,

        [DescriptionAttribute("5-Digit ZIP Code Tabulation Areas (ZCTAs) 2000 by State")]
        zipfive,

        [DescriptionAttribute("County and County Equivalent Areas by State")]
        counties,

        [DescriptionAttribute("State and State Equivalent Areas")]
        states,

        [DescriptionAttribute("Census Regions")]
        census_regions,

        [DescriptionAttribute("Census Divisions")]
        census_divisions,
    }
}

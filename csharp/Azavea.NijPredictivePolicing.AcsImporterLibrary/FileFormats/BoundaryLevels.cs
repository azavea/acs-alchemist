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
using System.ComponentModel;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats
{
    /// <summary>
    /// TODO: Unify these into classes?
    /// </summary>
    public enum BoundaryLevels
    {
        [DescriptionAttribute("Census Regions")]
        census_regions = 20,

        [DescriptionAttribute("Census Divisions")]
        census_divisions = 30,

        [DescriptionAttribute("State and State Equivalent Areas")]
        states = 40,

        [DescriptionAttribute("County and County Equivalent Areas by State")]
        counties = 50,

        [DescriptionAttribute("County Subdivisions by State 2000 ")]
        county_subdivisions = 60,

        [DescriptionAttribute("Census Tracts by State 2000 ")]
        census_tracts = 140,

        [DescriptionAttribute("Census Block Groups by State 2000 ")]
        census_blockgroups = 150,

        [DescriptionAttribute("Voting Districts by State 2000 ")]
        voting = 700,

        [DescriptionAttribute("3-Digit ZIP Code Tabulation Areas (ZCTAs) 2000 by State")]
        zipthree = -1,

        [DescriptionAttribute("5-Digit ZIP Code Tabulation Areas (ZCTAs) 2000 by State")]
        zipfive = -2
    }


    /// <summary>
    /// http://www.cubitplanning.com/blog/2011/03/census-summary-level-sumlev/
    /// http://www.census.gov/prod/cen2000/doc/sf1.pdf
    /// http://www.census.gov/prod/cen2010/doc/pl94-171.pdf
    /// ftp://ftp2.census.gov/acs2009_5yr/prod/Geography_Summary_Levels_and_Components.pdf
    /// </summary>
    public class CensusSummaryLevels
    {
        public const string census_regions = "020";
        public const string census_divisions = "030";
        public const string states = "040";
        public const string counties = "050";
        public const string county_subdivisions = "060";        
        public const string census_tracts = "140";
        public const string census_blockgroups = "150";

        public const string voting = "700";//not 100% sure about this one

        /// <summary>
        /// ftp://ftp2.census.gov/acs2009_5yr/prod/Geography_Summary_Levels_and_Components.pdf
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static string GetSummaryLevelFor(BoundaryLevels level)
        {
            switch (level)
            {
                case BoundaryLevels.states: return states;
                case BoundaryLevels.counties: return counties;
                case BoundaryLevels.census_tracts: return census_tracts;
                case BoundaryLevels.census_blockgroups: return census_blockgroups;
                case BoundaryLevels.census_regions: return census_regions;
                case BoundaryLevels.county_subdivisions: return county_subdivisions;
                case BoundaryLevels.census_divisions: return census_divisions;
                case BoundaryLevels.voting: return voting;
                //case BoundaryLevels.zipthree: return zipthree;
                //case BoundaryLevels.zipfive: return zipfive;

                default:
                    break;
            }
            return string.Empty;
        }

    }
}

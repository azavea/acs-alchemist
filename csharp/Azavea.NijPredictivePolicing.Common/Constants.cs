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

namespace Azavea.NijPredictivePolicing.Common
{
    /// <summary>
    /// Central spot for general string constants, hopefully it'll make localization a little easier
    /// </summary>
    public static class Constants
    {
        public enum ExitCodes
        {
            BAD_ARGUMENTS = -1,
            OK = 0
        }

        public const string Warning_MissingProjection = @"
*********************
IMPORTANT!:  
  You have not specified an output projection!
This means any exported shapefile will be in unprojected WGS84.
Your filtering geometries, envelope, grid cell sizes, 
and all other parameters must match that projection.
*********************";
        
    }

    public static class DbConstants
    {
        //public const string TABLE_Geographies = "geographies";
        public const string TABLE_ColumnMappings = "columnMappings";
        public const string TABLE_DesiredColumns = "desiredColumns";

    }
}

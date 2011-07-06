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
  You have not specified an output projection, meaning the resulting shapefile will
be in unprojected WGS84.  Your filtering geometries, envelope, grid cell sizes, 
and all other parameters must match that projection.
*********************";
        
    }

    public static class DbConstants
    {
        public const string TABLE_Geographies = "geographies";
        public const string TABLE_ColumnMappings = "columnMappings";
        public const string TABLE_DesiredColumns = "desiredColumns";

    }
}

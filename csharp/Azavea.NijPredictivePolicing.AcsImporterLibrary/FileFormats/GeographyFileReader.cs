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
using Azavea.NijPredictivePolicing.Common.Data;
using log4net;
using System.IO;

namespace Azavea.NijPredictivePolicing.ACSAlchemistLibrary.FileFormats
{
    /// <summary>
    /// This class contains fixed width column definitons for reading the ACS geography file
    /// </summary>
    public class GeographyFileReader : IDisposable
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected FixedWidthColumnReader _reader;
        public bool HasFile = false;

        /// <summary>
        /// Opens a geography file
        /// </summary>
        /// <param name="filename"></param>
        public GeographyFileReader(string filename)
        {
            this.HasFile = File.Exists(filename);
            _reader = new FixedWidthColumnReader(filename, GeographyFileReader.Columns);
        }

        public FixedWidthColumnReader GetReader()
        {
            return _reader;
        }


        #region Columns

        private static List<FixedWidthField> _columns;
        public static List<FixedWidthField> Columns
        {
            get
            {
                if (GeographyFileReader._columns == null)
                {
                    GeographyFileReader._columns = new List<FixedWidthField>(new FixedWidthField[] {
                    new FixedWidthField("FILEID", "File Identification", 6, 0),
                    new FixedWidthField("STUSAB", "State Postal Abbreviation", 2, 6),
                    new FixedWidthField("SUMLEVEL", "Summary Level", 3, 8),
                    new FixedWidthField("COMPONENT", "Geographic Component", 2, 11),
                    new FixedWidthField("LOGRECNO", "Logical Record Number", 7, 13),
                    new FixedWidthField("US", "US", 1, 20),
                    new FixedWidthField("REGION", "Census Region", 1, 21),
                    new FixedWidthField("DIVISION", "Census Division", 1, 22),
                    new FixedWidthField("STATECE", "State (Census Code)", 2, 23),
                    new FixedWidthField("STATE", "State (FIPS Code)", 2, 25),
                    new FixedWidthField("COUNTY", "County of current residence", 3, 27),
                    new FixedWidthField("COUSUB", "County Subdivision (FIPS)", 5, 30),
                    new FixedWidthField("PLACE", "Place (FIPS Code)", 5, 35),
                    new FixedWidthField("TRACT", "Census Tract", 6, 40),
                    new FixedWidthField("BLKGRP", "Block Group", 1, 46),
                    new FixedWidthField("CONCIT", "Consolidated City", 5, 47),
                    new FixedWidthField("AIANHH", "American Indian Area", 4, 52),
                    new FixedWidthField("AIANHHFP", "_____", 5, 56),
                    new FixedWidthField("AIHHTLI", "_____", 1, 61),
                    new FixedWidthField("AITSCE", "_____", 3, 62),
                    new FixedWidthField("AITS", "_____", 5, 65),
                    new FixedWidthField("ANRC", "_____", 5, 70),
                    new FixedWidthField("CBSA", "Metropolitan and Micropolitan Statistical Area", 5, 75),
                    new FixedWidthField("CSA", "Combined Statistical Area", 3, 80),
                    new FixedWidthField("METDIV", "Metropolitan Statistical Area-Metropolitan Division", 5, 83),
                    new FixedWidthField("MACC", "Metropolitan Area Central City", 1, 88),
                    new FixedWidthField("MEMI", "Metropolitan/Micropolitan Indicator Flag", 1, 89),
                    new FixedWidthField("NECTA", "New England City and Town Area", 5, 90),
                    new FixedWidthField("CNECTA", "New England City and Town Combined Statistical Area", 3, 95),
                    new FixedWidthField("NECTADIV", "New England City and Town Area Division", 5, 98),
                    new FixedWidthField("UA", "Urban Area", 5, 103),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    new FixedWidthField("CDCURR", "Current Congressional District ***", 2, 113),
                    new FixedWidthField("SLDU", "State Legislative District Upper", 3, 115),
                    new FixedWidthField("SLDL", "State Legislative District Lower", 3, 118),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    new FixedWidthField("SUBMCD", "Subminor Civil Division (FIPS)", 5, 135),
                    new FixedWidthField("SDELM", "State-School District (Elementary)", 5, 140),
                    new FixedWidthField("SDSEC", "State-School District (Secondary)", 5, 145),
                    new FixedWidthField("SDUNI", "State-School District (Unified)", 5, 150),
                    new FixedWidthField("UR", "Urban/Rural", 1, 155),
                    new FixedWidthField("PCI", "Principal City Indicator", 1, 156),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    new FixedWidthField("PUMA5", "Public Use Microdata Area – 5% File", 5, 168),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    new FixedWidthField("GEOID", "Geographic Identifier", 40, 178),
                    new FixedWidthField("NAME", "Area Name", 200, 218, FixedWidthTypes.STRING, FixedWidthTerminators.NEWLINE)
                    //new FixedWidthField("BLANK", "_____", _, _),                     
                    });
                }
                return GeographyFileReader._columns;
            }
        }

        #endregion Columns


        public void Dispose()
        {
            _reader.Close();
            _columns = null;
        }



    }
}

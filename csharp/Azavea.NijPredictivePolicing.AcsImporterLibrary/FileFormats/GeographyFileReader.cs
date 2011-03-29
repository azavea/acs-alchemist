using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azavea.NijPredictivePolicing.Common.Data;
using log4net;
using System.IO;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats
{
    public class GeographyFileReader
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected FixedWidthColumnReader _reader;
        public bool HasFile = false;

        public GeographyFileReader(string filename)
        {
            this.HasFile = File.Exists(filename);
            _reader = new FixedWidthColumnReader(filename, GeographyFileReader.Columns);
        }

        public GeographyFileReader(AcsState aState)
        {
            string filename = FileLocator.GetStateBlockGroupGeographyFilename(aState);
            if (!string.IsNullOrEmpty(filename))
            {
                _reader = new FixedWidthColumnReader(filename, GeographyFileReader.Columns);
                this.HasFile = File.Exists(filename);
            }
            else
            {
                _log.ErrorFormat("Couldn't locate geography file for state {0}", aState);
            }            
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
                    new FixedWidthField("FILEID", "File Identification", 6, 1),
                    new FixedWidthField("STUSAB", "State Postal Abbreviation", 2, 7),
                    new FixedWidthField("SUMLEVEL", "Summary Level", 3, 9),
                    new FixedWidthField("COMPONENT", "Geographic Component", 2, 12),
                    new FixedWidthField("LOGRECNO", "Logical Record Number", 7, 14),
                    new FixedWidthField("US", "US", 1, 21),
                    new FixedWidthField("REGION", "Census Region", 1, 22),
                    new FixedWidthField("DIVISION", "Census Division", 1, 23),
                    new FixedWidthField("STATECE", "State (Census Code)", 2, 24),
                    new FixedWidthField("STATE", "State (FIPS Code)", 2, 26),
                    new FixedWidthField("COUNTY", "County of current residence", 3, 28),
                    new FixedWidthField("COUSUB", "County Subdivision (FIPS)", 5, 31),
                    new FixedWidthField("PLACE", "Place (FIPS Code)", 5, 36),
                    new FixedWidthField("TRACT", "Census Tract", 6, 41),
                    new FixedWidthField("BLKGRP", "Block Group", 1, 47),
                    new FixedWidthField("CONCIT", "Consolidated City", 5, 48),
                    new FixedWidthField("AIANHH", "American Indian Area", 4, 53),
                    new FixedWidthField("AIANHHFP", "_____", 5, 57),
                    new FixedWidthField("AIHHTLI", "_____", 1, 62),
                    new FixedWidthField("AITSCE", "_____", 3, 63),
                    new FixedWidthField("AITS", "_____", 5, 66),
                    new FixedWidthField("ANRC", "_____", 5, 71),
                    new FixedWidthField("CBSA", "Metropolitan and Micropolitan Statistical Area", 5, 76),
                    new FixedWidthField("CSA", "Combined Statistical Area", 3, 81),
                    new FixedWidthField("METDIV", "Metropolitan Statistical Area-Metropolitan Division", 5, 84),
                    new FixedWidthField("MACC", "Metropolitan Area Central City", 1, 89),
                    new FixedWidthField("MEMI", "Metropolitan/Micropolitan Indicator Flag", 1, 90),
                    new FixedWidthField("NECTA", "New England City and Town Area", 5, 91),
                    new FixedWidthField("CNECTA", "New England City and Town Combined Statistical Area", 3, 96),
                    new FixedWidthField("NECTADIV", "New England City and Town Area Division", 5, 99),
                    new FixedWidthField("UA", "Urban Area", 5, 104),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    new FixedWidthField("CDCURR", "Current Congressional District ***", 2, 114),
                    new FixedWidthField("SLDU", "State Legislative District Upper", 3, 116),
                    new FixedWidthField("SLDL", "State Legislative District Lower", 3, 119),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    new FixedWidthField("SUBMCD", "Subminor Civil Division (FIPS)", 5, 136),
                    new FixedWidthField("SDELM", "State-School District (Elementary)", 5, 141),
                    new FixedWidthField("SDSEC", "State-School District (Secondary)", 5, 146),
                    new FixedWidthField("SDUNI", "State-School District (Unified)", 5, 151),
                    new FixedWidthField("UR", "Urban/Rural", 1, 156),
                    new FixedWidthField("PCI", "Principal City Indicator", 1, 157),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    new FixedWidthField("PUMA5", "Public Use Microdata Area – 5% File", 5, 169),
                    //new FixedWidthField("BLANK", "_____", _, _),
                    new FixedWidthField("GEOID", "Geographic Identifier", 40, 179),
                    new FixedWidthField("NAME", "Area Name", 200, 219, FixedWidthTypes.STRING, FixedWidthTerminators.NEWLINE)
                    //new FixedWidthField("BLANK", "_____", _, _),                     
                    });
                }
                return GeographyFileReader._columns;
            }
        }

        #endregion Columns






    }
}

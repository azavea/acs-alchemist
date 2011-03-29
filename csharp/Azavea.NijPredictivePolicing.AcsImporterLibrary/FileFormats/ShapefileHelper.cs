using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats
{
    public class ShapefileHelper
    {
        public static GeometryFactory GetGeomFactory()
        {
            //Geographic - 4269 - North America NAD83, Geographic, decimal degrees
            //Geographic - 4326 - World WGS84, Geographic, decimal degrees
            return new GeometryFactory(new PrecisionModel(), 4269);
        }


        public static System.Data.DataTable GetTable(string shapefilename, AcsState acsState)
        {
            return Shapefile.CreateDataTable(shapefilename, acsState.ToString(), ShapefileHelper.GetGeomFactory());
        }
    }
}

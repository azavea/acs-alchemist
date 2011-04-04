using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using log4net;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.CoordinateSystems;

namespace Azavea.NijPredictivePolicing.Common
{
    public class Utilities
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Copies stream "from" to stream "to" until it can't read anymore data.  This function 
        /// is built into .NET 4.0 and later, but until we upgrade, this will do.
        /// </summary>
        /// <param name="from">The stream to read from</param>
        /// <param name="to">The stream to write to</param>
        public static void CopyTo(Stream from, Stream to)
        {
            byte[] buffer = new byte[4096];
            int read = 0;
            while ((read = from.Read(buffer, 0, buffer.Length)) > 0)
                to.Write(buffer, 0, read);
        }

                /// <summary>
        /// Copies stream "from" to stream "to" until it can't read anymore data.  This function 
        /// is built into .NET 4.0 and later, but until we upgrade, this will do.
        /// </summary>
        /// <param name="from">The stream to read from</param>
        /// <param name="to">The stream to write to</param>
        public static void CopyToWithProgress(Stream from, long expectedLength, Stream to)
        {
            byte[] buffer = new byte[4096];
            int numBytesRead = 0, lastProgress = 0;
            DateTime start = DateTime.Now;
            long position = 0;

            while ((numBytesRead = from.Read(buffer, 0, buffer.Length)) > 0)
            {
                position += numBytesRead;
                to.Write(buffer, 0, numBytesRead);

                int step = (int)((((double)position) / ((double)expectedLength)) * 100.0);
                if (((step % 5) == 0) && (lastProgress != step))
                {
                    TimeSpan elapsed = (DateTime.Now - start);
                    if (elapsed.TotalSeconds > 1)
                    {
                        _log.DebugFormat("{0}% complete, {1:#0.0#} seconds elapsed, {2} bytes downloaded",
                            step, elapsed.TotalSeconds, position
                            );
                        lastProgress = step;
                    }
                }
            }
        }

        

        public static object GetAsType(Type destType, object value, object ifEmpty)
        {
            try
            {
                if ((!Convert.IsDBNull(value)) && (value != null))
                {
                    Type srcType = value.GetType();

                    if (srcType == destType)
                    {
                        return value;
                    }
                    else if ((srcType == typeof(double)) && (destType == typeof(string)))
                    {
                        //this is a special case for preserving as much precision on doubles as possible.
                        return Convert.ChangeType(((double)value).ToString("R"), destType);
                    }
                    else if ((srcType == typeof(string)) && (destType == typeof(bool)))
                    {
                        //special case for lowercase true/false
                        //this recognizes T, t, True, and true as all true, anything else is false.
                        string t = value as string;
                        value = ((!string.IsNullOrEmpty(t)) && ((t.ToLower() == true.ToString().ToLower()) || (t.ToLower() == "t")));
                        return value;
                    }
                    else if ((srcType == typeof(string)) && (destType.IsEnum))
                    {
                        return Enum.Parse(destType, (string)value);
                    }
                    else if ((destType == typeof(DateTime)) && (srcType == typeof(string)))
                    {
                        //get the ticks since Jan 1, 1601 (C# epoch) to the JS Epoch
                        DateTime JSepoch = new DateTime(1969, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);
                        //value = DateTime.FromFileTime((ticks * TimeSpan.TicksPerMillisecond) + JSepoch.ToFileTime());

                        //meant to convert a javascript (new Date().getTime()) result to a C# DateTime...
                        long ticks = -1;
                        if (long.TryParse((string)value, out ticks))
                            value = JSepoch.Add(TimeSpan.FromMilliseconds(ticks));
                        else
                            value = DateTime.Parse((string)value);

                        return value;
                    }
                    //add (datetime -> string) / (string -> datetime) conversions?
                    else
                    {
                        return Convert.ChangeType(value, destType);
                    }
                }
            }
            catch { }
            return ifEmpty;
        }


        /// <summary>
        /// Never worry about DBNull, or .Net type conversion again!
        /// (usually fast, and safe!)
        /// </summary>
        public static T GetAs<T>(object value, T ifEmpty)
        {
            try
            {
                if ((!Convert.IsDBNull(value)) && (value != null))
                {
                    Type destType = typeof(T);
                    Type srcType = value.GetType();

                    if (srcType == destType)
                    {
                        return (T)value;
                    }
                    else if ((srcType == typeof(string)) && (string.IsNullOrEmpty(value as string)))
                    {
                        return ifEmpty;
                    }
                    else if ((srcType == typeof(double)) && (destType == typeof(string)))
                    {
                        //this is a special case for preserving as much precision on doubles as possible.
                        return (T)Convert.ChangeType(((double)value).ToString("R"), destType);
                    }
                    else if ((srcType == typeof(string)) && (destType == typeof(bool)))
                    {
                        //special case for lowercase true/false
                        //this recognizes T, t, True, and true as all true, anything else is false.
                        string t = value as string;
                        value = ((!string.IsNullOrEmpty(t)) && ((t.ToLower() == true.ToString().ToLower()) || (t.ToLower() == "t")));
                        return (T)value;
                    }
                    else if ((srcType == typeof(string)) && (destType.IsEnum))
                    {
                        return (T)Enum.Parse(destType, (string)value);
                    }
                    else if ((destType == typeof(DateTime)) && (srcType == typeof(string)))
                    {
                        //get the ticks since Jan 1, 1601 (C# epoch) to the JS Epoch
                        DateTime JSepoch = new DateTime(1969, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);
                        //value = DateTime.FromFileTime((ticks * TimeSpan.TicksPerMillisecond) + JSepoch.ToFileTime());

                        //meant to convert a javascript (new Date().getTime()) result to a C# DateTime...
                        long ticks = -1;
                        if (long.TryParse((string)value, out ticks))
                            value = JSepoch.Add(TimeSpan.FromMilliseconds(ticks));
                        else
                            value = DateTime.Parse((string)value);

                        return (T)value;
                    }
                    //add (datetime -> string) / (string -> datetime) conversions?
                    else
                    {
                        return (T)Convert.ChangeType(value, destType);
                    }
                }
            }
            catch { }
            return ifEmpty;
        }

        public static void DisplayEnum(string label, Type enumType)
        {
            var levels = Enum.GetValues(enumType);
            _log.Debug(label);
            foreach (var value in levels)
            {
                _log.DebugFormat("{0}:{1}", value.ToString(), (int)value);
            }
        }

        public static string EnsureMaxLength(string str, int maxlen)
        {
            return (str.Length <= maxlen) ? str : str.Substring(0, maxlen);
        }


        public static IGeometry IEnvToIGeometry(IEnvelope env)
        {
            ICoordinate[] coords = new Coordinate[5];
            coords[0] = new Coordinate(env.MinX, env.MinY);
            coords[1] = new Coordinate(env.MaxX, env.MinY);
            coords[2] = new Coordinate(env.MaxX, env.MaxY);
            coords[3] = new Coordinate(env.MinX, env.MaxY);
            coords[4] = new Coordinate(env.MinX, env.MinY);
            var poly = new Polygon(new LinearRing(coords));

            return poly;
        }

        public static Point GetCellFeetForProjection(double feet)
        {
            const double FEET_PER_METER = 3.2808399;
            const string webMercator1984 = "PROJCS[\"WGS_1984_Web_Mercator\", GEOGCS[\"GCS_WGS_1984_Major_Auxiliary_Sphere\", DATUM[\"WGS_1984_Major_Auxiliary_Sphere\", SPHEROID[\"WGS_1984_Major_Auxiliary_Sphere\",6378137.0,0.0]], PRIMEM[\"Greenwich\",0.0], UNIT[\"Degree\",0.0174532925199433]], PROJECTION[\"Mercator_1SP\"], PARAMETER[\"False_Easting\",0.0], PARAMETER[\"False_Northing\",0.0], PARAMETER[\"Central_Meridian\",0.0], PARAMETER[\"latitude_of_origin\",0.0], UNIT[\"Meter\",1.0]]";

            var meters = (feet / FEET_PER_METER);

            CoordinateSystemFactory csf = new CoordinateSystemFactory();
            var webMercatorCS = csf.CreateFromWkt(webMercator1984);

            var f = new CoordinateTransformationFactory();
            var proj = f.CreateFromCoordinateSystems(webMercatorCS, GeographicCoordinateSystem.WGS84);

            var xStep = proj.MathTransform.Transform(new double[] { meters, 0 });
            var yStep = proj.MathTransform.Transform(new double[] { 0, meters });

            return new Point(xStep[0], yStep[1]);
        }




    }
}

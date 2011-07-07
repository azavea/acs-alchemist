﻿/*
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
using System.IO;
using Newtonsoft.Json;
using log4net;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GisSharpBlog.NetTopologySuite.Features;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

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
        /// Simple Stream to Stream function, logs progress every 5%, as long as at least
        /// 1 second has passed
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

        
        /// <summary>
        /// Non-generic version of the function 'GetAs<T>'
        /// </summary>
        /// <param name="destType"></param>
        /// <param name="value"></param>
        /// <param name="ifEmpty"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Enumerates over an Enumerated type, and outputs the contents to the logger
        /// </summary>
        /// <param name="label"></param>
        /// <param name="enumType"></param>
        public static void DisplayEnum(string label, Type enumType)
        {
            var levels = Enum.GetValues(enumType);
            _log.Debug(label);
            foreach (var value in levels)
            {
                _log.DebugFormat("{0}:{1}", value.ToString(), (int)value);
            }
        }

        /// <summary>
        /// Enumerates over an Enumerated type, and outputs the contents to the logger
        /// </summary>
        /// <param name="label"></param>
        /// <param name="enumType"></param>
        public static void DisplayEnumKeysOnly(string label, Type enumType)
        {
            var levels = Enum.GetValues(enumType);
            _log.Debug(label);
            foreach (var value in levels)
            {
                _log.Debug(value.ToString());
            }
        }

        /// <summary>
        /// Simple helper to ensure a given string is no longer than maxlen
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxlen"></param>
        /// <returns></returns>
        public static string EnsureMaxLength(string str, int maxlen)
        {
            return (str.Length <= maxlen) ? str : str.Substring(0, maxlen);
        }

        /// <summary>
        /// Simple helper to build a polygon using an IEnvelope
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Overly elaborate helper function for getting the feet->meter->wgs84 conversion
        /// </summary>
        /// <param name="widthFeet"></param>
        /// <param name="heightFeet"></param>
        /// <returns></returns>
        [Obsolete("Replaced with something better")]
        public static Point GetCellFeetForProjection(double widthFeet, double heightFeet)
        {
            const double FEET_PER_METER = 3.2808399;
            const string webMercator1984 = "PROJCS[\"WGS_1984_Web_Mercator\", GEOGCS[\"GCS_WGS_1984_Major_Auxiliary_Sphere\", DATUM[\"WGS_1984_Major_Auxiliary_Sphere\", SPHEROID[\"WGS_1984_Major_Auxiliary_Sphere\",6378137.0,0.0]], PRIMEM[\"Greenwich\",0.0], UNIT[\"Degree\",0.0174532925199433]], PROJECTION[\"Mercator_1SP\"], PARAMETER[\"False_Easting\",0.0], PARAMETER[\"False_Northing\",0.0], PARAMETER[\"Central_Meridian\",0.0], PARAMETER[\"latitude_of_origin\",0.0], UNIT[\"Meter\",1.0]]";

            CoordinateSystemFactory csf = new CoordinateSystemFactory();
            var webMercatorCS = csf.CreateFromWkt(webMercator1984);

            var f = new CoordinateTransformationFactory();
            var proj = f.CreateFromCoordinateSystems(webMercatorCS, GeographicCoordinateSystem.WGS84);

            var widthMeters = (widthFeet / FEET_PER_METER);
            var heightMeters = (heightFeet / FEET_PER_METER);

            var xStep = proj.MathTransform.Transform(new double[] { widthMeters, 0 });
            var yStep = proj.MathTransform.Transform(new double[] { 0, heightMeters });

            return new Point(xStep[0], yStep[1]);
        }

        /// <summary>
        /// Helper function for building a CoordinateSystemFactory using a file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static ICoordinateSystem GetCoordinateSystemByWKTFile(string filename)
        {
            CoordinateSystemFactory csf = new CoordinateSystemFactory();
            return csf.CreateFromWkt(File.ReadAllText(filename));
        }

        /// <summary>
        /// Helper function for building a transformation between two coordinate systems (useful when reprojecting)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static ICoordinateTransformation BuildTransformationObject(ICoordinateSystem src, ICoordinateSystem dest)
        {
            var f = new CoordinateTransformationFactory();
            return f.CreateFromCoordinateSystems(src, dest);
        }

        /// <summary>
        /// Helper function for applying a reprojection transformation on a collection of Features
        /// between a given file, and WGS84
        /// </summary>
        /// <param name="features"></param>
        /// <param name="wktFilename"></param>
        /// <returns></returns>
        public static List<Feature> ReprojectFeaturesTo(List<Feature> features, string wktFilename)
        {
            var destCRS = GetCoordinateSystemByWKTFile(wktFilename);
            var trans = BuildTransformationObject(GeographicCoordinateSystem.WGS84, destCRS);


            _log.DebugFormat("Reprojecting {0} features from \"{1}\" to \"{2}\"",
                features.Count,
                trans.SourceCS.AuthorityCode,
                (trans.TargetCS.AuthorityCode != -1) ? trans.TargetCS.AuthorityCode.ToString() : trans.TargetCS.Name);

            return ReprojectFeatures(features, trans);
        }

        /// <summary>
        /// Helper function for applying a reprojection transformation on a collection of IGeometry(s)
        /// between a given file, and WGS84
        /// </summary>
        /// <param name="features"></param>
        /// <param name="wktFilename"></param>
        /// <returns></returns>
        public static List<IGeometry> ReprojectFeaturesTo(List<IGeometry> features, string wktFilename)
        {
            var destCRS = GetCoordinateSystemByWKTFile(wktFilename);
            var trans = BuildTransformationObject(GeographicCoordinateSystem.WGS84, destCRS);


            _log.DebugFormat("Reprojecting {0} features from \"{1}\" to \"{2}\"",
                features.Count,
                trans.SourceCS.AuthorityCode,
                (trans.TargetCS.AuthorityCode != -1) ? trans.TargetCS.AuthorityCode.ToString() : trans.TargetCS.Name);

            return ReprojectFeatures(features, trans);
        }

        /// <summary>
        /// Helper function for applying a reprojection transformation on a list of Features
        /// </summary>
        /// <param name="features"></param>
        /// <param name="wktFilename"></param>
        /// <returns></returns>
        public static List<Feature> ReprojectFeatures(List<Feature> features, ICoordinateTransformation trans)
        {
            for (int i = 0; i < features.Count; i++)
            {
                features[i].Geometry = ReprojectGeometry(features[i].Geometry, trans);
            }
            return features;
        }

        /// <summary>
        /// Helper function for applying a reprojection transformation on a list of IGeometry(s)
        /// </summary>
        /// <param name="features"></param>
        /// <param name="wktFilename"></param>
        /// <returns></returns>
        public static List<IGeometry> ReprojectFeatures(List<IGeometry> features, ICoordinateTransformation trans)
        {
            for (int i = 0; i < features.Count; i++)
            {
                features[i] = ReprojectGeometry(features[i], trans);
            }
            return features;
        }
        

        /// <summary>
        /// Helper function for applying a reprojection transformation on a single IGeometry
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static IGeometry ReprojectGeometry(IGeometry geom, ICoordinateTransformation trans)
        {
            double[] srcPt = new double[2], pt = null;
            foreach (ICoordinate coord in geom.Coordinates)
            {
                srcPt[0] = coord.X;
                srcPt[1] = coord.Y;

                pt = trans.MathTransform.Transform(srcPt);

                coord.X = pt[0];
                coord.Y = pt[1];
            }
            return geom;
        }




    }
}

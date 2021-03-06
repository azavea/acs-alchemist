﻿/*
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
using Azavea.NijPredictivePolicing.ACSAlchemistLibrary;
using Azavea.NijPredictivePolicing.Common;
using log4net;
using Azavea.NijPredictivePolicing.ACSAlchemistLibrary.Transfer;
using Azavea.NijPredictivePolicing.Common.DB;
using Azavea.NijPredictivePolicing.ACSAlchemistLibrary.FileFormats;
using Azavea.NijPredictivePolicing.Common.Data;
using System.IO;

namespace Azavea.NijPredictivePolicing.ACSAlchemist
{
    /// <summary>
    /// Encapsulates our command line arguments, and our main 'lifecycle' of how we imagine users will interact with the app.
    /// Ensures we have files downloaded, databases created, etc.
    /// </summary>
    public class ImportJob
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Command Line Stuff

        public static CmdLineArg[] Arguments = new CmdLineArg[] {
            new CmdLineArg() { Flag = "s", Description = "State Code", DataType=typeof(AcsState), PropertyName="State"},
            new CmdLineArg() { Flag = "y", Description = "Year", DataType=typeof(AcsState), PropertyName="Year"},
            new CmdLineArg() { Flag = "e", Description = "Filter Spatially by Census Summary Level", DataType=typeof(string), PropertyName="SummaryLevel"},            
            new CmdLineArg() { Flag = "v", Description = "Filter data by variable name file", DataType=typeof(string), PropertyName="IncludedVariableFile"},                        
            new CmdLineArg() { Flag = "f", Description = "Filter Spatially by optional shapefile", DataType=typeof(string), PropertyName="ExportFilterShapefile"},
            
            //new CmdLineArg() { Flag = "ExportFilterSRID", Description = "SRID to use for shapefile filter if none is provided in the shapefile", DataType=typeof(string), PropertyName="ExportFilterSRID"},

            new CmdLineArg() { Flag = "j", Display=false, DataType=typeof(string), PropertyName="JobName"},
            new CmdLineArg() { Flag = "jobName", Description = "Specify a name for this job / shapefile", DataType=typeof(string), PropertyName="JobName"},
            new CmdLineArg() { Flag = "reuse", Display=false,  DataType=typeof(string), PropertyName="ReusePreviousJobTable"},
            new CmdLineArg() { Flag = "reuseJob", Description = "Reuse the database from a previous job", DataType=typeof(string), PropertyName="ReusePreviousJobTable"},
            
            new CmdLineArg() { Flag = "outputProjection", Description = "Provide the .prj file of a desired projection use", DataType=typeof(string), PropertyName="OutputProjection"},

            new CmdLineArg() { Flag = "exportToShape", Description = "Export results to shapefile", DataType=typeof(string), PropertyName="ExportToShapefile"},
            new CmdLineArg() { Flag = "exportToGrid", Description = "Export to shapefile of vector cells of given feet", DataType=typeof(string), PropertyName="ExportToGrid"},

            new CmdLineArg() { Flag = "gridEnvelope", Description = "Align the grid cells to an envelope in a file", DataType=typeof(string), PropertyName="GridEnvelope"},            
            new CmdLineArg() { Flag = "includeEmptyGridCells", Display=false,   DataType=typeof(string), PropertyName="IncludeEmptyGridCells"},
            new CmdLineArg() { Flag = "includeEmptyGeometries", Description = "Keeps empty grid cells during export", DataType=typeof(string), PropertyName="IncludeEmptyGridCells"},

            new CmdLineArg() { Flag = "outputFolder", Description = "Specify where you'd like the results saved", DataType=typeof(string), PropertyName = "OutputFolder"},
            new CmdLineArg() { Flag = "workingFolder", Description = "Specify where you'd like temporary files saved", DataType=typeof(string), PropertyName = "WorkingFolder"},
            new CmdLineArg() { Flag = "preserveJam", Description = "Optional flag to preserve non-numeric margin of error values", DataType=typeof(string), PropertyName="PreserveJam"},
            
            new CmdLineArg() { Flag = "listYears", Description = "Lists config file locations for each year it knows about", DataType=typeof(string), PropertyName="ListYears"},
            new CmdLineArg() { Flag = "listStateCodes", Description = "Displays a list of available state codes", DataType=typeof(string), PropertyName="DisplayStateCodes"},
            new CmdLineArg() { Flag = "listSummaryLevels", Description = "Displays a list of available census summary levels", DataType=typeof(string), PropertyName="DisplaySummaryLevels"},
            new CmdLineArg() { Flag = "stripGEOIDColumn", Description = "Adds an extra column to the shapefile output named \"GEOID_STRP\" that contains the same data as the \"GEOID\" column but without the \"15000US\" prefix", DataType=typeof(string), PropertyName = "AddStrippedGEOIDcolumn" }
            //This command is now kind of useless now that we discovered how mangled these variable names actually are
            //new CmdLineArg() { Flag = "exportVariables", Description = "Exports a CSV of all variables to allVariables.csv", DataType=typeof(string), PropertyName="ExportVariables"}
        };

        public string ArgumentLine;

        public AcsState State { get; set; }
        public string Year { get; set; }

        public string ExportFilterShapefile { get; set; }
        //public string ExportFilterSRID { get; set; }
        public string ExportVariables { get; set; }
        public string IncludedVariableFile { get; set; }
        public string JobName { get; set; }
        public string SummaryLevel { get; set; }
        public string ReusePreviousJobTable { get; set; }
        public string ExportToShapefile { get; set; }
        public string ExportToGrid { get; set; }

        public string DisplaySummaryLevels { get; set; }
        public string DisplayStateCodes { get; set; }
        public string ListYears { get; set; }


        public string GridEnvelope { get; set; }
        public string OutputProjection { get; set; }
        public string IncludeEmptyGridCells { get; set; }

        public string PreserveJam { get; set; }
        public string AddStrippedGEOIDcolumn { get; set; }
        public string AddGeometryAttributesToOutput { get; set; }
        

        protected string _outputFolder;
        protected string _workingFolder;

        public string OutputFolder
        {
            get { return _outputFolder; }
            set
            {
                _outputFolder = FileUtilities.CleanPath(value);
            }
        }
        public string WorkingFolder
        {
            get { return _workingFolder; }
            set
            {
                _workingFolder = FileUtilities.CleanPath(value);
            }
        }






        public ImportJob()
        {
            this.State = AcsState.None;
        }

        /// <summary>
        /// Replaced by 'FindDelimAfterWhitespace'
        /// </summary>
        /// <param name="str"></param>
        /// <param name="idx"></param>
        /// <param name="delims"></param>
        /// <returns></returns>
        public int IndexOf(string str, int idx, params char[] delims)
        {
            List<int> indices = new List<int>(delims.Length);
            foreach (char d in delims)
            {
                int next = str.IndexOf(d, idx);
                if (next >= 0)
                    indices.Add(next);
            }
            return (indices.Count > 0) ? indices.Min() : -1;
        }

        /// <summary>
        /// Finds the first of any of our delimiters, 
        /// but makes sure that the space before the delim contains a whitespace char
        /// </summary>
        /// <param name="str"></param>
        /// <param name="idx"></param>
        /// <param name="delims"></param>
        /// <returns></returns>
        public int FindDelimAfterWhitespace(string str, int idx, params char[] delims)
        {
            List<int> indices = new List<int>(delims.Length);
            foreach (char d in delims)
            {
                int next = str.IndexOf(d, idx);
                if (next < 0)
                    continue;

                //if we find a delim, make sure it is prefixed by whitespace
                if (next > 0)
                {
                    //if this one isn't prefixed by whitespace, check the next one
                    bool done = char.IsWhiteSpace(str[next - 1]);
                    while (!done)
                    {
                        next = str.IndexOf(d, next + 1);
                        done = (next == -1) || char.IsWhiteSpace(str[next - 1]);
                    }

                    //didn't find a better one
                    if (next < 0)
                        continue;

                    //did find it
                }

                indices.Add(next);
            }
            return (indices.Count > 0) ? indices.Min() : -1;
        }

        /// <summary>
        /// Takes a collection of command line arguments, and parses them / loads them onto our properties.
        /// Will also accept a single filename, and treat that as if it were provided at the command line
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool Load(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                sb.Append(args[i]).Append(' ');
            }
            string line = sb.ToString();
            this.ArgumentLine = line;
            //char delim = '-';
            char[] delims = new char[] { (char)45, (char)8211, (char)65533 }; //'-', '–', '?';
            int idx = FindDelimAfterWhitespace(line, 0, delims);     //int idx = line.IndexOf(delim);

            if (idx == -1)
            {
                //if we have arguments, but they didn't include flags, it's probably a file,
                //look for the file.
                if (File.Exists(args[0]))
                {
                    var lines = File.ReadAllLines(args[0]);
                    sb = new StringBuilder();
                    foreach (string chunk in lines)
                    {
                        if (chunk.StartsWith("#") || chunk.StartsWith("/"))
                            continue;

                        if (string.IsNullOrEmpty(chunk))
                            continue;

                        string tmpChunk = Utilities.TrimComments(chunk, '#');

                        sb.Append(tmpChunk.Trim()).Append(" ");
                    }
                    line = sb.ToString();
                }
                else
                {
                    _log.ErrorFormat("The arguments file you provided could not be read: {0}", args[0]);
                    return false;
                }
            }


            // iterate over our input, copying values to our properties

            var thisType = typeof(ImportJob);
            idx = FindDelimAfterWhitespace(line, 0, delims);     //int idx = line.IndexOf(delim);
            while (idx >= 0)
            {
                int nextSpace = line.IndexOf(' ', idx + 1);

                string flag = line.Substring(idx + 1, nextSpace - (idx + 1));
                string contents = string.Empty;

                idx += 1 + flag.Length;
                int end = FindDelimAfterWhitespace(line, idx, delims);   //int end = line.IndexOf(delim, idx);
                if (end == -1)
                {
                    end = line.Length;
                }
                if (end > idx)
                {
                    contents = line.Substring(idx, end - idx).Trim();
                    if (string.IsNullOrEmpty(contents))
                    {
                        contents = true.ToString();
                    }

                    idx = end;
                }
                idx = FindDelimAfterWhitespace(line, idx, delims);       //idx = line.IndexOf(delim, idx);


                // actually set the properties now
                for (int p = 0; p < Arguments.Length; p++)
                {
                    var arg = Arguments[p];
                    if (arg.Flag == flag)
                    {
                        var prop = thisType.GetProperty(arg.PropertyName);
                        object defval = (prop.PropertyType == typeof(AcsState)) ? (object)AcsState.None : null;
                        prop.SetValue(this, Utilities.GetAsType(prop.PropertyType, contents, defval), null);
                        break;
                    }
                }
            }
            return true;
        }

        #endregion Command Line Stuff

        #region Progress Reporting Stuff

        public delegate void ProgressUpdateHandler(int percentage);
        public event ProgressUpdateHandler OnProgressUpdated;

        /// <summary>
        /// Event added so we can provide some basic feedback while this is running in a background thread
        /// </summary>
        /// <param name="value"></param>
        public void UpdateProgress(int value)
        {
            if (this.OnProgressUpdated != null)
            {
                if (IsCancelled()) { value = 100; } 

                this.OnProgressUpdated(value);
            }
        }

        protected AcsDataManager _manager = null;
        protected bool _cancelled = false;
        public bool IsCancelled()
        {
            lock (this)
            {
                if (_cancelled && (_manager != null))
                {
                    _manager.Cancel();
                }

                return _cancelled;
            }
        }

        public void Cancel()
        {
            lock (this)
            {
                _cancelled = true;

                if (_manager != null)
                {
                    _manager.Cancel();
                }
            }
        }

        #endregion Progress Reporting Stuff


        /// <summary>
        /// Performs our main 'lifecycle'
        /// </summary>
        /// <returns></returns>
        public bool ExecuteJob()
        {
            _manager = null; _cancelled = false;

            this.UpdateProgress(0);
            if (IsCancelled()) { _log.Debug("Job Cancelled..."); return false; }

            DateTime startTime = DateTime.Now;
            try
            {
                if (!string.IsNullOrEmpty(DisplaySummaryLevels))
                {
                    Utilities.DisplayEnum<BoundaryLevels>("Summary Levels:", "{1} - {0}",
                        new HashSet<string>(new string[] { "None" })
                        );
                    return true;
                }
                if (!string.IsNullOrEmpty(DisplayStateCodes))
                {
                    Utilities.DisplayEnumKeysOnly("State Codes:", typeof(AcsState),
                        //Do not display internal control value
                        new HashSet<string>(new string[] { "None" })
                        );
                    return true;
                }
                if (!string.IsNullOrEmpty(ListYears))
                {
                    var years = Settings.LoadYearConfigs();
                    //_log.DebugFormat("I found {0} year config files ", years.Count);
                    _log.InfoFormat("I found {0} years available:", years.Count);
                    foreach (var key in years.Keys)
                    {
                        //_log.DebugFormat("{0} - {1}", key, years[key].GetFilename());

                        _log.InfoFormat(" * {0} ", key);
                    }
                    _log.InfoFormat(Environment.NewLine + "Done!");
                    return true;
                }

                if (this.State == AcsState.None)
                {
                    _log.Error("Invalid State selected, please select a state from the list and try again.");
                    return false;
                }

                if ((string.IsNullOrEmpty(this.JobName)) || (this.JobName == true.ToString()))
                {
                    this.JobName = string.Format("{0}_{1}_{2}", this.Year, this.State, DateTime.Now.ToShortDateString().Replace('/', '_'));
                    _log.DebugFormat("Jobname was empty, using {0}", this.JobName);
                }

                WorkingFolder = FileUtilities.CleanPath(WorkingFolder);
                var manager = new AcsDataManager(this.State, WorkingFolder, this.Year);
                this._manager = manager;
                manager.WorkOffline = this.WorkOffline;
                //TODO: check for bad combinations of inputs
                manager.SummaryLevel = this.SummaryLevel;
                manager.ExportFilterFilename = this.ExportFilterShapefile;
                manager.DesiredVariablesFilename = IncludedVariableFile;
                manager.ReusePreviousJobTable = (!string.IsNullOrEmpty(this.ReusePreviousJobTable));
                manager.OutputProjectionFilename = this.OutputProjection;
                manager.PreserveJam = (!string.IsNullOrEmpty(this.PreserveJam));
                manager.AddStrippedGEOIDcolumn = (!string.IsNullOrEmpty(this.AddStrippedGEOIDcolumn));
                manager.AddGeometryAttributesToOutput = (!string.IsNullOrEmpty(this.AddGeometryAttributesToOutput));
                manager.OutputFolder = FileUtilities.CleanPath(OutputFolder);
                manager.IncludeEmptyGridCells = (!string.IsNullOrEmpty(this.IncludeEmptyGridCells));


                if (FileUtilities.SafePathEnsure(OutputFolder) != OutputFolder)
                {
                    _log.ErrorFormat("Unable to set or create output folder, ( {0} ) exiting", OutputFolder);
                    _log.FatalFormat("Unable to set or create output folder, ( {0} ) exiting", OutputFolder);
                    return false;
                }

                this.UpdateProgress(25);
                if (IsCancelled()) { _log.Debug("Job Cancelled..."); return false; }

                if (string.IsNullOrEmpty(this.OutputProjection))
                {
                    _log.Warn(Constants.Warning_MissingProjection);
                }

                _log.Debug("\r\n/*************************************/");
                _log.Info("   Loading Prerequisites...");
                _log.Debug("/*************************************/");

                bool hasPrerequesites = true;
                hasPrerequesites &= !IsCancelled() && manager.CheckColumnMappingsFile();
                hasPrerequesites &= !IsCancelled() && manager.CheckCensusAggregatedDataFile();
                hasPrerequesites &= !IsCancelled() && manager.CheckDatabase();
                hasPrerequesites &= !IsCancelled() && manager.CheckShapefiles();

                if (!hasPrerequesites)
                {
                    _log.Info("Loading Prerequisites... Failed!");
                    _log.Error("Import cannot continue, one or more prerequisites failed!");
                    return false;
                }
                _log.Debug("Loading Prerequisites... Done!\r\n");
                this.UpdateProgress(35);
                if (IsCancelled()) { _log.Debug("Job Cancelled..."); return false; }


                if (!string.IsNullOrEmpty(IncludedVariableFile) && !string.IsNullOrEmpty(this.JobName))
                {
                    _log.Info("\r\n/*************************************/");
                    _log.Info("   Importing requested variables...    ");
                    _log.Info("/*************************************/");

                    if (!manager.CheckBuildVariableTable(this.JobName))
                    {
                        _log.Error("Importing requested variables... Failed! A problem was detected, exiting.");
                        return false;
                    }
                    else
                    {
                        _log.Debug("Importing requested variables... Done!");
                    }
                }
                this.UpdateProgress(50);
                if (IsCancelled()) { _log.Debug("Job Cancelled..."); return false; }

                if (!string.IsNullOrEmpty(ExportToShapefile))
                {
                    _log.Debug("\r\n/*************************************/");
                    _log.Info("   Exporting to shapefile... ");
                    _log.Debug("/*************************************/");

                    if (!manager.ExportShapefile(this.JobName))
                    {
                        _log.Error("There was an error while exporting the shapefile");

                        _log.Debug("\r\nExporting to shapefile... Failed!");
                    }
                    else
                    {
                        _log.Debug("Exporting to shapefile... Done!");
                    }
                }
                else
                {
                    _log.Debug("\r\n/****  No shapefile export requested ****/\r\n");
                }
                this.UpdateProgress(75);
                if (IsCancelled()) { _log.Debug("Job Cancelled..."); return false; }

                if (!string.IsNullOrEmpty(ExportToGrid))
                {
                    _log.Debug("\r\n/*************************************/");
                    _log.Info("   Exporting to gridded shapefile...");
                    _log.Debug("/*************************************/");

                    manager.GridEnvelopeFilename = GridEnvelope;
                    manager.SetGridParam(ExportToGrid);

                    _log.DebugFormat("Exporting all requested variables to fishnet shapefile with grid cell size {0} ", ExportToGrid);
                    manager.ExportGriddedShapefile(this.JobName);

                    _log.Debug("Exporting to gridded shapefile... Done!");
                }
                else
                {
                    _log.Debug("\r\n/****  No gridded shapefile export requested ****/\r\n");
                }
                this.UpdateProgress(100);

                _log.Info("Done!");

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error thrown during import job ", ex);
                _log.Fatal("Please see the errors log file for details");
            }
            finally
            {
                TimeSpan elapsed = DateTime.Now - startTime;
                if (IsCancelled())
                {
                    _log.DebugFormat("Job was cancelled and stopped at {0} seconds", elapsed.TotalSeconds);
                }
                else
                {
                    _log.DebugFormat("Job completed in {0} seconds", elapsed.TotalSeconds);
                }
            }

            return false;
        }



        /// <summary>
        /// Saves a nicely commented job file at the location provided
        /// </summary>
        /// <param name="filename"></param>
        public void SaveJobFile(string filename)
        {
            var thisType = typeof(ImportJob);
            string nl = Environment.NewLine;

            StringBuilder sb = new StringBuilder(2048);
            sb.Append("#").Append(nl);
            sb.Append("#  ACS Alchemist save file").Append(nl);
            sb.AppendFormat("#  Generated automatically at {0} - {1}",
                DateTime.Now.ToLongDateString(),
                DateTime.Now.ToShortTimeString()
                ).Append(nl);
            sb.Append(nl).Append(nl);
            HashSet<string> distinctProps = new HashSet<string>();


            for (int p = 0; p < Arguments.Length; p++)
            {
                var arg = Arguments[p];

                if (!arg.Display)
                {
                    //these generally are aliases, and don't have descriptions, skip them before we check unique props
                    continue;
                }

                if (!distinctProps.Add(arg.PropertyName))
                {
                    //already added, some command line params refer to the same properties,
                    //just keep the first one we find, not both, that's confusing to the user.
                    continue;
                }


                var prop = thisType.GetProperty(arg.PropertyName);
                object val = prop.GetValue(this, null);
                if (prop.PropertyType == typeof(AcsState))
                    val = (int)val;

                if (prop.PropertyType == typeof(string))
                {
                    string valStr = val as string;

                    //skip it before it set it to empty so we can write it prettily
                    if (string.IsNullOrEmpty(valStr))
                        continue;

                    if (valStr == "true")
                        val = string.Empty;

                    if (!string.IsNullOrEmpty(valStr) && (valStr.Contains(' ')))
                    {
                        val = "\"" + val + "\"";
                    }
                }
                else if (prop.PropertyType == typeof(AcsState))
                {
                    val = ((AcsState)val).ToString();
                }

                if (arg.PropertyName == "SummaryLevel")
                {
                    val = val + "       #" + Utilities.GetAs<BoundaryLevels>(val, BoundaryLevels.census_blockgroups);
                }

                sb.Append("# ").Append(arg.Description).Append(nl);
                sb.Append("-" + arg.Flag).Append(" ").Append(val).Append(nl);
                sb.Append(nl);
            }

            File.WriteAllText(filename, sb.ToString());
        }


        public bool WorkOffline { get; set; }
    }
}

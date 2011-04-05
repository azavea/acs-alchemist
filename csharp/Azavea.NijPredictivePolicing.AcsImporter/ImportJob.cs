using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azavea.NijPredictivePolicing.AcsImporterLibrary;
using Azavea.NijPredictivePolicing.Common;
using log4net;
using Azavea.NijPredictivePolicing.AcsImporterLibrary.Transfer;
using Azavea.NijPredictivePolicing.Common.DB;
using Azavea.NijPredictivePolicing.AcsImporterLibrary.FileFormats;
using Azavea.NijPredictivePolicing.Common.Data;
using System.IO;

namespace Azavea.NijPredictivePolicing.AcsDataImporter
{
    public class ImportJob
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Command Line Stuff

        public static ImportArg[] Arguments = new ImportArg[] {
            new ImportArg() { Flag = "s", Description = "State Code", DataType=typeof(AcsState), PropertyName="State"},
            new ImportArg() { Flag = "v", Description = "Filter data by variable name file", DataType=typeof(string), PropertyName="IncludedVariableFile"},
            
            new ImportArg() { Flag = "e", Description = "Filter Spatially by Census Summary Level", DataType=typeof(string), PropertyName="SummaryLevel"},
            new ImportArg() { Flag = "f", Description = "Filter Spatially by optional filename of WKT geometries", DataType=typeof(string), PropertyName="WKTFilterFilename"},
            

            new ImportArg() { Flag = "j", Display=false, DataType=typeof(string), PropertyName="JobName"},
            new ImportArg() { Flag = "jobName", Description = "Specify a name for this job / shapefile", DataType=typeof(string), PropertyName="JobName"},

            new ImportArg() { Flag = "r", Display=false,  DataType=typeof(string), PropertyName="ReplaceTable"},
            new ImportArg() { Flag = "replaceJob", Description = "Replace an existing job / shapefile", DataType=typeof(string), PropertyName="ReplaceTable"},
            
            new ImportArg() { Flag = "exportToShape", Description = "Export results to shapefile", DataType=typeof(string), PropertyName="ExportToShapefile"},
            new ImportArg() { Flag = "exportToGrid", Description = "Export results to fishnetted shapefile where value = # feet", DataType=typeof(string), PropertyName="ExportToGrid"},

            new ImportArg() { Flag = "gridEnvelope", Description = "Align the grid cells to an envelope in a file", DataType=typeof(string), PropertyName="GridEnvelope"},
            new ImportArg() { Flag = "outputProjection", Description = "Provide the WKT of a desired projection to operate in", DataType=typeof(string), PropertyName="OutputProjection"},
            new ImportArg() { Flag = "includeEmptyGridCells", Description = "Keeps empty grid cells during export", DataType=typeof(string), PropertyName="IncludeEmptyGridCells"},

            new ImportArg() { Flag = "outputFolder", Description = "Specify where you'd like the results saved", DataType=typeof(string), PropertyName="OutputFolder"},
            
            new ImportArg() { Flag = "listStateCodes", Description = "Displays a list of available state codes", DataType=typeof(string), PropertyName="DisplayStateCodes"},
            new ImportArg() { Flag = "listSummaryLevels", Description = "Displays a list of available boundary levels", DataType=typeof(string), PropertyName="DisplaySummaryLevels"},
            new ImportArg() { Flag = "exportVariables", Description = "Exports a CSV of all variables to allVariables.csv", DataType=typeof(string), PropertyName="ExportVariables"}
        };



        public AcsState State { get; set; }

        public string WKTFilterFilename { get; set; }
        public string ExportVariables { get; set; }
        public string IncludedVariableFile { get; set; }
        public string JobName { get; set; }
        public string SummaryLevel { get; set; }
        public string ReplaceTable { get; set; }
        public string ExportToShapefile { get; set; }
        public string ExportToGrid { get; set; }
        
        public string DisplaySummaryLevels { get; set; }
        public string DisplayStateCodes { get; set; }
        public string GridEnvelope { get; set; }
        public string OutputProjection { get; set; }
        public string IncludeEmptyGridCells { get; set; }
        public string OutputFolder { get; set; }
        
        

        


        public ImportJob()
        {
            this.State = AcsState.None;
        }


        public void Load(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                sb.Append(args[i]).Append(' ');
            }
            string line = sb.ToString();
            char delim = '-';


            if (line.IndexOf(delim) == -1)
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
                        sb.Append(chunk).Append(" ");
                    }
                    line = sb.ToString();
                }
            }


            var thisType = typeof(ImportJob);
            int idx = line.IndexOf(delim);
            while (idx >= 0)
            {
                int nextSpace = line.IndexOf(' ', idx + 1);

                string flag = line.Substring(idx + 1, nextSpace - (idx + 1));
                string contents = string.Empty;

                idx += 1 + flag.Length;
                int end = line.IndexOf(delim, idx);
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
                idx = line.IndexOf(delim, idx);


                for (int p = 0; p < Arguments.Length; p++)
                {
                    var arg = Arguments[p];
                    if (arg.Flag == flag)
                    {
                        var prop = thisType.GetProperty(arg.PropertyName);
                        prop.SetValue(this, Utilities.GetAsType(prop.PropertyType, contents, null), null);
                        break;
                    }
                }
            }
        }

        #endregion Command Line Stuff


        public bool ExecuteJob()
        {
            DateTime startTime = DateTime.Now;
            try
            {
                if (!string.IsNullOrEmpty(DisplaySummaryLevels))
                {
                    Utilities.DisplayEnum("Summary Levels:", typeof(BoundaryLevels));
                    return true;
                }
                if (!string.IsNullOrEmpty(DisplayStateCodes))
                {
                    Utilities.DisplayEnum("State Codes:", typeof(AcsState));
                    return true;
                }

                if (this.State != AcsState.None)
                {
                    if (string.IsNullOrEmpty(this.JobName))
                    {
                        this.JobName = string.Format("{0}_{1}", this.State, DateTime.Now.ToShortDateString().Replace('/', '_'));
                        _log.DebugFormat("Jobname was empty, using {0}", this.JobName);
                    }

                    _log.Debug("");
                    _log.Debug("Loading Prerequisites...");

                    var manager = new AcsDataManager(this.State);
                    if ((manager.CheckColumnMappingsFile())
                        && (manager.CheckBlockGroupFile())
                        && (manager.CheckDatabase())
                        && (manager.CheckShapefiles())
                        )
                    {
                        _log.Debug("Done!");
                        _log.Debug("");



                        if (!string.IsNullOrEmpty(ExportVariables))
                        {
                            _log.Debug("Exporting table of all available variables to allVariables.csv");

                            var variablesDT = manager.GetAllSequenceVariables();

                            CommaSeparatedValueWriter writer = new CommaSeparatedValueWriter("allVariables.csv");
                            FileWriterHelpers.WriteDataTable(writer, variablesDT);

                            _log.Debug("Done!");
                            return true;
                        }



                        manager.SummaryLevel = this.SummaryLevel;
                        manager.ExportFilterFilename = this.WKTFilterFilename;
                        manager.DesiredVariablesFilename = IncludedVariableFile;
                        manager.ReplaceTable = (!string.IsNullOrEmpty(this.ReplaceTable));
                        manager.OutputProjectionFilename = this.OutputProjection;
                        manager.OutputFolder = this.OutputFolder;

                        if (string.IsNullOrEmpty(this.OutputProjection))
                        {
                            _log.Warn("*********************");
                            _log.Warn(
@"IMPORTANT!:  
    You have not specified an output projection, meaning the resulting shapefile will
be in unprojected WGS84.  Your filtering geometries, envelope, grid cell sizes, 
and all other parameters must match that projection.");
                            _log.Warn("*********************");
                        }

                        manager.IncludeEmptyGridCells = (!string.IsNullOrEmpty(this.IncludeEmptyGridCells));
                        

                        if (!string.IsNullOrEmpty(IncludedVariableFile) && !string.IsNullOrEmpty(this.JobName))
                        {
                            _log.Debug("Importing all requested variables...");

                            if (!manager.CheckBuildVariableTable(this.JobName))
                            {
                                _log.Error("There was a problem building the variable table, exiting");
                                return false;
                            }
                            _log.Debug("Done!");
                        }

                        if (!string.IsNullOrEmpty(ExportToShapefile))
                        {
                            _log.Debug("Exporting all requested variables to shapefile");
                            if (manager.ExportShapefile(this.JobName))
                            {
                                _log.Debug("Exported successfully");
                            }
                            else
                            {
                                _log.Debug("There was an error while exporting the shapefile");
                            }
                            _log.Debug("Done!");
                        }

                        if (!string.IsNullOrEmpty(ExportToGrid))
                        {
                            manager.GridEnvelopeFilename = GridEnvelope;
                            manager.SetGridParam(ExportToGrid);

                            _log.DebugFormat("Exporting all requested variables to fishnet shapefile with grid cell size {0} ", ExportToGrid);
                            manager.ExportGriddedShapefile(this.JobName);
                            _log.Debug("Done!");
                        }

                        

                    }
                    else
                    {
                        return false;
                    }
                }

                //if (!string.IsNullOrEmpty(this.RunTests))
                //{
                //    AcsDataManager manager = new AcsDataManager(AcsState.Wyoming);
                //    string filename = manager.GetLocalShapefileName();
                //    ShapefileHelper helper = new ShapefileHelper();
                //    helper.OpenShapefile(filename);                    
                //    var schemaDT = helper.GetSchema();
                //    _log.DebugFormat("Here");
                //}






                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Error while executing job", ex);
            }
            finally
            {
                TimeSpan elapsed = DateTime.Now - startTime;
                _log.DebugFormat("Job completed in {0} seconds", elapsed.TotalSeconds);
            }

            return false;
        }


    }
}

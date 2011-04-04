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

            new ImportArg() { Flag = "jobName", Description = "Specify a name for this job / shapefile", DataType=typeof(string), PropertyName="JobName"},
            new ImportArg() { Flag = "replaceJob", Description = "Replace an existing job / shapefile", DataType=typeof(string), PropertyName="ReplaceTable"},
            

            new ImportArg() { Flag = "exportToShape", Description = "Export results to shapefile", DataType=typeof(string), PropertyName="ExportToShapefile"},
            

            new ImportArg() { Flag = "listStateCodes", Description = "Displays a list of available state codes", DataType=typeof(string), PropertyName="DisplayStateCodes"},
            new ImportArg() { Flag = "listBoundaryLevels", Description = "Displays a list of available boundary levels", DataType=typeof(string), PropertyName="DisplayBoundaryLevels"},
            new ImportArg() { Flag = "listVariables", Description = "Displays a list of all available variables", DataType=typeof(string), PropertyName="DoListVariables"}



            //TODO: add option for fishnet config file?  spatial output config file?  something?


            //new ImportArg() { Flag = "t", Description = "Run Tests", PropertyName="RunTests"},
            //new ImportArg() { Flag = "a", Description = "Optional thing that a", PropertyName="PropA"},
            //new ImportArg() { Flag = "b", Description = "Optional thing that b", PropertyName="PropB"},
            //new ImportArg() { Flag = "c", Description = "Optional thing that c", PropertyName="PropC"}
        };

        public AcsState State { get; set; }

        public string WKTFilterFilename { get; set; }
        public string DoListVariables { get; set; }
        public string IncludedVariableFile { get; set; }
        public string JobName { get; set; }
        public string SummaryLevel { get; set; }
        public string ReplaceTable { get; set; }
        public string ExportToShapefile { get; set; }
        public string DisplayBoundaryLevels { get; set; }
        public string DisplayStateCodes { get; set; }
        
        
        
        
        
        //public string RunTests { get; set; }
        public string PropA { get; set; }
        public string PropB { get; set; }
        public string PropC { get; set; }


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
                if (!string.IsNullOrEmpty(DisplayBoundaryLevels))
                {
                    Utilities.DisplayEnum("Boundary Levels:", typeof(BoundaryLevels));
                }
                if (!string.IsNullOrEmpty(DisplayStateCodes))
                {
                    Utilities.DisplayEnum("State Codes:", typeof(AcsState));
                }

                if (this.State != AcsState.None)
                {
                    if (string.IsNullOrEmpty(this.JobName))
                    {
                        this.JobName = string.Format("{0}_{1}", this.State, DateTime.Now.ToShortDateString().Replace('/', '_'));
                        _log.DebugFormat("Jobname was empty, using {0}", this.JobName);
                    }


                    var manager = new AcsDataManager(this.State);
                    if ((manager.CheckColumnMappingsFile())
                        && (manager.CheckBlockGroupFile())
                        && (manager.CheckDatabase())
                        && (manager.CheckShapefiles())
                        )
                    {

                        if (!string.IsNullOrEmpty(DoListVariables))
                        {
                            var allVars = manager.GetAllSequenceVariableTableIds();
                            foreach (string varname in allVars)
                            {
                                _log.Debug(" " + varname);
                            }
                        }


                        
                        manager.SummaryLevel = this.SummaryLevel;
                        manager.WKTFilterFilename = this.SummaryLevel;
                        manager.IncludedVariableFile = IncludedVariableFile;
                        manager.ReplaceTable = (!string.IsNullOrEmpty(this.ReplaceTable));

                        if (!string.IsNullOrEmpty(IncludedVariableFile) && !string.IsNullOrEmpty(this.JobName))
                        {
                            if (!manager.CheckBuildVariableTable(this.JobName))
                            {
                                _log.Error("There was a problem building the variable table, exiting");
                                return false;
                            }
                        }


                        if (!string.IsNullOrEmpty(ExportToShapefile))
                        {
                            _log.Debug("Exporting!");
                            manager.ExportShapefile(this.JobName);
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

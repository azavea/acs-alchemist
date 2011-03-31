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

        public static ImportArg[] Arguments = new ImportArg[] {
            new ImportArg() { Flag = "f", Description = "Optional filename containing WellKnownTexts of desired output polygons", DataType=typeof(string), PropertyName="ShapeFilename"},
            new ImportArg() { Flag = "s", Description = "State Code (specifying this will download that state's data)", DataType=typeof(AcsState), PropertyName="State"},
            //new ImportArg() { Flag = "t", Description = "Run Tests", PropertyName="RunTests"},
            new ImportArg() { Flag = "a", Description = "Optional thing that a", PropertyName="PropA"},
            new ImportArg() { Flag = "b", Description = "Optional thing that b", PropertyName="PropB"},
            new ImportArg() { Flag = "c", Description = "Optional thing that c", PropertyName="PropC"}
        };

        public AcsState State { get; set; }

        public string ShapeFilename { get; set; }
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
                string flag = line.Substring(idx + 1, 1);
                string contents = string.Empty;

                idx += 2;
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


        public bool ExecuteJob()
        {
            DateTime startTime = DateTime.Now;
            try
            {
                if (this.State != AcsState.None)
                {
                    var manager = new AcsDataManager(this.State);

                    if ((manager.CheckColumnMappingsFile())
                        && (manager.CheckBlockGroupFile())
                        && (manager.CheckDatabase())
                        && (manager.CheckShapefile())
                        )
                    {
                        //var dt = manager.GetShapefileData();
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

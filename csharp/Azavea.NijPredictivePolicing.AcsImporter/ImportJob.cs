using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azavea.NijPredictivePolicing.AcsDataImporter
{
    public class ImportJob
    {
        public static ImportArg[] Arguments = new ImportArg[] {
            new ImportArg() { Flag = "f", Description = "Optional filename containing WellKnownTexts of desired output polygons", DataType=typeof(string), PropertyName="ShapeFilename"},
            new ImportArg() { Flag = "a", Description = "Optional thing that a", DataType=typeof(string), PropertyName="PropA"},
            new ImportArg() { Flag = "b", Description = "Optional thing that b", DataType=typeof(string), PropertyName="PropB"},
            new ImportArg() { Flag = "c", Description = "Optional thing that c", DataType=typeof(string), PropertyName="PropC"}
        };

        public string ShapeFilename { get; set; }
        public string PropA { get; set; }
        public string PropB { get; set; }
        public string PropC { get; set; }


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
                        thisType.GetProperty(arg.PropertyName).SetValue(this, contents, null);
                        break;
                    }
                }
            }
        
        }


    }
}

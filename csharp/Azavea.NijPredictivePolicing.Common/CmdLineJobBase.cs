using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Azavea.NijPredictivePolicing.Common
{
    public class CmdLineJobBase
    {
        public void Load(string[] args, CmdLineArg[] availFlags, object dest)
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


            var thisType = dest.GetType();
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


                for (int p = 0; p < availFlags.Length; p++)
                {
                    var arg = availFlags[p];
                    if (arg.Flag == flag)
                    {
                        var prop = thisType.GetProperty(arg.PropertyName);
                        prop.SetValue(dest, Utilities.GetAsType(prop.PropertyType, contents, null), null);
                        break;
                    }
                }
            }
        }

    }
}

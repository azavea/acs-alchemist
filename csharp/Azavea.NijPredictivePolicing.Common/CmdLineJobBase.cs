using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using log4net;

namespace Azavea.NijPredictivePolicing.Common
{
    public class CmdLineJobBase
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool Load(string[] args, CmdLineArg[] availFlags, object dest)
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

            HashSet<string> knownFlags = new HashSet<string>();
            foreach (var arg in availFlags)
            {
                var flag = arg.Flag.ToLower();

                if (!knownFlags.Contains(flag))
                {
                    knownFlags.Add(flag);
                }
                else
                {
                    _log.ErrorFormat("Programmer error, the flag {0} appears more than once ", flag);
                    return false;
                }
            }


            var thisType = dest.GetType();
            int idx = line.IndexOf(delim);  //grab the first flag
            while (idx >= 0)
            {
                int nextSpace = line.IndexOf(' ', idx + 1);

                string flag = line.Substring(idx + 1, nextSpace - (idx + 1));
                string contents = string.Empty;

                idx += 1 + flag.Length;

                if (!knownFlags.Contains(flag))
                {
                    _log.ErrorFormat("Invalid parameter: {0}", flag.Substring(0, Math.Min(512, flag.Length)));
                    return false;
                }

                int end = 0;
                int nextQuote = line.IndexOf('\"', idx);
                if (nextQuote >= 0)
                {
                    int quoteEnd = line.IndexOf('\"', nextQuote + 1);
                    if (quoteEnd >= 0)
                    {
                        end = quoteEnd + 1;
                    }
                    else
                    {
                        _log.Error("Unterminated Quote found in arguments.");
                        return false;
                    }
                }
                else
                {
                    end = line.IndexOf(delim, idx);
                }


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
                    else if (contents.StartsWith("\""))
                    {
                        contents = contents.Trim('\"');
                    }

                    idx = end;
                }
                //find the next flag.
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

            return true;



        }

    }
}

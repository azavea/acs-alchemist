using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azavea.NijPredictivePolicing.Common
{
    /// <summary>
    /// Basic container describing a command line argument we'd like to accept
    /// </summary>
    public class CmdLineArg
    {
        public string Flag;
        public string PropertyName;
        public string Description;
        public Type DataType;
        public bool Display = true;
    }
}

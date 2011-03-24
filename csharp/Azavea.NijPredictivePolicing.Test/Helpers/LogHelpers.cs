using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Azavea.NijPredictivePolicing.Test.Helpers
{
    public static class LogHelpers
    {
        public static ILog ResetLogger(Type declaringType)
        {
            log4net.LogManager.ResetConfiguration();
            log4net.Config.BasicConfigurator.Configure();
            return log4net.LogManager.GetLogger(declaringType);
        }

        public static ILog ResetLogger(string declaringType)
        {
            log4net.LogManager.ResetConfiguration();
            log4net.Config.BasicConfigurator.Configure();
            return log4net.LogManager.GetLogger(declaringType);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using log4net;
using System.Configuration;
using System.IO;
using Azavea.NijPredictivePolicing.Test.Helpers;


namespace Azavea.NijPredictivePolicing.Test
{
    [TestFixture]
    public abstract class BaseTest
    {
        protected static ILog _log = null;

        public static readonly string BaseDir;

        static BaseTest()
        {
            BaseDir = @"..\..\..\Azavea.NijPredictivePolicing.Test\";
            //Fixes 32 vs 64 bit directory structure differences
            if (!Directory.Exists(BaseDir))
            {
                BaseDir = "..\\" + BaseDir;
                if (!Directory.Exists(BaseDir))
                {
                    Assert.Fail("Error: input directory not found at {0}", BaseDir);
                }
            }
            BaseDir = Path.GetFullPath(BaseDir);
        }

        [TestFixtureSetUp]
        public void Init()
        {
            _log = LogHelpers.ResetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

    }
}

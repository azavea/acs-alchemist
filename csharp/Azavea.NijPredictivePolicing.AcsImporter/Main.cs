using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Layout;
using Azavea.NijPredictivePolicing.Common;

namespace Azavea.NijPredictivePolicing.AcsDataImporter
{
    /// <summary>
    /// Contains main() entry point
    /// </summary>
    class Program
    {
        private static ILog _log = null;
        protected static void Init()
        {
            ConsoleAppender con = new ConsoleAppender();
            con.Layout = new PatternLayout("%message%newline");
            log4net.Config.BasicConfigurator.Configure(con);
            _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }


        protected static void ShowWelcomeScreen()
        {
            _log.Debug("");
            _log.Debug("         ,..");
            _log.Debug("       .'   |");
            _log.Debug("        |_,,'");
            _log.Debug("     ,--|   ,--.");
            _log.Debug("    '   |' |   ''      _____.  _____.: _____`_  /.   :.    __.   _____.");
            _log.Debug("     -- '   --.|            p|      /       ||      .\' //'  |.       p|");
            _log.Debug(" ,-^-.  ,-^'.  |'`'.   _,..,||    ,'    `... |   ,| '/  |,...-/  _,..,||");
            _log.Debug(" [   ,==.   | |.   |  /|   ,'|  ,'     '    /|   `O//    '      /|   .'|");
            _log.Debug("  `--    --'    --'    '`''--' ^-----'  `''\"`-    \"-     '`''\"'  '`'''-'");
            _log.Debug("");
            _log.Debug("+--------------------------------------------+");
            _log.Debug("|   Welcome to the Acs Data Importer         |");
            _log.Debug("+--------------------------------------------+");
            _log.Debug("");
            _log.Debug("");
        }

        protected static void LoadConfigFile()
        {
            Settings.ConfigFile = new Config("importer.config");
        }

        protected static void DisplayOptions()
        {
            foreach (var arg in ImportJob.Arguments)
            {
                _log.DebugFormat("  -{0}:\t{1}", arg.Flag, arg.Description);
            }
        }

        static void Main(string[] args)
        {
            Init();
            ShowWelcomeScreen();


            ImportJob job = new ImportJob();
            if ((args != null) && (args.Length > 0))
            {
                _log.Debug("Loading arguments...");
                job.Load(args);

                job.ExecuteJob();
            }
            else
            {
                DisplayOptions();
            }


            _log.Debug("Done! Press ANY KEY to Quit");
            Console.ReadKey();
        }

        
    }
}

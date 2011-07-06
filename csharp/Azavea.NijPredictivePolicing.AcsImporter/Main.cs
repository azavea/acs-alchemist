using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Layout;
using Azavea.NijPredictivePolicing.Common;
using System.IO;

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
            Settings.ConfigFile = new Config(Path.Combine(Settings.ApplicationPath, "importer.config"));
            if (Settings.ConfigFile.IsEmpty())
            {
                Settings.RestoreDefaults();
            }
        }



        protected static void DisplayOptions()
        {
            foreach (var arg in ImportJob.Arguments)
            {
                if (!arg.Display)
                    continue;

                _log.DebugFormat("  -{0, -15}: {1}", arg.Flag, arg.Description);
            }
            _log.Debug("");
            _log.Debug("**Pro Tip!  You can place all your arguments in a text file, and call the importer with just that filename");
            _log.Debug("");
        }

        static int Main(string[] args)
        {
            Init();
            LoadConfigFile();
            ShowWelcomeScreen();


            ImportJob job = new ImportJob();
            if ((args != null) && (args.Length > 0))
            {
                _log.Debug("Loading arguments...");
                if (!job.Load(args))
                {
                    _log.Debug("Error while loading arguments. Exiting.");
                    return -1;
                }

                if (!job.ExecuteJob())
                {
                    _log.Fatal("An error was encountered while performing the operation.  Please examine the log output and try again if necessary.");
                }
            }
            else
            {
                DisplayOptions();
            }

//#if DEBUG
//            _log.Debug("Done! Press ANY KEY to Quit");
//            Console.ReadKey();
//#endif
            return 0;
        }

        
    }
}

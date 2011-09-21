/*
  Copyright (c) 2011 Azavea, Inc.
 
  This file is part of _SOLUTIONNAME_.

  _SOLUTIONNAME_ is free software: you can redistribute it and/or modify
  it under the terms of the GNU Lesser General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  _SOLUTIONNAME_ is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Lesser General Public License for more details.

  You should have received a copy of the GNU Lesser General Public License
  along with _SOLUTIONNAME_.  If not, see <http://www.gnu.org/licenses/>.
*/

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
    public class Program
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
            _log.Debug("|   Welcome to _SOLUTIONNAME_                |");
            _log.Debug("+--------------------------------------------+");
            _log.Debug("");
            _log.Debug("");
        }

        protected static void ShowCopyrightAndLicense()
        {
            _log.Info("+-----------------------------------------------------+");
            _log.Info(" _SOLUTIONNAME_  Copyright (C) 2011 Azavea, Inc.");
            _log.Info(" This program comes with ABSOLUTELY NO WARRANTY;");
            _log.Info(" This is free software, and you are welcome to redistribute it");
            _log.Info(" under the terms of the GNU Lesser General Public License");
            _log.Info("+-----------------------------------------------------+");
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

        public static int Main(string[] args)
        {
            //Console.ReadKey();

            Init();
            LoadConfigFile();
            ShowWelcomeScreen();
            ShowCopyrightAndLicense();


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

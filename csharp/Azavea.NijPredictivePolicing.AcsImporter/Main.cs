﻿/*
  Copyright (c) 2012 Azavea, Inc.
 
  This file is part of ACS Alchemist.

  ACS Alchemist is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  ACS Alchemist is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with ACS Alchemist.  If not, see <http://www.gnu.org/licenses/>.
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

namespace Azavea.NijPredictivePolicing.ACSAlchemist
{
    /// <summary>
    /// Contains main() entry point
    /// </summary>
    public class Program
    {
        private static ILog _log = null;

        public const string DefaultConfigPath = "configs/AcsAlchemist.json.config";

        protected static void Init()
        {
            try
            {
                //attempt to load logging configuration
                System.IO.FileInfo configFile = new System.IO.FileInfo(Path.Combine(Settings.ApplicationPath, "Logging.config"));
                log4net.Config.XmlConfigurator.ConfigureAndWatch(configFile);
                _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            }
            catch
            {

                ConsoleAppender con = new ConsoleAppender();
                con.Layout = new PatternLayout("%message%newline");
                //con.Threshold = log4net.Core.Level.Info;
                log4net.Config.BasicConfigurator.Configure(con);
                _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            }
        }


        public static void ShowWelcomeScreen()
        {
            _log.Debug("+--------------------------------------------+");
            _log.Debug("|   Welcome to ACS Alchemist                 |");
            _log.Debug("+--------------------------------------------+");
            _log.Debug("");
			
			_log.Debug("This project was supported by Award No. 2010-DE-BX-K004,");
			_log.Debug("awarded by the National Institute of Justice, Office of ");
			_log.Debug("Justice Programs, U.S. Department of Justice. The opinions, ");
			_log.Debug("findings, and conclusions or recommendations expressed in this ");
			_log.Debug("software are those of the author(s) and do not necessarily reflect");
			_log.Debug("those of the Department of Justice or Temple University.");

			_log.Debug("The software was developed by Azavea in connection with the ");
			_log.Debug("National Institute of Justice grant awarded to Jerry Ratcliffe");
			_log.Debug("and Ralph Taylor of Temple University's Center for Security and Crime Science.");
			
            _log.Debug(" The source code is released under a GPLv3 license and is available at: ");
            _log.Debug("https://github.com/azavea/acs-alchemist");
            _log.Debug("");
        }

        public static void ShowCopyrightAndLicense()
        {
            _log.Info("+-------------------------------------------------------------+");
            _log.Info(" ACS Alchemist - Copyright (c) 2011-2012 Azavea Inc.");
            _log.Info(" This program comes with ABSOLUTELY NO WARRANTY;");
            _log.Info(" This is free software, and you are welcome to redistribute it");
            _log.Info(" under the terms of the GNU General Public License");

            //TODO: libraries we need to list here?

            _log.Info("+-------------------------------------------------------------+");
        }


        /// <summary>
        /// Searches for our config file in the application path, if it can't find it,
        /// just uses defaults
        /// </summary>
        protected static void LoadConfigFile()
        {
            string configName = Path.Combine(Settings.ApplicationPath, DefaultConfigPath);
            Settings.ConfigFile = new Config(configName);
            if (Settings.ConfigFile.IsEmpty())
            {
                _log.FatalFormat("Config file {0} missing or empty!  Cannot continue, exiting", DefaultConfigPath);
                Environment.Exit((int)Constants.ExitCodes.BadConfig);
            }

            //search for our 'per-year' config files that explain which paths to check, file naming, etc.
            Settings.LoadYearConfigs();
        }


        /// <summary>
        /// Iterates over our Arguments collection and displays descriptions, flags, etc.
        /// </summary>
        protected static void DisplayOptions()
        {
            int maxCols = 80;   //try and wrap the output to look nice 

            foreach (var arg in ImportJob.Arguments)
            {
                if (!arg.Display)
                    continue;

                string line = string.Format("  -{0, -15}: {1}", arg.Flag, arg.Description);
                if (line.Length > maxCols)
                {
                    while (line.Length > 0)
                    {
                        int numChars = Math.Min(maxCols, line.Length);

                        _log.InfoFormat(line.Substring(0, numChars));
                        if ((line.Length - maxCols) > 0)
                        {
                            line = "".PadLeft(arg.Flag.Length + 5, ' ') + line.Substring(numChars, line.Length - maxCols);
                        }
                        else
                            line = string.Empty;
                    }
                }
                else
                {
                    _log.InfoFormat(line);
                }
            }
            _log.Info("");
            _log.Info("**Pro Tip!  You can place all your arguments in a text file, and call AcsAlchemist.exe with just that filename");
            _log.Info("");
        }

        /// <summary>
        /// Our main entry point for the command line app
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
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
/*
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
using System.IO;
using Azavea.NijPredictivePolicing.Common;
using log4net.Appender;
using log4net.Layout;
using Azavea.NijPredictivePolicing.ACSAlchemist;
using Azavea.NijPredictivePolicing.ACSAlchemistLibrary;
using Azavea.NijPredictivePolicing.ACSAlchemistLibrary.FileFormats;

namespace Azavea.NijPredictivePolicing.AcsAlchemistGui
{
    /// <summary>
    /// This class is meant to act as our interface between the GUI and the rest of the importer logic
    /// </summary>
    public class FormController
    {
        static FormController()
        {
            _instance = new FormController();
        }

        private static FormController _instance;
        public static FormController Instance
        {
            get { return _instance; }
        }


        #region FormController Properties

        /// <summary>
        /// Our resident instance of the current 'job', typically holds 
        /// command line params, runs an import, does exports, etc.
        /// </summary>
        public ImportJob JobInstance { get; set; }


        /// <summary>
        /// Returns a modifiable list of years that correspond to ACS data that
        /// can be used as a live binding source for a form
        /// </summary>
        public List<string> AvailableYears
        {
            get
            {
                if (_availableYears != null) { return _availableYears; }

                //a quick initialization
                _availableYears = new List<string>(Settings.LoadYearConfigs().Keys);
                _availableYears.Insert(0, string.Empty);    //blank option
                return _availableYears;
            }
            set { _availableYears = value; }
        }
        protected List<string> _availableYears;


        /// <summary>
        /// Returns a modifiable list of states that correspond to ACS data that
        /// can be used as a live binding source for a form
        /// </summary>
        public List<AcsState> AvailableStates
        {
            get
            {
                if (_availableStates != null) { return _availableStates; }

                //a quick initialization
                _availableStates = Utilities.GetEnumAsList<AcsState>(
                    new HashSet<AcsState>(new AcsState[] { AcsState.None })
                    );
                _availableStates.Insert(0, AcsState.None);    //blank option

                return _availableStates;
            }
            set { _availableStates = value; }
        }
        protected List<AcsState> _availableStates;

        /// <summary>
        /// Returns a modifiable list of summary levels that correspond to ACS data that
        /// can be used as a live binding source for a form
        /// </summary>
        public List<BoundaryLevels> AvailableLevels
        {
            get
            {
                if (_availableLevels != null) { return _availableLevels; }

                //a quick initialization
                _availableLevels = Utilities.GetEnumAsList<BoundaryLevels>(
                    new HashSet<BoundaryLevels>(new BoundaryLevels[] { BoundaryLevels.None })
                    );
                _availableLevels.Insert(0, BoundaryLevels.None);    //blank option

                return _availableLevels;
            }
            set { _availableLevels = value; }
        }
        protected List<BoundaryLevels> _availableLevels;

        /// <summary>
        /// Returns a modifiable list of projections that correspond to SRID.csv
        /// </summary>
        public List<string> AvailableProjections
        {
            get
            {
                if (_availableProjections != null) { return _availableProjections; }

                //a quick initialization
                _availableProjections = Utilities.ListAllCoordinateSystemIDs();

                return _availableProjections;
            }
            set { _availableProjections = value; }
        }
        protected List<string> _availableProjections;




        #endregion FormController Properties









        #region Initialization Boilerplate


        private static ILog _log = null;
        internal void InitLogging(IAppender appenderObj)
        {
            if (_log != null)
            {
                _log.Debug("Logging already initialized");
                return;
            }

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

            if (appenderObj != null)
            {
                ((log4net.Repository.Hierarchy.Hierarchy)log4net.LogManager.GetLoggerRepository()).Root.AddAppender(appenderObj);
            }
        }


        protected void ShowWelcomeScreen()
        {
            _log.Debug("+--------------------------------------------+");
            _log.Debug("|   Welcome to ACS Alchemist                 |");
            _log.Debug("+--------------------------------------------+");
            _log.Debug("");
            _log.Debug(" This tool was developed by Azavea in collaboration with Jerry Ratcliffe and ");
            _log.Debug("Ralph Taylor of Temple University Center for Security and Crime Science and ");
            _log.Debug("partially funded by a Predictive Policing grant from the National Institute ");
            _log.Debug("of Justice (Award # 2010-DE-BX-K004).  The source code is released under a ");
            _log.Debug("GPLv3 license and is available at:  http://github.com/azavea/ACS-Alchemist/");
         
            _log.Debug("");
        }

        protected void ShowCopyrightAndLicense()
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
        protected void LoadConfigFile()
        {
            Settings.ConfigFile = new Config(Path.Combine(Settings.ApplicationPath, "AcsAlchemist.json.config"));
            if (Settings.ConfigFile.IsEmpty())
            {
                Settings.RestoreDefaults();
            }

            Settings.LoadYearConfigs();
        }


        /// <summary>
        /// Run initialization sequence (logging, load config, welcome, copyright, license, create job instance)
        /// </summary>
        internal void Initialize()
        {
            //InitLogging();

            LoadConfigFile();
            ShowWelcomeScreen();
            ShowCopyrightAndLicense();
            this.JobInstance = new ImportJob();
        }


        #endregion Initialization Boilerplate









        /// <summary>
        /// Performs some sanity checks on our variables file
        /// (does it exist, can I read it, does it have at least one variable, etc.)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal bool ValidateVariablesFile(string filename, out string errorMessage)
        {
            if (!File.Exists(filename))
            {
                errorMessage = "File does not exist";
                return false;
            }

            //EXTRA CREDIT: read / parse file, make sure it is a valid variables file

            //DOUBLE BONUS EXTRA CREDIT: read / parse file, lookup corresponding description in sequence file, display to user

            errorMessage = string.Empty;
            return true;
        }


        /// <summary>
        /// replaces our current job instance with a new one loaded from a saved file
        /// </summary>
        /// <param name="filename"></param>
        internal void LoadNewJobInstance(string filename)
        {
            FormController.Instance.JobInstance = new ImportJob();
            FormController.Instance.JobInstance.Load(new string[] { filename });
        }

        /// <summary>
        /// There is no structure ensuring these defaults are the same that will show up when the form loads,
        /// This function is meant to reflect whatever the form defaults are.
        /// </summary>
        internal void NewDefaultJobInstance()
        {
            FormController.Instance.JobInstance = new ImportJob();

            //we need to set some defaults here, to keep things happy
            FormController.Instance.JobInstance.PreserveJam = "true";
            FormController.Instance.JobInstance.AddStrippedGEOIDcolumn = "true";
            FormController.Instance.JobInstance.IncludeEmptyGridCells = "true";
            FormController.Instance.JobInstance.Year = string.Empty;
            FormController.Instance.JobInstance.State = AcsState.None;
            FormController.Instance.JobInstance.SummaryLevel = BoundaryLevels.None.ToString();
        }

    }
}

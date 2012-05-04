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
                return _availableYears;
            }
            set { _availableYears = value; }
        }
        protected List<string> _availableYears;


        /// <summary>
        /// Returns a modifiable list of states that correspond to ACS data that
        /// can be used as a live binding source for a form
        /// </summary>
        public List<string> AvailableStates
        {
            get
            {
                if (_availableStates != null) { return _availableStates; }
                
                //a quick initialization                
                _availableStates = new List<string>(Utilities.GetEnumKeysAsList(
                    typeof(AcsState),
                    new HashSet<string>(new string[] { "None" })
                ));

                return _availableStates;
            }
            set { _availableStates = value; }
        }
        protected List<string> _availableStates;

        #endregion FormController Properties









        #region Initialization Boilerplate


        private static ILog _log = null;
        protected void InitLogging()
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
        }


        protected void ShowWelcomeScreen()
        {
            //_log.Warn("DEBUG - DEBUG - PAUSING FOR DRAMATIC EFFECT - DEBUG - DEBUG"); Console.ReadKey();
            _log.Debug("+--------------------------------------------+");
            _log.Debug("|   Welcome to ACS Alchemist                |");
            _log.Debug("+--------------------------------------------+");
            _log.Debug("");
            _log.Debug(" This tool was developed by Azavea in collaboration with");
            _log.Debug(" Jerry Ratcliffe and Ralph Taylor of Temple University and partially funded by ");
            _log.Debug(" a Predictive Policing grant from the National Institute of Justice ");
            _log.Debug(" (Award # 2010-DE-BX-K004). ");

            _log.Debug(" The source code is released under a GPLv3 license.");
            _log.Debug("");
        }

        protected void ShowCopyrightAndLicense()
        {
            _log.Info("+-----------------------------------------------------+");
            _log.Info(" ACS Alchemist  Copyright (C) 2012 Azavea, Inc.");
            _log.Info(" This program comes with ABSOLUTELY NO WARRANTY;");
            _log.Info(" This is free software, and you are welcome to redistribute it");
            _log.Info(" under the terms of the GNU General Public License");

            //TODO: are we obligated to list other libraries here?

            _log.Info("+-----------------------------------------------------+");
        }

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
            InitLogging();
            LoadConfigFile();
            ShowWelcomeScreen();
            ShowCopyrightAndLicense();
            this.JobInstance = new ImportJob();
        }



        #endregion Initialization Boilerplate








    }
}

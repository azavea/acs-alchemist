using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using log4net.Appender;
using log4net.Layout;
using log4net;
using Azavea.NijPredictivePolicing.Common;
using Azavea.NijPredictivePolicing.ACSAlchemistLibrary;
using Azavea.NijPredictivePolicing.ACSAlchemist;

namespace Azavea.NijPredictivePolicing.AcsAlchemistGui
{
    public partial class MainForm : Form
    {
        private static ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialize the controls and whatnot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                ShowLoadingSpinner();

                //TODO: special initializer for the logger / or something to get the output stream before the copyright / etc is shown

                var appender = new TextboxAppender(this.txtLogConsole);
                FormController.Instance.InitLogging(appender);

                //wait until after the form is loaded to do this, we want to show the log output on the form
                FormController.Instance.Initialize();

                //
                // Initialize the rest of the form
                //

                this.PopulateLists();
                this.AddDefaultTooltips();
                this.SmartToggler();
            }
            catch (Exception ex)
            {
                this.DisplayException("Form Load", ex);
            }
            finally
            {
                HideLoadingSpinner();
            }
        }




        /// <summary>
        /// Populates our 'year', 'state', 'summary level', and 'srid' controls, 
        /// as well as any other "choose from a set" controls that come up.
        /// </summary>
        protected void PopulateLists()
        {
            this.cboYear.DataSource = new BindingSource(FormController.Instance.AvailableYears, string.Empty);
            this.cboStates.DataSource = new BindingSource(FormController.Instance.AvailableStates, string.Empty);
            this.cboSummaryLevel.DataSource = new BindingSource(FormController.Instance.AvailableLevels, string.Empty);

            //NOTE! We're doing this initialization in "radioSRIDFromList_CheckedChanged",
            //instead of here, this is because it's expensive (we have to read through a whole file)
            //so we're shaving a half-second off the init, and moving it to a spot where the user shouldn't notice.
            //this.cboProjections.DataSource = new BindingSource(FormController.Instance.AvailableProjections, string.Empty);                        
        }

        /// <summary>
        /// collection of tooltip controls
        /// </summary>
        protected Dictionary<Control, ToolTip> _tooltips = new Dictionary<Control, ToolTip>();

        /// <summary>
        /// Helper for setting tooltips
        /// </summary>
        /// <param name="ctl"></param>
        /// <param name="label"></param>
        protected void SetTooltip(Control ctl, string label)
        {
            if (!this._tooltips.ContainsKey(ctl))
            {
                this._tooltips[ctl] = new ToolTip();
            }
            this._tooltips[ctl].SetToolTip(ctl, label);
        }


        protected void AddDefaultTooltips()
        {
            this.SetTooltip(chkPreserveJamValues, "When checked, does not attempt to force all error values to be numeric");
            this.SetTooltip(chkStripExtraGeoID, "When checked, it adds a copy of the GEOID column \"GEOID_STRP\" except without the \"15000US\" prefix ");
            this.SetTooltip(cboIncludeEmptyGeom, "When checked, it keeps all cells or polygons in the output, even if they don't have any data ");


        }





        private void openJobFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileJob.ShowDialog();
            txtJobFilePath.Text = openFileJob.FileName;
        }

        private void saveJobFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileJob.ShowDialog();
            // if it's not cancelled
            txtJobFilePath.Text = saveFileJob.FileName;
        }



        #region File Browsers

        /// <summary>
        /// Opens a file dialog for -- Variables File
        /// Extensions: *.txt, *.vars / All
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowseVariableFile_Click(object sender, EventArgs e)
        {
            if (ofdVariablesFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtVariableFilePath.Text = ofdVariablesFile.FileName;
                txtVariableFilePath.Focus();
            }
        }

        /// <summary>
        /// Browse for the "Output Folder"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowseOutputFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserOutputDir.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtOutputDirectory.Text = folderBrowserOutputDir.SelectedPath;
            }
        }

        /// <summary>
        /// Browse for "Grid boundary-alignment file"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowseFishnetEnvelopeFile_Click(object sender, EventArgs e)
        {
            if (ofdGridEnvelopeShp.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtFishnetEnvelopeFilePath.Text = ofdGridEnvelopeShp.FileName;
            }
        }

        /// <summary>
        /// Browse for the "Output Projection File"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowsePrjFile_Click(object sender, EventArgs e)
        {
            if (ofdOutputProjection.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtPrjFilePath.Text = ofdOutputProjection.FileName;
            }
        }

        /// <summary>
        /// Browse for the "Output Filter Boundary" Shapefile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowseBoundaryShpFile_Click(object sender, EventArgs e)
        {
            if (ofdExportBoundaryShp.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtBoundaryShpFilePath.Text = ofdExportBoundaryShp.FileName;
            }
        }

        #endregion File Browsers


        #region Event Boilerplate

        /// <summary>
        /// Perform all our form enable/disables in one place, so the form is always in a consistent state
        /// (this also makes this much easier to maintain)
        /// </summary>
        public void SmartToggler()
        {
            /** TODO: Anything that enables/disables */


            //if (radioDefaultSRID.Checked) { }     //nothing to do here

            //projection list
            cboProjections.Enabled = (radioSRIDFromList.Checked && !backgroundWorker1.IsBusy);

            //projection file controls
            txtPrjFilePath.Enabled = (radioSRIDFile.Checked && !backgroundWorker1.IsBusy);
            btnBrowsePrjFile.Enabled = (radioSRIDFile.Checked && !backgroundWorker1.IsBusy);


            {
                bool enabledIfIdle = !backgroundWorker1.IsBusy;

                this.btnShapefile.Enabled = enabledIfIdle;
                this.btnFishnet.Enabled = enabledIfIdle;

                this.cboYear.Enabled = enabledIfIdle;
                this.cboStates.Enabled = enabledIfIdle;
                this.cboSummaryLevel.Enabled = enabledIfIdle;
                this.txtVariableFilePath.Enabled = enabledIfIdle;
                this.txtOutputDirectory.Enabled = enabledIfIdle;

                this.radioDefaultSRID.Enabled = enabledIfIdle;
                this.radioSRIDFromList.Enabled = enabledIfIdle;
                this.radioSRIDFile.Enabled = enabledIfIdle;


                this.txtJobName.Enabled = enabledIfIdle;

                this.chkReplaceJob.Enabled = enabledIfIdle;
                this.txtFishnetCellSize.Enabled = enabledIfIdle;
                this.txtFishnetEnvelopeFilePath.Enabled = enabledIfIdle;
                this.cboIncludeEmptyGeom.Enabled = enabledIfIdle;
                this.txtBoundaryShpFilePath.Enabled = enabledIfIdle;
                this.chkPreserveJamValues.Enabled = enabledIfIdle;
                this.chkStripExtraGeoID.Enabled = enabledIfIdle;



                this.btnBrowseVariableFile.Enabled = enabledIfIdle;
                this.btnBrowseOutputFolder.Enabled = enabledIfIdle;
                this.btnBrowseBoundaryShpFile.Enabled = enabledIfIdle;
                this.btnBrowseFishnetEnvelopeFile.Enabled = enabledIfIdle;
            }
        }

        private void radioDefaultSRID_CheckedChanged(object sender, EventArgs e)
        {
            SmartToggler();
        }

        private void radioSRIDFromList_CheckedChanged(object sender, EventArgs e)
        {
            SmartToggler();

            ShowLoadingSpinner();
            //we're not doing this on startup, because it's expensive / slow,
            //so lets only do it when they ask us to:

            //if we haven't yet, populate the SRID dropdown:
            if (this.cboProjections.Items.Count == 0)
            {
                this.cboProjections.DataSource = new BindingSource(FormController.Instance.AvailableProjections, string.Empty);
            }

            HideLoadingSpinner();
        }

        private void radioSRIDFile_CheckedChanged(object sender, EventArgs e)
        {
            SmartToggler();
        }

        /// <summary>
        /// Sets the projection text as the tooltip, best way we have for showing what projection they've selected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboProjections_SelectedIndexChanged(object sender, EventArgs e)
        {
            //NOTE: Normally, I would optimize this call, but it seems incredibly fast on my machine,
            //please let me know if it seems slow, and I'll just read it into a dictionary and use that instead.
            var projectionText = Utilities.GetCoordinateSystemWKTByID(this.cboProjections.Text);

            this.SetTooltip(this.radioSRIDFromList, projectionText);
            this.SetTooltip(this.cboProjections, projectionText);
        }

        #endregion Event Boilerplate




        private void btnSaveMessageLog_Click(object sender, EventArgs e)
        {
            saveFileMessageLog.ShowDialog();
            txtMessageLogFilePath.Text = saveFileMessageLog.FileName;
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        /// <summary>
        /// A very basic error display helper --
        ///  NOTE! It is up to the developer to choose what exceptions are FATAL or not.
        /// This helper is for potentially NON-FATAL exceptions / or those that can be recovered from
        /// </summary>
        /// <param name="label"></param>
        /// <param name="ex"></param>
        protected void DisplayException(string label, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("An exception was caught during \"{0}\"{1}", label, Environment.NewLine);
            sb.AppendFormat("Message:{0}{1}{1}", ex.Message, Environment.NewLine);

            sb.Append("The program might not continue to run as expected, please restart the application");

            MessageBox.Show(sb.ToString(), "An exception was caught");
        }



        protected void ShowLoadingSpinner()
        {
            this.pgbStatus.Style = ProgressBarStyle.Continuous;
        }

        protected void HideLoadingSpinner()
        {
            this.pgbStatus.Style = ProgressBarStyle.Blocks;
        }




        protected void GatherInputs(bool isFishnet)
        {
            /** TODO: Gather all inputs and update our controller / job instance */

            var importObj = FormController.Instance.JobInstance;


            importObj.Year = cboYear.Text;                                             //1
            importObj.State = (AcsState)cboStates.SelectedValue;                       //2
            importObj.SummaryLevel = ((int)cboSummaryLevel.SelectedValue).ToString();  //3
            importObj.IncludedVariableFile = txtVariableFilePath.Text;                 //4
            importObj.OutputFolder = txtOutputDirectory.Text;                          //5

            //TODO: Stub in default output folder?



            //srid
            if (radioDefaultSRID.Checked)
            {
                importObj.OutputProjection = string.Empty;
            }
            if (radioSRIDFromList.Checked)
            {
                importObj.OutputProjection = this.cboProjections.Text;
            }
            if (radioSRIDFile.Checked)
            {
                importObj.OutputProjection = this.txtPrjFilePath.Text;
            }


            //stub in the default jobname
            if (string.IsNullOrEmpty(txtJobName.Text))
            {
                txtJobName.Text = string.Format("{0}_{1}_{2}", importObj.Year, importObj.State, DateTime.Now.ToShortDateString().Replace('/', '_'));
                _log.DebugFormat("Jobname was empty, using {0}", txtJobName.Text);
            }


            //job name:
            importObj.JobName = txtJobName.Text;
            importObj.ReusePreviousJobTable = (chkReplaceJob.Checked) ? string.Empty : "true";

            //shapefile:
            importObj.ExportToShapefile = (!isFishnet) ? "true" : string.Empty;

            //fishnet:
            importObj.ExportToGrid = (isFishnet) ? txtFishnetCellSize.Text : string.Empty;
            importObj.GridEnvelope = (isFishnet) ? txtFishnetEnvelopeFilePath.Text : string.Empty;

            //this applies to both shapefile and fishnet exports (despite its name)
            importObj.IncludeEmptyGridCells = (cboIncludeEmptyGeom.Checked) ? "true" : string.Empty;

            //clip bounds
            importObj.ExportFilterShapefile = txtBoundaryShpFilePath.Text;

            //optional flags
            importObj.PreserveJam = (chkPreserveJamValues.Checked) ? "true" : string.Empty;
            importObj.AddStrippedGEOIDcolumn = (chkStripExtraGeoID.Checked) ? "true" : string.Empty;




        }


        /// <summary>
        /// Sanity check to determine if we have enough information to start a job / export --
        /// 
        /// This function should display a MessageBox if something is really wrong
        /// </summary>
        /// <param name="isFishnet"></param>
        protected bool CheckValidity(bool isFishnet)
        {
            /** TODO: Gather all inputs and update our controller / job instance */

            var importObj = FormController.Instance.JobInstance;

            if (string.IsNullOrEmpty(importObj.Year))
            {
                MessageBox.Show("No year selected", "Required setting missing", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            if (importObj.State == AcsState.None)
            {
                MessageBox.Show("No state selected", "Required setting missing", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            if (string.IsNullOrEmpty(importObj.SummaryLevel))
            {
                MessageBox.Show("No summary level selected", "Required setting missing", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            if (string.IsNullOrEmpty(importObj.IncludedVariableFile))
            {
                MessageBox.Show("No variables file selected", "Required setting missing", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }

            //TODO: prompt them to save it somewhere?
            //if (string.IsNullOrEmpty(importObj.OutputFolder))
            //{
            //    MessageBox.Show("Required setting missing", "No Year Provided", MessageBoxButtons.OK, MessageBoxIcon.Question);
            //    return false;
            //}


            return true;

        }






        private void btnShapefile_Click(object sender, EventArgs e)
        {
            GatherInputs(false);
            if (!CheckValidity(false))
            {
                return;
            }

            _log.Debug("Ready to go!");
            HideLoadingSpinner();
            this.backgroundWorker1.RunWorkerAsync(FormController.Instance.JobInstance);

            SmartToggler();
        }

        private void btnFishnet_Click(object sender, EventArgs e)
        {
            GatherInputs(true);
            if (!CheckValidity(true))
            {
                return;
            }

            _log.Debug("Ready to go!");
            HideLoadingSpinner();
            this.backgroundWorker1.RunWorkerAsync(FormController.Instance.JobInstance);

            SmartToggler();
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            ImportJob job = (ImportJob)e.Argument;

            job.OnProgressUpdated += new ImportJob.ProgressUpdateHandler(this.backgroundWorker1.ReportProgress);

            e.Result = job.ExecuteJob();



            //TODO: add support for cancellation?
            //TODO: add progress reporting?


        }


        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.pgbStatus.Value = e.ProgressPercentage;
            //this.pgbStatus
            //string message = (string)e.UserState;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!(bool)e.Result)
            {
                ShowLoadingSpinner();
            }

            SmartToggler();
        }


        #region Control Validation


        private void txtVariableFilePath_Validating(object sender, CancelEventArgs e)
        {
            //validate variables file
            string errMessage = string.Empty;
            bool valid = FormController.Instance.ValidateVariablesFile(txtVariableFilePath.Text, out errMessage);

            this.errorProvider1.SetError(this.txtVariableFilePath, errMessage);
        }

        private void cboProjections_Validating(object sender, CancelEventArgs e)
        {
            bool listSelected = radioSRIDFromList.Checked;
            bool valueInList = cboProjections.SelectedIndex != -1;
            string errorMessage = string.Empty;


            //if our radio button is selected.
            //if the value is not in the list
            if (listSelected && !valueInList)
            {
                errorMessage = "Unknown SRID";
            }

            this.errorProvider1.SetError(cboProjections, errorMessage);
        }


        private void txtPrjFilePath_Validating(object sender, CancelEventArgs e)
        {
            //if our radio button is selected.
            //if the file doesn't have a valid projection
            bool listSelected = radioSRIDFile.Checked;
            bool validProjectionFile = false;        //TODO
            string errorMessage = string.Empty;

            if (listSelected && !validProjectionFile)
            {
                errorMessage = "Invalid Projection File";
            }

            this.errorProvider1.SetError(txtPrjFilePath, errorMessage);
        }


        private void txtFishnetCellSize_Validating(object sender, CancelEventArgs e)
        {
            //check to see if it parses nicely
            string errorMessage = string.Empty;

            string cellSize = txtFishnetCellSize.Text.Trim();
            if (!string.IsNullOrEmpty(cellSize))
            {
                var chunks = cellSize.Split("x_:, ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                //1 param, square
                if ((chunks.Length == 1)
                    && ((Utilities.GetAs<double>(chunks[0], -1)) == -1))
                {
                    errorMessage = "Invalid size";
                }
                //2 params, rectangle
                else if ((chunks.Length == 2)
                    && (((Utilities.GetAs<double>(chunks[0], -1)) == -1)
                        || ((Utilities.GetAs<double>(chunks[1], -1)) == -1)))
                {
                    errorMessage = "Invalid width/height";
                }
            }

            this.errorProvider1.SetError(txtFishnetCellSize, errorMessage);
        }

        #endregion Control Validation




    }
}

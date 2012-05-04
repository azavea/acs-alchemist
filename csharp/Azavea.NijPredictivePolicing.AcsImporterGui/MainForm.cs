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

namespace Azavea.NijPredictivePolicing.AcsAlchemistGui
{
    public partial class MainForm : Form
    {
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

                //TODO: special initializer for the logger / or something to get the output stream before the copyright / etc is shown

                //wait until after the form is loaded to do this, we want to show the log output on the form
                FormController.Instance.Initialize();

                //
                // Initialize the rest of the form
                //

                this.PopulateLists();
            }
            catch (Exception ex)
            {
                this.DisplayException("Form Load", ex);
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

        private void txtVariableFilePath_Validating(object sender, CancelEventArgs e)
        {
            //validate variables file
            bool isValid = FormController.Instance.ValidateVariablesFile(txtVariableFilePath.Text);
            e.Cancel = (isValid == false);      //set e.Cancel to true when the file is NOT valid

            if (e.Cancel)
            {
                //this.errorProvider1.SetError(textBox1, errorMsg);     //TODO: error provider?
            }
        }

        


    }
}

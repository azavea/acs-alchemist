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

        private void btnBrowseVariableFile_Click(object sender, EventArgs e)
        {
            openFileVariableFile.ShowDialog();
            txtVariableFilePath.Text = openFileVariableFile.FileName;
        }

        private void btnBrowseOutputFolder_Click(object sender, EventArgs e)
        {
            folderBrowserOutputDir.ShowDialog();
            txtOutputDirectoryPath.Text = folderBrowserOutputDir.SelectedPath;
        }

        private void btnBrowseFishnetEnvelopeFile_Click(object sender, EventArgs e)
        {
            openFileFishnetEnvelopeShp.ShowDialog();
            txtFishnetEnvelopeFilePath.Text = openFileFishnetEnvelopeShp.FileName;
        }

        private void btnBrowsePrjFile_Click(object sender, EventArgs e)
        {
            openFilePrjFile.ShowDialog();
            txtPrjFilePath.Text = openFilePrjFile.FileName;
        }

        private void btnBrowseBoundaryShpFile_Click(object sender, EventArgs e)
        {
            openFileBoundaryShp.ShowDialog();
            txtBoundaryShpFilePath.Text = openFileBoundaryShp.FileName;
        }

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


    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Azavea.NijPredictivePolicing.AcsAlchemistGui
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
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


    }
}

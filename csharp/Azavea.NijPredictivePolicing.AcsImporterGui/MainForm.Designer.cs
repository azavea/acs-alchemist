namespace Azavea.NijPredictivePolicing.AcsAlchemistGui
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtFishnetEnvelopeFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowseFishnetEnvelopeFile = new System.Windows.Forms.Button();
            this.chkStripExtraGeoID = new System.Windows.Forms.CheckBox();
            this.chkPreserveJamValues = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtFishnetCellSize = new System.Windows.Forms.TextBox();
            this.chkExportFishnet = new System.Windows.Forms.CheckBox();
            this.chkReplaceJob = new System.Windows.Forms.CheckBox();
            this.txtJobName = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtBoundaryShpFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowseBoundaryShpFile = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtPrjFilePath = new System.Windows.Forms.TextBox();
            this.btnBrowsePrjFile = new System.Windows.Forms.Button();
            this.chkExportShapefile = new System.Windows.Forms.CheckBox();
            this.txtJobFilePath = new System.Windows.Forms.TextBox();
            this.btnSaveMessageLog = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.cboACSYear = new System.Windows.Forms.ComboBox();
            this.cboState = new System.Windows.Forms.ComboBox();
            this.cboSummaryLevel = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.folderBrowserOutputDir = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileVariableFile = new System.Windows.Forms.OpenFileDialog();
            this.btnBrowseVariableFile = new System.Windows.Forms.Button();
            this.btnBrowseOutputFolder = new System.Windows.Forms.Button();
            this.txtVariableFilePath = new System.Windows.Forms.TextBox();
            this.txtOutputDirectoryPath = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openJobFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveJobFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileJob = new System.Windows.Forms.SaveFileDialog();
            this.openFileBoundaryShp = new System.Windows.Forms.OpenFileDialog();
            this.openFileJob = new System.Windows.Forms.OpenFileDialog();
            this.txtMessageLogFilePath = new System.Windows.Forms.TextBox();
            this.pgbStatus = new System.Windows.Forms.ProgressBar();
            this.openFilePrjFile = new System.Windows.Forms.OpenFileDialog();
            this.openFileFishnetEnvelopeShp = new System.Windows.Forms.OpenFileDialog();
            this.saveFileMessageLog = new System.Windows.Forms.SaveFileDialog();
            this.label15 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.textBox1.Enabled = false;
            this.textBox1.Location = new System.Drawing.Point(509, 68);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(381, 506);
            this.textBox1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(506, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Log Messages";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 99);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "1. ACS Year";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 124);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "2. State";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 151);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "3. Summary Level";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 185);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "4. Variable File";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.txtFishnetEnvelopeFilePath);
            this.groupBox1.Controls.Add(this.btnBrowseFishnetEnvelopeFile);
            this.groupBox1.Controls.Add(this.chkStripExtraGeoID);
            this.groupBox1.Controls.Add(this.chkPreserveJamValues);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.txtFishnetCellSize);
            this.groupBox1.Controls.Add(this.chkExportFishnet);
            this.groupBox1.Controls.Add(this.chkReplaceJob);
            this.groupBox1.Controls.Add(this.txtJobName);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.txtBoundaryShpFilePath);
            this.groupBox1.Controls.Add(this.btnBrowseBoundaryShpFile);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.textBox3);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.txtPrjFilePath);
            this.groupBox1.Controls.Add(this.btnBrowsePrjFile);
            this.groupBox1.Controls.Add(this.chkExportShapefile);
            this.groupBox1.Location = new System.Drawing.Point(20, 247);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(433, 312);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Optional Parameters";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(76, 157);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(89, 13);
            this.label14.TabIndex = 36;
            this.label14.Text = "Fishnet Envelope";
            // 
            // txtFishnetEnvelopeFilePath
            // 
            this.txtFishnetEnvelopeFilePath.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtFishnetEnvelopeFilePath.Location = new System.Drawing.Point(169, 154);
            this.txtFishnetEnvelopeFilePath.Name = "txtFishnetEnvelopeFilePath";
            this.txtFishnetEnvelopeFilePath.Size = new System.Drawing.Size(190, 20);
            this.txtFishnetEnvelopeFilePath.TabIndex = 35;
            // 
            // btnBrowseFishnetEnvelopeFile
            // 
            this.btnBrowseFishnetEnvelopeFile.Location = new System.Drawing.Point(367, 152);
            this.btnBrowseFishnetEnvelopeFile.Name = "btnBrowseFishnetEnvelopeFile";
            this.btnBrowseFishnetEnvelopeFile.Size = new System.Drawing.Size(56, 23);
            this.btnBrowseFishnetEnvelopeFile.TabIndex = 34;
            this.btnBrowseFishnetEnvelopeFile.Text = "Browse";
            this.btnBrowseFishnetEnvelopeFile.UseVisualStyleBackColor = true;
            this.btnBrowseFishnetEnvelopeFile.Click += new System.EventHandler(this.btnBrowseFishnetEnvelopeFile_Click);
            // 
            // chkStripExtraGeoID
            // 
            this.chkStripExtraGeoID.AutoSize = true;
            this.chkStripExtraGeoID.Checked = true;
            this.chkStripExtraGeoID.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStripExtraGeoID.Location = new System.Drawing.Point(191, 28);
            this.chkStripExtraGeoID.Name = "chkStripExtraGeoID";
            this.chkStripExtraGeoID.Size = new System.Drawing.Size(131, 17);
            this.chkStripExtraGeoID.TabIndex = 33;
            this.chkStripExtraGeoID.Text = "Strip Extra GEOID text";
            this.chkStripExtraGeoID.UseVisualStyleBackColor = true;
            // 
            // chkPreserveJamValues
            // 
            this.chkPreserveJamValues.AutoSize = true;
            this.chkPreserveJamValues.Location = new System.Drawing.Point(16, 28);
            this.chkPreserveJamValues.Name = "chkPreserveJamValues";
            this.chkPreserveJamValues.Size = new System.Drawing.Size(149, 17);
            this.chkPreserveJamValues.TabIndex = 32;
            this.chkPreserveJamValues.Text = "Preserve ACS Jam Values";
            this.chkPreserveJamValues.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Enabled = false;
            this.label13.Location = new System.Drawing.Point(278, 132);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(84, 13);
            this.label13.TabIndex = 31;
            this.label13.Text = "Fishnet Cell Size";
            // 
            // txtFishnetCellSize
            // 
            this.txtFishnetCellSize.BackColor = System.Drawing.Color.White;
            this.txtFishnetCellSize.Enabled = false;
            this.txtFishnetCellSize.Location = new System.Drawing.Point(224, 129);
            this.txtFishnetCellSize.Name = "txtFishnetCellSize";
            this.txtFishnetCellSize.Size = new System.Drawing.Size(52, 20);
            this.txtFishnetCellSize.TabIndex = 30;
            // 
            // chkExportFishnet
            // 
            this.chkExportFishnet.Location = new System.Drawing.Point(132, 131);
            this.chkExportFishnet.Name = "chkExportFishnet";
            this.chkExportFishnet.Size = new System.Drawing.Size(103, 17);
            this.chkExportFishnet.TabIndex = 29;
            this.chkExportFishnet.Text = "Export Fishnet Shapefile";
            this.chkExportFishnet.UseVisualStyleBackColor = true;
            // 
            // chkReplaceJob
            // 
            this.chkReplaceJob.AutoSize = true;
            this.chkReplaceJob.Checked = true;
            this.chkReplaceJob.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkReplaceJob.Location = new System.Drawing.Point(265, 62);
            this.chkReplaceJob.Name = "chkReplaceJob";
            this.chkReplaceJob.Size = new System.Drawing.Size(86, 17);
            this.chkReplaceJob.TabIndex = 28;
            this.chkReplaceJob.Text = "Replace Job";
            this.chkReplaceJob.UseVisualStyleBackColor = true;
            // 
            // txtJobName
            // 
            this.txtJobName.BackColor = System.Drawing.Color.White;
            this.txtJobName.Location = new System.Drawing.Point(74, 60);
            this.txtJobName.Name = "txtJobName";
            this.txtJobName.Size = new System.Drawing.Size(182, 20);
            this.txtJobName.TabIndex = 27;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(13, 63);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(55, 13);
            this.label12.TabIndex = 26;
            this.label12.Text = "Job Name";
            // 
            // txtBoundaryShpFilePath
            // 
            this.txtBoundaryShpFilePath.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtBoundaryShpFilePath.Location = new System.Drawing.Point(97, 276);
            this.txtBoundaryShpFilePath.Name = "txtBoundaryShpFilePath";
            this.txtBoundaryShpFilePath.Size = new System.Drawing.Size(262, 20);
            this.txtBoundaryShpFilePath.TabIndex = 25;
            // 
            // btnBrowseBoundaryShpFile
            // 
            this.btnBrowseBoundaryShpFile.Location = new System.Drawing.Point(366, 274);
            this.btnBrowseBoundaryShpFile.Name = "btnBrowseBoundaryShpFile";
            this.btnBrowseBoundaryShpFile.Size = new System.Drawing.Size(56, 23);
            this.btnBrowseBoundaryShpFile.TabIndex = 24;
            this.btnBrowseBoundaryShpFile.Text = "Browse";
            this.btnBrowseBoundaryShpFile.UseVisualStyleBackColor = true;
            this.btnBrowseBoundaryShpFile.Click += new System.EventHandler(this.btnBrowseBoundaryShpFile_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 279);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(77, 13);
            this.label11.TabIndex = 23;
            this.label11.Text = "Boundary Filter";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(13, 189);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(68, 13);
            this.label9.TabIndex = 19;
            this.label9.Text = "Output SRID";
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.Color.White;
            this.textBox3.Location = new System.Drawing.Point(81, 186);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(52, 20);
            this.textBox3.TabIndex = 18;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(151, 189);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(72, 13);
            this.label8.TabIndex = 17;
            this.label8.Text = "Prj File (WKT)";
            // 
            // txtPrjFilePath
            // 
            this.txtPrjFilePath.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtPrjFilePath.Location = new System.Drawing.Point(224, 186);
            this.txtPrjFilePath.Name = "txtPrjFilePath";
            this.txtPrjFilePath.Size = new System.Drawing.Size(135, 20);
            this.txtPrjFilePath.TabIndex = 16;
            // 
            // btnBrowsePrjFile
            // 
            this.btnBrowsePrjFile.Location = new System.Drawing.Point(367, 184);
            this.btnBrowsePrjFile.Name = "btnBrowsePrjFile";
            this.btnBrowsePrjFile.Size = new System.Drawing.Size(56, 23);
            this.btnBrowsePrjFile.TabIndex = 15;
            this.btnBrowsePrjFile.Text = "Browse";
            this.btnBrowsePrjFile.UseVisualStyleBackColor = true;
            this.btnBrowsePrjFile.Click += new System.EventHandler(this.btnBrowsePrjFile_Click);
            // 
            // chkExportShapefile
            // 
            this.chkExportShapefile.AutoSize = true;
            this.chkExportShapefile.Checked = true;
            this.chkExportShapefile.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportShapefile.Location = new System.Drawing.Point(16, 131);
            this.chkExportShapefile.Name = "chkExportShapefile";
            this.chkExportShapefile.Size = new System.Drawing.Size(103, 17);
            this.chkExportShapefile.TabIndex = 0;
            this.chkExportShapefile.Text = "Export Shapefile";
            this.chkExportShapefile.UseVisualStyleBackColor = true;
            // 
            // txtJobFilePath
            // 
            this.txtJobFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtJobFilePath.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtJobFilePath.Location = new System.Drawing.Point(595, 19);
            this.txtJobFilePath.Name = "txtJobFilePath";
            this.txtJobFilePath.Size = new System.Drawing.Size(262, 20);
            this.txtJobFilePath.TabIndex = 22;
            // 
            // btnSaveMessageLog
            // 
            this.btnSaveMessageLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSaveMessageLog.Location = new System.Drawing.Point(799, 578);
            this.btnSaveMessageLog.Name = "btnSaveMessageLog";
            this.btnSaveMessageLog.Size = new System.Drawing.Size(58, 23);
            this.btnSaveMessageLog.TabIndex = 21;
            this.btnSaveMessageLog.Text = "Browse";
            this.btnSaveMessageLog.UseVisualStyleBackColor = true;
            this.btnSaveMessageLog.Click += new System.EventHandler(this.btnSaveMessageLog_Click);
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(505, 583);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(86, 13);
            this.label10.TabIndex = 20;
            this.label10.Text = "Save Log File as";
            // 
            // cboACSYear
            // 
            this.cboACSYear.FormattingEnabled = true;
            this.cboACSYear.Location = new System.Drawing.Point(122, 95);
            this.cboACSYear.Name = "cboACSYear";
            this.cboACSYear.Size = new System.Drawing.Size(89, 21);
            this.cboACSYear.TabIndex = 8;
            // 
            // cboState
            // 
            this.cboState.FormattingEnabled = true;
            this.cboState.Location = new System.Drawing.Point(122, 120);
            this.cboState.Name = "cboState";
            this.cboState.Size = new System.Drawing.Size(185, 21);
            this.cboState.TabIndex = 9;
            // 
            // cboSummaryLevel
            // 
            this.cboSummaryLevel.FormattingEnabled = true;
            this.cboSummaryLevel.Location = new System.Drawing.Point(122, 147);
            this.cboSummaryLevel.Name = "cboSummaryLevel";
            this.cboSummaryLevel.Size = new System.Drawing.Size(121, 21);
            this.cboSummaryLevel.TabIndex = 10;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 216);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(96, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "5. Output Directory";
            // 
            // openFileVariableFile
            // 
            this.openFileVariableFile.Filter = "Text files|*.txt";
            // 
            // btnBrowseVariableFile
            // 
            this.btnBrowseVariableFile.Location = new System.Drawing.Point(387, 180);
            this.btnBrowseVariableFile.Name = "btnBrowseVariableFile";
            this.btnBrowseVariableFile.Size = new System.Drawing.Size(56, 23);
            this.btnBrowseVariableFile.TabIndex = 12;
            this.btnBrowseVariableFile.Text = "Browse";
            this.btnBrowseVariableFile.UseVisualStyleBackColor = true;
            this.btnBrowseVariableFile.Click += new System.EventHandler(this.btnBrowseVariableFile_Click);
            // 
            // btnBrowseOutputFolder
            // 
            this.btnBrowseOutputFolder.Location = new System.Drawing.Point(387, 211);
            this.btnBrowseOutputFolder.Name = "btnBrowseOutputFolder";
            this.btnBrowseOutputFolder.Size = new System.Drawing.Size(56, 23);
            this.btnBrowseOutputFolder.TabIndex = 13;
            this.btnBrowseOutputFolder.Text = "Browse";
            this.btnBrowseOutputFolder.UseVisualStyleBackColor = true;
            this.btnBrowseOutputFolder.Click += new System.EventHandler(this.btnBrowseOutputFolder_Click);
            // 
            // txtVariableFilePath
            // 
            this.txtVariableFilePath.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtVariableFilePath.Location = new System.Drawing.Point(122, 181);
            this.txtVariableFilePath.Name = "txtVariableFilePath";
            this.txtVariableFilePath.Size = new System.Drawing.Size(259, 20);
            this.txtVariableFilePath.TabIndex = 14;
            // 
            // txtOutputDirectoryPath
            // 
            this.txtOutputDirectoryPath.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtOutputDirectoryPath.Location = new System.Drawing.Point(122, 212);
            this.txtOutputDirectoryPath.Name = "txtOutputDirectoryPath";
            this.txtOutputDirectoryPath.Size = new System.Drawing.Size(259, 20);
            this.txtOutputDirectoryPath.TabIndex = 15;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(893, 24);
            this.menuStrip1.TabIndex = 16;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openJobFileToolStripMenuItem,
            this.saveJobFileToolStripMenuItem,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openJobFileToolStripMenuItem
            // 
            this.openJobFileToolStripMenuItem.Name = "openJobFileToolStripMenuItem";
            this.openJobFileToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.openJobFileToolStripMenuItem.Text = "Open Job File";
            this.openJobFileToolStripMenuItem.Click += new System.EventHandler(this.openJobFileToolStripMenuItem_Click);
            // 
            // saveJobFileToolStripMenuItem
            // 
            this.saveJobFileToolStripMenuItem.Name = "saveJobFileToolStripMenuItem";
            this.saveJobFileToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.saveJobFileToolStripMenuItem.Text = "Save Job File";
            this.saveJobFileToolStripMenuItem.Click += new System.EventHandler(this.saveJobFileToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.quitToolStripMenuItem.Text = "&Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // saveFileJob
            // 
            this.saveFileJob.Filter = "Text files|*.txt|Job files|*.job";
            // 
            // openFileBoundaryShp
            // 
            this.openFileBoundaryShp.Filter = "Shapefile|*.shp";
            // 
            // openFileJob
            // 
            this.openFileJob.Filter = "Text files|*.txt|Job files|*.job";
            // 
            // txtMessageLogFilePath
            // 
            this.txtMessageLogFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtMessageLogFilePath.BackColor = System.Drawing.SystemColors.ControlLight;
            this.txtMessageLogFilePath.Location = new System.Drawing.Point(597, 580);
            this.txtMessageLogFilePath.Name = "txtMessageLogFilePath";
            this.txtMessageLogFilePath.Size = new System.Drawing.Size(198, 20);
            this.txtMessageLogFilePath.TabIndex = 23;
            // 
            // pgbStatus
            // 
            this.pgbStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pgbStatus.Location = new System.Drawing.Point(0, 610);
            this.pgbStatus.Name = "pgbStatus";
            this.pgbStatus.Size = new System.Drawing.Size(893, 22);
            this.pgbStatus.TabIndex = 24;
            this.pgbStatus.Visible = false;
            // 
            // openFilePrjFile
            // 
            this.openFilePrjFile.Filter = "PRJ file|*.prj";
            // 
            // openFileFishnetEnvelopeShp
            // 
            this.openFileFishnetEnvelopeShp.Filter = "Shapefile|*.shp";
            // 
            // saveFileMessageLog
            // 
            this.saveFileMessageLog.Filter = "Text files|*.txt|Log files|*.log";
            // 
            // label15
            // 
            this.label15.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label15.AutoSize = true;
            this.label15.BackColor = System.Drawing.SystemColors.Window;
            this.label15.Location = new System.Drawing.Point(506, 22);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(83, 13);
            this.label15.TabIndex = 25;
            this.label15.Text = "Current Job File:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(199, 84);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 26;
            this.pictureBox1.TabStop = false;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.pgbStatus);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.txtOutputDirectoryPath);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.label15);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.txtVariableFilePath);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.txtJobFilePath);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnBrowseOutputFolder);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.txtMessageLogFilePath);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnBrowseVariableFile);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.textBox1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.label7);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.btnSaveMessageLog);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.cboSummaryLevel);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.label10);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.cboState);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.label2);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.cboACSYear);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.pictureBox1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.groupBox1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.label3);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.label6);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.label4);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.label5);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(893, 632);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(893, 656);
            this.toolStripContainer1.TabIndex = 27;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.menuStrip1);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(893, 656);
            this.Controls.Add(this.toolStripContainer1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "ACS Data Ermine";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.PerformLayout();
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkExportShapefile;
        private System.Windows.Forms.ComboBox cboACSYear;
        private System.Windows.Forms.ComboBox cboState;
        private System.Windows.Forms.ComboBox cboSummaryLevel;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserOutputDir;
        private System.Windows.Forms.OpenFileDialog openFileVariableFile;
        private System.Windows.Forms.Button btnBrowseVariableFile;
        private System.Windows.Forms.Button btnBrowseOutputFolder;
        private System.Windows.Forms.TextBox txtVariableFilePath;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtPrjFilePath;
        private System.Windows.Forms.Button btnBrowsePrjFile;
        private System.Windows.Forms.TextBox txtOutputDirectoryPath;
        private System.Windows.Forms.TextBox txtJobFilePath;
        private System.Windows.Forms.Button btnSaveMessageLog;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openJobFileToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileJob;
        private System.Windows.Forms.TextBox txtBoundaryShpFilePath;
        private System.Windows.Forms.Button btnBrowseBoundaryShpFile;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtJobName;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.OpenFileDialog openFileBoundaryShp;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtFishnetCellSize;
        private System.Windows.Forms.CheckBox chkExportFishnet;
        private System.Windows.Forms.CheckBox chkReplaceJob;
        private System.Windows.Forms.CheckBox chkPreserveJamValues;
        private System.Windows.Forms.OpenFileDialog openFileJob;
        private System.Windows.Forms.CheckBox chkStripExtraGeoID;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtFishnetEnvelopeFilePath;
        private System.Windows.Forms.Button btnBrowseFishnetEnvelopeFile;
        private System.Windows.Forms.ToolStripMenuItem saveJobFileToolStripMenuItem;
        private System.Windows.Forms.TextBox txtMessageLogFilePath;
        private System.Windows.Forms.ProgressBar pgbStatus;
        private System.Windows.Forms.OpenFileDialog openFilePrjFile;
        private System.Windows.Forms.OpenFileDialog openFileFishnetEnvelopeShp;
        private System.Windows.Forms.SaveFileDialog saveFileMessageLog;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
    }
}


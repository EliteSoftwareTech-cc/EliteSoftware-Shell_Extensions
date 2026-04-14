using System.Windows.Forms;
using System.Drawing;

namespace WimMergeApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private TabControl tabControl;
        private TabPage tabIsos;
        private TabPage tabDrivers;
        private TabPage tabCustomization;
        private TabPage tabBuild;

        private ListBox lstIsos;
        private Button btnAddIso;
        private Button btnRemoveIso;

        private TextBox txtDriverFolder;
        private Button btnBrowseDrivers;
        private Label lblDriverInfo;

        private TextBox txtBootMenuTitle;
        private Label lblBootMenuTitle;
        private TextBox txtEulaFile;
        private Button btnBrowseEula;
        private Label lblEulaFile;
        private TextBox txtWallpaperFile;
        private Button btnBrowseWallpaper;
        private Label lblWallpaperFile;
        private TextBox txtIconFile;
        private Button btnBrowseIcon;
        private Label lblIconFile;
        private CheckBox chkDisableSigEnforcement;

        private CheckBox chkUseUltraIso;
        private TextBox txtUltraIsoPath;
        private Button btnBrowseUltraIso;

        private TextBox txtOutputFile;
        private Button btnBrowseOutput;
        private Label lblOutputFile;
        private Button btnBuild;
        private TextBox txtLog;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabIsos = new System.Windows.Forms.TabPage();
            this.tabDrivers = new System.Windows.Forms.TabPage();
            this.tabCustomization = new System.Windows.Forms.TabPage();
            this.tabBuild = new System.Windows.Forms.TabPage();
            this.lstIsos = new System.Windows.Forms.ListBox();
            this.btnAddIso = new System.Windows.Forms.Button();
            this.btnRemoveIso = new System.Windows.Forms.Button();
            this.txtDriverFolder = new System.Windows.Forms.TextBox();
            this.btnBrowseDrivers = new System.Windows.Forms.Button();
            this.lblDriverInfo = new System.Windows.Forms.Label();
            this.txtBootMenuTitle = new System.Windows.Forms.TextBox();
            this.lblBootMenuTitle = new System.Windows.Forms.Label();
            this.txtEulaFile = new System.Windows.Forms.TextBox();
            this.btnBrowseEula = new System.Windows.Forms.Button();
            this.lblEulaFile = new System.Windows.Forms.Label();
            this.txtWallpaperFile = new System.Windows.Forms.TextBox();
            this.btnBrowseWallpaper = new System.Windows.Forms.Button();
            this.lblWallpaperFile = new System.Windows.Forms.Label();
            this.txtIconFile = new System.Windows.Forms.TextBox();
            this.btnBrowseIcon = new System.Windows.Forms.Button();
            this.lblIconFile = new System.Windows.Forms.Label();
            this.chkDisableSigEnforcement = new System.Windows.Forms.CheckBox();
            this.chkUseUltraIso = new System.Windows.Forms.CheckBox();
            this.txtUltraIsoPath = new System.Windows.Forms.TextBox();
            this.btnBrowseUltraIso = new System.Windows.Forms.Button();
            this.txtOutputFile = new System.Windows.Forms.TextBox();
            this.btnBrowseOutput = new System.Windows.Forms.Button();
            this.lblOutputFile = new System.Windows.Forms.Label();
            this.btnBuild = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();

            this.tabControl.SuspendLayout();
            this.tabIsos.SuspendLayout();
            this.tabDrivers.SuspendLayout();
            this.tabCustomization.SuspendLayout();
            this.tabBuild.SuspendLayout();
            this.SuspendLayout();

            // tabControl
            this.tabControl.Controls.Add(this.tabIsos);
            this.tabControl.Controls.Add(this.tabDrivers);
            this.tabControl.Controls.Add(this.tabCustomization);
            this.tabControl.Controls.Add(this.tabBuild);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(600, 400);

            // tabIsos
            this.tabIsos.Controls.Add(this.lstIsos);
            this.tabIsos.Controls.Add(this.btnAddIso);
            this.tabIsos.Controls.Add(this.btnRemoveIso);
            this.tabIsos.Location = new System.Drawing.Point(4, 24);
            this.tabIsos.Name = "tabIsos";
            this.tabIsos.Padding = new System.Windows.Forms.Padding(10);
            this.tabIsos.Size = new System.Drawing.Size(592, 372);
            this.tabIsos.Text = "ISO Inputs";
            this.tabIsos.UseVisualStyleBackColor = true;

            this.lstIsos.Dock = System.Windows.Forms.DockStyle.Top;
            this.lstIsos.ItemHeight = 15;
            this.lstIsos.Location = new System.Drawing.Point(10, 10);
            this.lstIsos.Name = "lstIsos";
            this.lstIsos.Size = new System.Drawing.Size(572, 200);

            this.btnAddIso.Location = new System.Drawing.Point(10, 220);
            this.btnAddIso.Name = "btnAddIso";
            this.btnAddIso.Size = new System.Drawing.Size(100, 30);
            this.btnAddIso.Text = "Add ISO(s)";
            this.btnAddIso.UseVisualStyleBackColor = true;
            this.btnAddIso.Click += new System.EventHandler(this.btnAddIso_Click);

            this.btnRemoveIso.Location = new System.Drawing.Point(120, 220);
            this.btnRemoveIso.Name = "btnRemoveIso";
            this.btnRemoveIso.Size = new System.Drawing.Size(100, 30);
            this.btnRemoveIso.Text = "Remove";
            this.btnRemoveIso.UseVisualStyleBackColor = true;
            this.btnRemoveIso.Click += new System.EventHandler(this.btnRemoveIso_Click);

            // tabDrivers
            this.tabDrivers.Controls.Add(this.lblDriverInfo);
            this.tabDrivers.Controls.Add(this.txtDriverFolder);
            this.tabDrivers.Controls.Add(this.btnBrowseDrivers);
            this.tabDrivers.Location = new System.Drawing.Point(4, 24);
            this.tabDrivers.Name = "tabDrivers";
            this.tabDrivers.Padding = new System.Windows.Forms.Padding(10);
            this.tabDrivers.Size = new System.Drawing.Size(592, 372);
            this.tabDrivers.Text = "Drivers";
            this.tabDrivers.UseVisualStyleBackColor = true;

            this.lblDriverInfo.AutoSize = true;
            this.lblDriverInfo.Location = new System.Drawing.Point(10, 10);
            this.lblDriverInfo.Name = "lblDriverInfo";
            this.lblDriverInfo.Size = new System.Drawing.Size(400, 15);
            this.lblDriverInfo.Text = "Select driver root folder. Drivers will be injected into all WIM indexes recursively.";

            this.txtDriverFolder.Location = new System.Drawing.Point(10, 35);
            this.txtDriverFolder.Name = "txtDriverFolder";
            this.txtDriverFolder.Size = new System.Drawing.Size(460, 23);

            this.btnBrowseDrivers.Location = new System.Drawing.Point(480, 34);
            this.btnBrowseDrivers.Name = "btnBrowseDrivers";
            this.btnBrowseDrivers.Size = new System.Drawing.Size(100, 25);
            this.btnBrowseDrivers.Text = "Browse...";
            this.btnBrowseDrivers.UseVisualStyleBackColor = true;
            this.btnBrowseDrivers.Click += new System.EventHandler(this.btnBrowseDrivers_Click);

            // tabCustomization
            this.tabCustomization.Controls.Add(this.lblBootMenuTitle);
            this.tabCustomization.Controls.Add(this.txtBootMenuTitle);
            this.tabCustomization.Controls.Add(this.lblEulaFile);
            this.tabCustomization.Controls.Add(this.txtEulaFile);
            this.tabCustomization.Controls.Add(this.btnBrowseEula);
            this.tabCustomization.Controls.Add(this.lblWallpaperFile);
            this.tabCustomization.Controls.Add(this.txtWallpaperFile);
            this.tabCustomization.Controls.Add(this.btnBrowseWallpaper);
            this.tabCustomization.Controls.Add(this.lblIconFile);
            this.tabCustomization.Controls.Add(this.txtIconFile);
            this.tabCustomization.Controls.Add(this.btnBrowseIcon);
            this.tabCustomization.Controls.Add(this.chkDisableSigEnforcement);
            this.tabCustomization.Location = new System.Drawing.Point(4, 24);
            this.tabCustomization.Name = "tabCustomization";
            this.tabCustomization.Padding = new System.Windows.Forms.Padding(10);
            this.tabCustomization.Size = new System.Drawing.Size(592, 372);
            this.tabCustomization.Text = "Customization";
            this.tabCustomization.UseVisualStyleBackColor = true;

            int currentY = 10;
            
            // Boot Menu Title
            this.lblBootMenuTitle.AutoSize = true;
            this.lblBootMenuTitle.Location = new System.Drawing.Point(10, currentY);
            this.lblBootMenuTitle.Text = "Boot Menu Brand/Title:";
            this.txtBootMenuTitle.Location = new System.Drawing.Point(150, currentY - 3);
            this.txtBootMenuTitle.Size = new System.Drawing.Size(320, 23);
            this.txtBootMenuTitle.Text = "Custom OS Setup";
            currentY += 40;

            // EULA
            this.lblEulaFile.AutoSize = true;
            this.lblEulaFile.Location = new System.Drawing.Point(10, currentY);
            this.lblEulaFile.Text = "Custom EULA (.rtf):";
            this.txtEulaFile.Location = new System.Drawing.Point(150, currentY - 3);
            this.txtEulaFile.Size = new System.Drawing.Size(320, 23);
            this.btnBrowseEula.Location = new System.Drawing.Point(480, currentY - 4);
            this.btnBrowseEula.Size = new System.Drawing.Size(100, 25);
            this.btnBrowseEula.Text = "Browse...";
            this.btnBrowseEula.UseVisualStyleBackColor = true;
            this.btnBrowseEula.Click += new System.EventHandler(this.btnBrowseEula_Click);
            currentY += 40;

            // Wallpaper
            this.lblWallpaperFile.AutoSize = true;
            this.lblWallpaperFile.Location = new System.Drawing.Point(10, currentY);
            this.lblWallpaperFile.Text = "Setup Wallpaper (.bmp):";
            this.txtWallpaperFile.Location = new System.Drawing.Point(150, currentY - 3);
            this.txtWallpaperFile.Size = new System.Drawing.Size(320, 23);
            this.btnBrowseWallpaper.Location = new System.Drawing.Point(480, currentY - 4);
            this.btnBrowseWallpaper.Size = new System.Drawing.Size(100, 25);
            this.btnBrowseWallpaper.Text = "Browse...";
            this.btnBrowseWallpaper.UseVisualStyleBackColor = true;
            this.btnBrowseWallpaper.Click += new System.EventHandler(this.btnBrowseWallpaper_Click);
            currentY += 40;

            // Icon
            this.lblIconFile.AutoSize = true;
            this.lblIconFile.Location = new System.Drawing.Point(10, currentY);
            this.lblIconFile.Text = "Custom Drive Icon (.ico):";
            this.txtIconFile.Location = new System.Drawing.Point(150, currentY - 3);
            this.txtIconFile.Size = new System.Drawing.Size(320, 23);
            this.btnBrowseIcon.Location = new System.Drawing.Point(480, currentY - 4);
            this.btnBrowseIcon.Size = new System.Drawing.Size(100, 25);
            this.btnBrowseIcon.Text = "Browse...";
            this.btnBrowseIcon.UseVisualStyleBackColor = true;
            this.btnBrowseIcon.Click += new System.EventHandler(this.btnBrowseIcon_Click);
            currentY += 40;

            // Sig Enforcement
            this.chkDisableSigEnforcement.AutoSize = true;
            this.chkDisableSigEnforcement.Location = new System.Drawing.Point(10, currentY);
            this.chkDisableSigEnforcement.Text = "Disable Driver Signature Enforcement (Test Mode)";
            this.chkDisableSigEnforcement.UseVisualStyleBackColor = true;
            this.chkDisableSigEnforcement.Checked = true;

            // tabBuild
            this.tabBuild.Controls.Add(this.chkUseUltraIso);
            this.tabBuild.Controls.Add(this.txtUltraIsoPath);
            this.tabBuild.Controls.Add(this.btnBrowseUltraIso);
            this.tabBuild.Controls.Add(this.lblOutputFile);
            this.tabBuild.Controls.Add(this.txtOutputFile);
            this.tabBuild.Controls.Add(this.btnBrowseOutput);
            this.tabBuild.Controls.Add(this.btnBuild);
            this.tabBuild.Controls.Add(this.txtLog);
            this.tabBuild.Location = new System.Drawing.Point(4, 24);
            this.tabBuild.Name = "tabBuild";
            this.tabBuild.Padding = new System.Windows.Forms.Padding(10);
            this.tabBuild.Size = new System.Drawing.Size(592, 372);
            this.tabBuild.Text = "Build";
            this.tabBuild.UseVisualStyleBackColor = true;

            this.lblOutputFile.AutoSize = true;
            this.lblOutputFile.Location = new System.Drawing.Point(10, 10);
            this.lblOutputFile.Text = "Output ISO File:";
            this.txtOutputFile.Location = new System.Drawing.Point(100, 7);
            this.txtOutputFile.Size = new System.Drawing.Size(370, 23);
            this.btnBrowseOutput.Location = new System.Drawing.Point(480, 6);
            this.btnBrowseOutput.Size = new System.Drawing.Size(100, 25);
            this.btnBrowseOutput.Text = "Browse...";
            this.btnBrowseOutput.UseVisualStyleBackColor = true;
            this.btnBrowseOutput.Click += new System.EventHandler(this.btnBrowseOutput_Click);

            this.chkUseUltraIso.AutoSize = true;
            this.chkUseUltraIso.Location = new System.Drawing.Point(10, 40);
            this.chkUseUltraIso.Text = "Use UltraISO for extraction/creation";
            this.chkUseUltraIso.UseVisualStyleBackColor = true;
            this.chkUseUltraIso.CheckedChanged += new System.EventHandler(this.chkUseUltraIso_CheckedChanged);

            this.txtUltraIsoPath.Location = new System.Drawing.Point(220, 38);
            this.txtUltraIsoPath.Size = new System.Drawing.Size(250, 23);
            this.txtUltraIsoPath.Enabled = false;
            this.btnBrowseUltraIso.Location = new System.Drawing.Point(480, 37);
            this.btnBrowseUltraIso.Size = new System.Drawing.Size(100, 25);
            this.btnBrowseUltraIso.Text = "Browse...";
            this.btnBrowseUltraIso.UseVisualStyleBackColor = true;
            this.btnBrowseUltraIso.Enabled = false;
            this.btnBrowseUltraIso.Click += new System.EventHandler(this.btnBrowseUltraIso_Click);

            this.btnBuild.Location = new System.Drawing.Point(10, 70);
            this.btnBuild.Size = new System.Drawing.Size(120, 30);
            this.btnBuild.Text = "Start Build";
            this.btnBuild.UseVisualStyleBackColor = true;
            this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);

            this.txtLog.Location = new System.Drawing.Point(10, 110);
            this.txtLog.Multiline = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(570, 250);
            this.txtLog.ReadOnly = true;

            // MainForm
            this.ClientSize = new System.Drawing.Size(600, 400);
            this.Controls.Add(this.tabControl);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "MainForm";
            this.Text = "WIM & Installer Manager";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            this.tabControl.ResumeLayout(false);
            this.tabIsos.ResumeLayout(false);
            this.tabDrivers.ResumeLayout(false);
            this.tabDrivers.PerformLayout();
            this.tabCustomization.ResumeLayout(false);
            this.tabCustomization.PerformLayout();
            this.tabBuild.ResumeLayout(false);
            this.tabBuild.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
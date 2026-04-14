using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WimMergeApp
{
    public partial class MainForm : Form, WimMergeEngine.ILogger
    {
        private List<string> _isoFiles = new List<string>();

        public MainForm()
        {
            InitializeComponent();
            CheckPrerequisites();
            LoadEmbeddedIcon();
        }

        private void LoadEmbeddedIcon()
        {
            try
            {
                var assembly = typeof(WimMergeEngine.ProjectBuilder).Assembly;
                using (var stream = assembly.GetManifestResourceStream("WIM_MERGE_ENGINE.app.ico"))
                {
                    if (stream != null)
                    {
                        this.Icon = new Icon(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Warning: Could not load embedded icon: {ex.Message}");
            }
        }

        public void Log(string message)
        {
            LogMessage(message);
        }

        private void CheckPrerequisites()
        {
            // Create directories if they don't exist
            Directory.CreateDirectory("ISO_INPUT");
            Directory.CreateDirectory("DRIVERS");
            Directory.CreateDirectory("Tools");
            
            // Log message
            LogMessage("Application initialized. Visual Styles enabled.");
            LogMessage("Please ensure 7z.exe, 7z.dll, and oscdimg.exe are in the 'Tools' folder.");

            // Auto-detect UltraISO
            string[] possibleUltraIsoPaths = {
                @"C:\Program Files (x86)\UltraISO\UltraISO.exe",
                @"C:\Program Files\UltraISO\UltraISO.exe"
            };

            foreach (var path in possibleUltraIsoPaths)
            {
                if (File.Exists(path))
                {
                    txtUltraIsoPath.Text = path;
                    break;
                }
            }
        }

        public void LogMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(LogMessage), message);
                return;
            }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }

        private void btnAddIso_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "ISO Files (*.iso)|*.iso|All Files (*.*)|*.*";
                ofd.Multiselect = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in ofd.FileNames)
                    {
                        if (!_isoFiles.Contains(file))
                        {
                            _isoFiles.Add(file);
                            lstIsos.Items.Add(Path.GetFileName(file));
                        }
                    }
                }
            }
        }

        private void btnRemoveIso_Click(object sender, EventArgs e)
        {
            if (lstIsos.SelectedIndex >= 0)
            {
                int index = lstIsos.SelectedIndex;
                _isoFiles.RemoveAt(index);
                lstIsos.Items.RemoveAt(index);
            }
        }

        private void btnBrowseDrivers_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select Drivers Folder";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtDriverFolder.Text = fbd.SelectedPath;
                }
            }
        }

        private void btnBrowseEula_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "RTF Files (*.rtf)|*.rtf";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtEulaFile.Text = ofd.FileName;
                }
            }
        }

        private void btnBrowseWallpaper_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files (*.bmp;*.jpg)|*.bmp;*.jpg";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtWallpaperFile.Text = ofd.FileName;
                }
            }
        }

        private void btnBrowseIcon_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Icon Files (*.ico)|*.ico";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtIconFile.Text = ofd.FileName;
                }
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "ISO Files (*.iso)|*.iso";
                sfd.FileName = "CustomWindows.iso";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    txtOutputFile.Text = sfd.FileName;
                }
            }
        }

        private void btnBrowseUltraIso_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Executable Files (*.exe)|*.exe";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtUltraIsoPath.Text = ofd.FileName;
                }
            }
        }

        private void chkUseUltraIso_CheckedChanged(object sender, EventArgs e)
        {
            txtUltraIsoPath.Enabled = chkUseUltraIso.Checked;
            btnBrowseUltraIso.Enabled = chkUseUltraIso.Checked;
        }

        private async void btnBuild_Click(object sender, EventArgs e)
        {
            if (_isoFiles.Count < 2)
            {
                MessageBox.Show("Please add at least 2 ISO files to merge.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtOutputFile.Text))
            {
                MessageBox.Show("Please specify an output ISO file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (chkUseUltraIso.Checked && !File.Exists(txtUltraIsoPath.Text))
            {
                MessageBox.Show("UltraISO is enabled but the specified executable path is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string driverFolder = string.IsNullOrWhiteSpace(txtDriverFolder.Text) ? Path.GetFullPath("DRIVERS") : txtDriverFolder.Text;
            
            btnBuild.Enabled = false;
            LogMessage("Starting Build Process...");

            try
            {
                await Task.Run(() => 
                {
                    var builder = new WimMergeEngine.ProjectBuilder(this);
                    builder.Build(
                        _isoFiles,
                        txtOutputFile.Text,
                        driverFolder,
                        txtBootMenuTitle.Text,
                        txtEulaFile.Text,
                        txtWallpaperFile.Text,
                        txtIconFile.Text,
                        chkDisableSigEnforcement.Checked,
                        chkUseUltraIso.Checked,
                        txtUltraIsoPath.Text
                    );
                });
                
                LogMessage("Build completed successfully!");
                MessageBox.Show("Build completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR: {ex.Message}");
                MessageBox.Show($"Build failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnBuild.Enabled = true;
            }
        }
    }
}

#pragma warning disable CS8618, CS8600, CS8602, CS8604, CS8622, CS8625
using SharpShell.Attributes;
using SharpShell.SharpPropertySheet;
using SharpShell.SharpContextMenu;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using MetadataEditor.Engine;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;

namespace MetadataEditor.ShellExtension
{
    [ComVisible(true)]
    [Guid("6F3A1B2C-4D5E-6F7A-8B9C-0D1E2F3A4B5C")]
    [RegistrationName("Metadata Editor")]
    [SharpShell.Attributes.DisplayName("Metadata Editor Extension")]
    [COMServerAssociation(AssociationType.AllFiles)]
    [COMServerAssociation(AssociationType.Class, @"Directory")]
    [COMServerAssociation(AssociationType.Class, @"Directory\Background")]
    [COMServerAssociation(AssociationType.Class, @"Drive")]
    [COMServerAssociation(AssociationType.FileExtension, ".iso")]
    [COMServerAssociation(AssociationType.FileExtension, ".img")]
    [COMServerAssociation(AssociationType.FileExtension, ".bin")]
    [COMServerAssociation(AssociationType.FileExtension, ".cdr")]
    [COMServerAssociation(AssociationType.FileExtension, ".vhd")]
    [COMServerAssociation(AssociationType.FileExtension, ".vhdx")]
    public class MetadataPropertySheetExtension : SharpPropertySheet
    {
        protected override bool CanShowSheet()
        {
            return SelectedItemPaths.Count() == 1;
        }

        protected override IEnumerable<SharpPropertyPage> CreatePages()
        {
            var pages = new List<SharpPropertyPage>();
            pages.Add(new MetadataPropertyPage());

            string path = SelectedItemPaths.First();
            string ext = Path.GetExtension(path).ToLower();
            string[] imageExts = { ".iso", ".img", ".bin", ".cdr", ".vhd", ".vhdx" };

            if (imageExts.Contains(ext))
            {
                pages.Add(new ImageManagementPropertyPage());
            }

            return pages;
        }
    }

    [ComVisible(true)]
    [Guid("9A8B7C6D-5E4F-3D2C-1B0A-9F8E7D6C5B4A")]
    [COMServerAssociation(AssociationType.AllFiles)]
    [COMServerAssociation(AssociationType.Class, "Directory")]
    [COMServerAssociation(AssociationType.FileExtension, ".iso")]
    [COMServerAssociation(AssociationType.FileExtension, ".img")]
    [COMServerAssociation(AssociationType.FileExtension, ".bin")]
    [COMServerAssociation(AssociationType.FileExtension, ".cdr")]
    [COMServerAssociation(AssociationType.FileExtension, ".vhd")]
    [COMServerAssociation(AssociationType.FileExtension, ".vhdx")]
    public class MetadataEditorContextMenu : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return SelectedItemPaths.Count() == 1;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();
            var item = new ToolStripMenuItem("Open in Metadata Editor");
            item.Click += (s, e) => {
                try
                {
                    string asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string appExe = Path.Combine(asmDir, "METADATA_EDITOR_APP.exe");
                    if (File.Exists(appExe)) Process.Start(appExe, $"{SelectedItemPaths.First()}");
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };
            menu.Items.Add(item);
            return menu;
        }
    }

    public class MetadataPropertyPage : SharpPropertyPage
    {
        private string targetPath = "";
        private TextBox pathBox;
        private PropertyGrid propertyGrid;
        private Button editAdsBtn;
        private Button addCustomAdsBtn;

        public MetadataPropertyPage()
        {
            this.Padding = new Padding(5);
            PageTitle = "Metadata";
            InitializeComponent();
            
            FileMetadataWrapper.SchemaUpdated += (s, e) => {
                try {
                    if (propertyGrid.SelectedObject is FileMetadataWrapper wrapper)
                    {
                        wrapper.RefreshValues();
                    }
                    if (this.InvokeRequired) this.Invoke(new MethodInvoker(() => {
                        TypeDescriptor.Refresh(propertyGrid.SelectedObject);
                        propertyGrid.Refresh();
                    }));
                    else {
                        TypeDescriptor.Refresh(propertyGrid.SelectedObject);
                        propertyGrid.Refresh();
                    }
                } catch {}
            };
        }

        protected override void OnPropertyPageInitialised(SharpPropertySheet parent)
        {
            if (parent.SelectedItemPaths.Any())
            {
                targetPath = parent.SelectedItemPaths.First();
                pathBox.Text = targetPath;
                LoadMetadataAsync();
            }
        }

        private void InitializeComponent()
        {
            Panel mainScrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(2) };

            Label pathLabel = new Label { Text = "Target:", Location = new Point(5, 10), AutoSize = true };
            pathBox = new TextBox 
            { 
                Location = new Point(50, 7), 
                Size = new Size(140, 20), 
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.None,
                BackColor = SystemColors.Control
            };

            propertyGrid = new PropertyGrid
            {
                Location = new Point(5, 30),
                Size = new Size(185, 300),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                PropertySort = PropertySort.Categorized,
                ToolbarVisible = true,
                
                // Rufus-Style Blue & White Styling
                CategoryForeColor = Color.White,
                CategorySplitterColor = Color.MediumBlue,
                LineColor = Color.MediumBlue,
                HelpBackColor = Color.White,
                HelpForeColor = Color.Black,
                CommandsBackColor = Color.White,
                CommandsForeColor = Color.Black,
                ViewBackColor = Color.White,
                ViewForeColor = Color.Black
            };

            editAdsBtn = new Button
            {
                Text = "Manage ADS",
                Location = new Point(5, 335),
                Size = new Size(90, 30)
            };
            editAdsBtn.Click += (s, e) => {
                try {
                    string asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string appExe = Path.Combine(asmDir, "METADATA_EDITOR_APP.exe");
                    if (File.Exists(appExe)) Process.Start(appExe, $"\"{targetPath}\" ");
                } catch {}
            };

            addCustomAdsBtn = new Button
            {
                Text = "Add Field",
                Location = new Point(100, 335),
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            addCustomAdsBtn.Click += (s, e) => {
                if (string.IsNullOrEmpty(targetPath)) return;
                string name = Microsoft.VisualBasic.Interaction.InputBox("Enter name for custom ADS field:", "Custom ADS", "NewStream");
                if (!string.IsNullOrWhiteSpace(name))
                {
                    try {
                        AdsEngine.WriteStream(targetPath, name, "");
                        LoadMetadataAsync();
                    } catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            };

            mainScrollPanel.Controls.AddRange(new Control[] { pathLabel, pathBox, propertyGrid, editAdsBtn, addCustomAdsBtn });
            this.Controls.Add(mainScrollPanel);
        }

        private async void LoadMetadataAsync()
        {
            if (File.Exists(targetPath) || Directory.Exists(targetPath))
            {
                try
                {
                    var wrapper = await System.Threading.Tasks.Task.Run(() => new MetadataEditor.Engine.FileMetadataWrapper(targetPath));
                    this.Invoke((MethodInvoker)delegate {
                        propertyGrid.SelectedObject = wrapper;
                    });
                }
                catch {}
            }
        }
    }

    public class ImageManagementPropertyPage : SharpPropertyPage
    {
        private string targetPath = "";
        private ComboBox deviceList;
        private ComboBox bootSelection;
        private ComboBox partitionScheme;
        private ComboBox targetSystem;
        private TextBox volumeLabel;
        private ComboBox fileSystem;
        private ComboBox clusterSize;
        private ProgressBar progressBar;
        private Label statusLabel;
        private Button startBtn;
        private Label footerStatus;
        
        private Panel mainScrollPanel;
        private Panel advancedDrivePanel;
        private Panel advancedFormatPanel;
        private GroupBox formatGroup;
        private CheckBox badBlockChk;
        private ComboBox badBlockPasses;

        private List<string> logs = new List<string>();

        public ImageManagementPropertyPage()
        {
            this.Padding = new Padding(0);
            PageTitle = "Image Management Features";
            InitializeComponent();
        }

        protected override void OnPropertyPageInitialised(SharpPropertySheet parent)
        {
            if (parent.SelectedItemPaths.Any())
            {
                targetPath = parent.SelectedItemPaths.First();
                bootSelection.Text = Path.GetFileName(targetPath);
                // Discovery is async to prevent shell freeze
                RefreshDeviceListAsync();
            }
        }

        private void InitializeComponent()
        {
            mainScrollPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(2) };
            
            // --- Drive Properties ---
            GroupBox driveGroup = new GroupBox { Text = "Drive Properties", Location = new Point(5, 5), Size = new Size(195, 150), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            
            Label devLabel = new Label { Text = "Device:", Location = new Point(5, 20), AutoSize = true };
            deviceList = new ComboBox { Location = new Point(5, 35), Size = new Size(160, 21), DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            Button saveBtn = new Button { Text = "S", Location = new Point(168, 33), Size = new Size(22, 23), Anchor = AnchorStyles.Top | AnchorStyles.Right };

            Label bootLabel = new Label { Text = "Boot selection:", Location = new Point(5, 60), AutoSize = true };
            bootSelection = new ComboBox { Location = new Point(5, 75), Size = new Size(100, 21), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            Label checkMark = new Label { Text = "V", Location = new Point(108, 77), Size = new Size(15, 20), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            Button selectBtn = new Button { Text = "...", Location = new Point(125, 73), Size = new Size(65, 25), Anchor = AnchorStyles.Top | AnchorStyles.Right };

            Label partLabel = new Label { Text = "Partition:", Location = new Point(5, 105), AutoSize = true };
            partitionScheme = new ComboBox { Location = new Point(5, 120), Size = new Size(90, 21), DropDownStyle = ComboBoxStyle.DropDownList };
            partitionScheme.Items.AddRange(new object[] { "MBR", "GPT" });
            partitionScheme.SelectedIndex = 1;

            Label targetLabel = new Label { Text = "Target:", Location = new Point(100, 105), AutoSize = true };
            targetSystem = new ComboBox { Location = new Point(100, 120), Size = new Size(90, 21), DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            targetSystem.Items.AddRange(new object[] { "BIOS", "UEFI" });
            targetSystem.SelectedIndex = 1;

            driveGroup.Controls.AddRange(new Control[] { devLabel, deviceList, saveBtn, bootLabel, bootSelection, checkMark, selectBtn, partLabel, partitionScheme, targetLabel, targetSystem });

            CheckBox advDriveToggle = new CheckBox { Text = "Advanced properties", Location = new Point(5, 158), AutoSize = true };
            advancedDrivePanel = new Panel { Location = new Point(5, 175), Size = new Size(195, 65), Visible = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            advancedDrivePanel.Controls.Add(new CheckBox { Text = "List USB HDDs", Location = new Point(5, 0), AutoSize = true });
            advancedDrivePanel.Controls.Add(new CheckBox { Text = "BIOS Fixes", Location = new Point(5, 20), AutoSize = true });
            advancedDrivePanel.Controls.Add(new CheckBox { Text = "UEFI Val", Location = new Point(5, 40), AutoSize = true });
            
            advDriveToggle.CheckedChanged += (s, e) => {
                advancedDrivePanel.Visible = advDriveToggle.Checked;
                UpdateLayout();
            };

            // --- Format Options ---
            formatGroup = new GroupBox { Text = "Format Options", Location = new Point(5, 180), Size = new Size(195, 120), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            Label volLabel = new Label { Text = "Label:", Location = new Point(5, 20), AutoSize = true };
            volumeLabel = new TextBox { Location = new Point(5, 35), Size = new Size(185, 21), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            Label fsLabel = new Label { Text = "FS:", Location = new Point(5, 60), AutoSize = true };
            fileSystem = new ComboBox { Location = new Point(5, 75), Size = new Size(90, 21), DropDownStyle = ComboBoxStyle.DropDownList };
            fileSystem.Items.AddRange(new object[] { "FAT32", "NTFS", "exFAT", "UDF" });
            fileSystem.SelectedIndex = 1;

            Label clusterLabel = new Label { Text = "Cluster:", Location = new Point(100, 60), AutoSize = true };
            clusterSize = new ComboBox { Location = new Point(100, 75), Size = new Size(90, 21), DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            clusterSize.Items.Add("Default");
            clusterSize.SelectedIndex = 0;

            formatGroup.Controls.AddRange(new Control[] { volLabel, volumeLabel, fsLabel, fileSystem, clusterLabel, clusterSize });

            CheckBox advFormatToggle = new CheckBox { Text = "Advanced format", Location = new Point(5, 305), AutoSize = true };
            advancedFormatPanel = new Panel { Location = new Point(5, 325), Size = new Size(195, 65), Visible = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            advancedFormatPanel.Controls.Add(new CheckBox { Text = "Quick format", Location = new Point(5, 0), AutoSize = true, Checked = true });
            advancedFormatPanel.Controls.Add(new CheckBox { Text = "Ext label", Location = new Point(5, 20), AutoSize = true, Checked = true });
            badBlockChk = new CheckBox { Text = "Bad blocks", Location = new Point(5, 40), AutoSize = true };
            badBlockPasses = new ComboBox { Location = new Point(120, 38), Size = new Size(70, 21), DropDownStyle = ComboBoxStyle.DropDownList };
            badBlockPasses.Items.AddRange(new object[] { "1p", "2p", "3p", "4p" });
            badBlockPasses.SelectedIndex = 0;
            advancedFormatPanel.Controls.AddRange(new Control[] { badBlockChk, badBlockPasses });

            advFormatToggle.CheckedChanged += (s, e) => {
                advancedFormatPanel.Visible = advFormatToggle.Checked;
                UpdateLayout();
            };

            // --- Status & Buttons ---
            statusLabel = new Label { Text = "READY", Location = new Point(5, 340), Size = new Size(195, 20), TextAlign = ContentAlignment.MiddleCenter, BorderStyle = BorderStyle.Fixed3D, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            progressBar = new ProgressBar { Location = new Point(5, 365), Size = new Size(195, 10), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            
            startBtn = new Button { Text = "START", Location = new Point(5, 380), Size = new Size(95, 30) };
            Button closeBtn = new Button { Text = "CLOSE", Location = new Point(105, 380), Size = new Size(95, 30), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            
            // Footer Icons
            Panel footer = new Panel { Location = new Point(5, 415), Size = new Size(195, 30), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            Button webBtn = new Button { Text = "W", Location = new Point(0, 0), Size = new Size(22, 22) };
            Button infoBtn = new Button { Text = "I", Location = new Point(25, 0), Size = new Size(22, 22) };
            Button settingsBtn = new Button { Text = "S", Location = new Point(50, 0), Size = new Size(22, 22) };
            Button logBtn = new Button { Text = "L", Location = new Point(75, 0), Size = new Size(22, 22) };
            logBtn.Click += (s, e) => ShowLog();
            
            footerStatus = new Label { Text = "Checking...", Location = new Point(100, 5), AutoSize = true };
            footer.Controls.AddRange(new Control[] { webBtn, infoBtn, settingsBtn, logBtn, footerStatus });

            startBtn.Click += StartBtn_Click;

            mainScrollPanel.Controls.AddRange(new Control[] { driveGroup, advDriveToggle, advancedDrivePanel, formatGroup, advFormatToggle, advancedFormatPanel, statusLabel, progressBar, startBtn, closeBtn, footer });
            this.Controls.Add(mainScrollPanel);
            
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            int currentY = 158;
            advancedDrivePanel.Top = currentY + 17;
            if (advancedDrivePanel.Visible) currentY += 82; else currentY += 22;
            
            formatGroup.Top = currentY;
            currentY += 125;
            
            // Advanced format toggle
            Control advFormatToggle = mainScrollPanel.Controls.Cast<Control>().First(c => c.Text == "Advanced format");
            advFormatToggle.Top = currentY;
            currentY += 20;
            
            advancedFormatPanel.Top = currentY;
            if (advancedFormatPanel.Visible) currentY += 70;
            
            statusLabel.Top = currentY; currentY += 25;
            progressBar.Top = currentY; currentY += 15;
            startBtn.Top = currentY; 
            mainScrollPanel.Controls.Cast<Control>().First(c => c.Text == "CLOSE").Top = currentY;
            currentY += 35;
            
            // Footer
            mainScrollPanel.Controls.Cast<Control>().First(c => c is Panel && c != advancedDrivePanel && c != advancedFormatPanel).Top = currentY;
        }

        private async void RefreshDeviceListAsync()
        {
            try
            {
                var devices = await System.Threading.Tasks.Task.Run(() => BurnEngine.EnumerateTargetDevices());
                this.Invoke((MethodInvoker)delegate {
                    deviceList.Items.Clear();
                    foreach (var dev in devices) deviceList.Items.Add(dev.DisplayName);
                    if (deviceList.Items.Count > 0)
                    {
                        deviceList.SelectedIndex = 0;
                        footerStatus.Text = $"{deviceList.Items.Count} device found";
                    }
                    else footerStatus.Text = "0 devices found";
                });
            }
            catch { }
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            if (deviceList.SelectedIndex < 0) return;

            string selectedDevice = deviceList.SelectedItem.ToString();
            string driveLetter = selectedDevice.Substring(0, 3);

            bool isWindowsIso = false;
            try {
                if (targetPath.ToLower().Contains("windows") || targetPath.ToLower().Contains("win10") || targetPath.ToLower().Contains("win11")) isWindowsIso = true;
            } catch {}

            BurnEngine.WindowsExperienceOptions experience = null;
            if (isWindowsIso)
            {
                using (var dlg = new WindowsExperienceDialog(Environment.UserName))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        experience = new BurnEngine.WindowsExperienceOptions
                        {
                            CreateLocalAccount = dlg.CreateLocalAccount,
                            Username = dlg.Username,
                            SetRegionalOptions = dlg.SetRegionalOptions,
                            DisableDataCollection = dlg.DisableDataCollection,
                            DisableBitLocker = dlg.DisableBitLocker
                        };
                    }
                    else return;
                }
            }

            DialogResult res = MessageBox.Show($"WARNING: ALL DATA ON DRIVE {driveLetter} WILL BE DESTROYED.\nTo continue with this operation, click OK. To quit, click CANCEL.", "WARNING", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            
            if (res == DialogResult.OK)
            {
                logs.Clear();
                logs.Add($"Operation started for {driveLetter}");
                startBtn.Enabled = false;
                System.Threading.Tasks.Task.Run(() => {
                    BurnEngine.WriteImageToDisk(targetPath, driveLetter, (progress, status) => {
                        this.Invoke((MethodInvoker)delegate {
                            progressBar.Value = progress;
                            statusLabel.Text = status;
                            logs.Add($"[{progress}%] {status}");
                            if (progress == 100) startBtn.Enabled = true;
                        });
                    }, experience);
                });
            }
        }

        private void ShowLog()
        {
            Form logForm = new Form { Text = "Log", Size = new Size(500, 400), StartPosition = FormStartPosition.CenterParent };
            TextBox logBox = new TextBox { Multiline = true, Dock = DockStyle.Fill, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Text = string.Join(Environment.NewLine, logs) };
            Panel btnPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            Button clearBtn = new Button { Text = "Clear", Location = new Point(10, 5) };
            Button saveBtn = new Button { Text = "Save", Location = new Point(100, 5) };
            Button closeBtn = new Button { Text = "Close", Location = new Point(380, 5) };
            
            clearBtn.Click += (s, e) => { logs.Clear(); logBox.Clear(); };
            closeBtn.Click += (s, e) => logForm.Close();
            
            btnPanel.Controls.AddRange(new Control[] { clearBtn, saveBtn, closeBtn });
            logForm.Controls.AddRange(new Control[] { logBox, btnPanel });
            logForm.ShowDialog();
        }
    }
}

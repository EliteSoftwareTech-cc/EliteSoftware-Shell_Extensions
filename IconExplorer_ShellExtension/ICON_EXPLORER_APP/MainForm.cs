#pragma warning disable CS8618, CS8600, CS8602, CS8622, CS8625
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using IconExplorer.Engine;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace IconExplorer.App
{
    public class MainForm : Form
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);


        private Label pathLabel;
        private TextBox pathBox;
        private Button browseBtn;
        private Label listLabel;
        private ListView iconList;
        private ImageList imageList;
        private Button extractBtn;
        private Button restoreSizeBtn;
        private CheckBox openFolderCheck;
        private Button doneBtn;
        private Button cancelBtn;

        private ContextMenuStrip iconContextMenu;
        private string currentRightClickFile = "";
        
        private bool showIDs = false;
        private bool multiMode = false;
        private int targetZoomSize = 64;
        private System.Windows.Forms.Timer zoomTimer;
        
        private System.Windows.Forms.Timer asyncTimer;
        private int extractIndex = 0;
        private uint totalIcons = 0;
        private string targetFile = "";
        private List<Image> imageCache = new List<Image>();
        private string tempDir;

        private bool isPickMode = false;
        private bool pickAdvanced = false;
        private string initialFilePath = "";

        public MainForm(string[] args = null)
        {
            if (args != null)
            {
                foreach (var arg in args)
                {
                    if (arg.Equals("-pick", StringComparison.OrdinalIgnoreCase))
                    {
                        isPickMode = true;
                    }
                    else if (arg.Equals("-pick_advanced", StringComparison.OrdinalIgnoreCase))
                    {
                        isPickMode = true;
                        pickAdvanced = true;
                    }
                    else if (File.Exists(arg))
                    {
                        initialFilePath = arg;
                    }
                }
            }

            InitializeComponent();
            SetupTempDir();
            SetupContextMenu();
            SetupTimers();
            this.TopMost = true;
            
            // Double buffering
            PropertyInfo prop = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null)
                prop.SetValue(iconList, true, null);

            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;
            this.MouseWheel += IconList_MouseWheel;
            this.Resize += MainForm_Resize;
        }

        private const string RegPath = @"Software\EliteSoftwareTech\IconExplorer\Settings";

        private void LoadSettings()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegPath))
                {
                    if (key != null)
                    {
                        int width = (int)key.GetValue("WindowWidth", this.Width);
                        int height = (int)key.GetValue("WindowHeight", this.Height);
                        int x = (int)key.GetValue("WindowX", this.Location.X);
                        int y = (int)key.GetValue("WindowY", this.Location.Y);
                        
                        this.Size = new Size(Math.Max(500, width), Math.Max(400, height));
                        
                        // Prevent loading off-screen
                        if (x >= SystemInformation.VirtualScreen.Left && y >= SystemInformation.VirtualScreen.Top)
                        {
                            this.StartPosition = FormStartPosition.Manual;
                            this.Location = new Point(x, y);
                        }

                        if (!isPickMode && string.IsNullOrEmpty(initialFilePath))
                        {
                            string lastPath = (string)key.GetValue("LastPath", @"C:\Windows\System32\imageres.dll");
                            if (File.Exists(lastPath) || Directory.Exists(lastPath))
                                pathBox.Text = lastPath;
                        }

                        int savedViewIndex = (int)key.GetValue("ViewIndex", 3);
                        if (savedViewIndex >= 0 && savedViewIndex < viewSelector.Items.Count)
                            viewSelector.SelectedIndex = savedViewIndex;

                        showIDs = Convert.ToBoolean(key.GetValue("ShowIDs", false));
                        multiMode = Convert.ToBoolean(key.GetValue("MultiMode", false));
                        openFolderCheck.Checked = Convert.ToBoolean(key.GetValue("OpenFolder", false));

                        if (multiMode)
                        {
                            multiSelectBtn.Text = "Multi-Select: ON";
                            iconList.CheckBoxes = true;
                            iconList.MultiSelect = true;
                        }
                    }
                }
            }
            catch { }
        }

        private void SaveSettings()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegPath))
                {
                    if (this.WindowState == FormWindowState.Normal)
                    {
                        key.SetValue("WindowWidth", this.Width);
                        key.SetValue("WindowHeight", this.Height);
                        key.SetValue("WindowX", this.Location.X);
                        key.SetValue("WindowY", this.Location.Y);
                    }
                    key.SetValue("LastPath", pathBox.Text);
                    key.SetValue("ViewIndex", viewSelector.SelectedIndex);
                    key.SetValue("ShowIDs", showIDs);
                    key.SetValue("MultiMode", multiMode);
                    key.SetValue("OpenFolder", openFolderCheck.Checked);
                }
            }
            catch { }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            CenterBottomButtons();
        }

        private void CenterBottomButtons()
        {
            if (doneBtn != null && cancelBtn != null)
            {
                int totalWidth = doneBtn.Width + 10 + cancelBtn.Width;
                int startX = (this.ClientSize.Width - totalWidth) / 2;
                doneBtn.Location = new Point(startX, this.ClientSize.Height - 35);
                cancelBtn.Location = new Point(startX + doneBtn.Width + 10, this.ClientSize.Height - 35);
            }
        }

        private void SetupTempDir()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "EliteIconViewer");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }
        }

        private FlowLayoutPanel topPanel;
        private Button selectAllBtn;
        private ComboBox viewSelector;
        private Button extractAllBtn;
        private Button multiSelectBtn;
        private Button toggleBtn;

        private Button propsBtn;

        [StructLayout(LayoutKind.Sequential)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        public const uint SEE_MASK_INVOKEIDLIST = 0x0000000C;

        private void ShowProperties(string path)
        {
            if (!File.Exists(path) && !Directory.Exists(path)) return;
            SHELLEXECUTEINFO sei = new SHELLEXECUTEINFO();
            sei.cbSize = Marshal.SizeOf(sei);
            sei.lpVerb = "properties";
            sei.lpFile = path;
            sei.nShow = 5; // SW_SHOW
            sei.fMask = SEE_MASK_INVOKEIDLIST;
            ShellExecuteEx(ref sei);
        }

        private void InitializeComponent()
        {
            this.Text = "Icon Viewer";
            this.Size = new Size(600, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;
            this.MinimumSize = new Size(500, 400);
            this.SizeGripStyle = SizeGripStyle.Show;

            pathLabel = new Label { Text = "Look for icons in this file:", Location = new Point(10, 10), AutoSize = true };
            
            pathBox = new TextBox 
            {
                Location = new Point(10, 30),
                Size = new Size(420, 20),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Text = @"C:\Windows\System32\imageres.dll",
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.FileSystem
            };

            browseBtn = new Button 
            {
                Text = "...",
                Location = new Point(440, 28),
                Size = new Size(30, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            propsBtn = new Button
            {
                Text = "Properties",
                Location = new Point(475, 28),
                Size = new Size(100, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            // Top Button Panel
            topPanel = new FlowLayoutPanel
            {
                Location = new Point(10, 55),
                Size = new Size(570, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            selectAllBtn = new Button { Text = "Select All", Size = new Size(75, 23) };
            
            viewSelector = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(110, 23) };
            viewSelector.Items.AddRange(new object[] { "16x16 (Tiny)", "32x32 (Small)", "48x48 (Med)", "64x64 (Large)", "128x128 (Huge)", "256x256 (Native)" });
            viewSelector.SelectedIndex = 3; // 64x64

            multiSelectBtn = new Button { Text = "Multi: OFF", Size = new Size(85, 23) };
            toggleBtn = new Button { Text = "Toggle IDs", Size = new Size(75, 23) };
            
            topPanel.Controls.AddRange(new Control[] { viewSelector, selectAllBtn, multiSelectBtn, toggleBtn });

            imageList = new ImageList 
            {
                ImageSize = new Size(64, 64),
                ColorDepth = ColorDepth.Depth32Bit
            };

            iconList = new ListView 
            {
                Location = new Point(10, 90),
                Size = new Size(580, 300),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                View = View.LargeIcon,
                MultiSelect = false,
                LargeImageList = imageList,
                HideSelection = false
            };

            extractBtn = new Button 
            {
                Text = "Extract Selected",
                Location = new Point(10, 400),
                Size = new Size(110, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            extractAllBtn = new Button 
            {
                Text = "Extract ALL Icons",
                Location = new Point(125, 400),
                Size = new Size(110, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            restoreSizeBtn = new Button 
            {
                Text = "Restore Size",
                Location = new Point(240, 400),
                Size = new Size(100, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            openFolderCheck = new CheckBox 
            {
                Text = "Open folder after extract",
                Location = new Point(12, 435),
                AutoSize = true,
                Checked = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            doneBtn = new Button { Text = "Done", Size = new Size(75, 25), Anchor = AnchorStyles.Bottom };
            cancelBtn = new Button { Text = "Cancel", Size = new Size(75, 25), Anchor = AnchorStyles.Bottom };

            this.Controls.AddRange(new Control[] { 
                pathLabel, pathBox, browseBtn, propsBtn, topPanel, 
                iconList, extractBtn, extractAllBtn, restoreSizeBtn, openFolderCheck, doneBtn, cancelBtn 
            });

            CenterBottomButtons();

            if (isPickMode && !pickAdvanced)
            {
                extractBtn.Visible = false;
                extractAllBtn.Visible = false;
                openFolderCheck.Visible = false;
                multiSelectBtn.Visible = false;
                restoreSizeBtn.Location = new Point(10, 400); 
            }

            // Events
            browseBtn.Click += BrowseBtn_Click;
            propsBtn.Click += PropsBtn_Click;
            pathBox.KeyDown += PathBox_KeyDown;
            selectAllBtn.Click += SelectAllBtn_Click;
            
            viewSelector.SelectedIndexChanged += ViewSelector_SelectedIndexChanged;
            viewSelector.SelectedIndexChanged += (s, e) => SaveSettings();
            
            multiSelectBtn.Click += MultiSelectBtn_Click;
            multiSelectBtn.Click += (s, e) => SaveSettings();
            
            toggleBtn.Click += ToggleBtn_Click;
            toggleBtn.Click += (s, e) => SaveSettings();
            
            openFolderCheck.CheckedChanged += (s, e) => SaveSettings();
            
            this.ResizeEnd += (s, e) => SaveSettings(); // Fires after resizing OR moving the window

            restoreSizeBtn.Click += RestoreSizeBtn_Click;
            extractBtn.Click += ExtractBtn_Click;
            extractAllBtn.Click += ExtractAllBtn_Click;
            doneBtn.Click += DoneBtn_Click;
            cancelBtn.Click += (s, e) => this.Close();
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
            
            iconList.DoubleClick += IconList_DoubleClick;
            iconList.MouseClick += IconList_MouseClick;
        }

        private void SelectAllBtn_Click(object sender, EventArgs e)
        {
            iconList.BeginUpdate();
            foreach (ListViewItem item in iconList.Items)
            {
                if (multiMode) item.Checked = true;
                item.Selected = true;
            }
            iconList.EndUpdate();
        }

        private void ViewSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            int size = 64;
            switch (viewSelector.SelectedIndex)
            {
                case 0: size = 16; break;
                case 1: size = 32; break;
                case 2: size = 48; break;
                case 3: size = 64; break;
                case 4: size = 128; break;
                case 5: size = 256; break;
            }
            targetZoomSize = size;
            zoomTimer.Stop();
            zoomTimer.Start();
        }

        private void ExtractAllBtn_Click(object sender, EventArgs e)
        {
            if (iconList.Items.Count == 0) return;

            DialogResult res = MessageBox.Show(
                $"Warning: You are about to extract ALL {iconList.Items.Count} icons from this file. This may clutter your workspace.\n\nContinue with extraction?",
                "Extract ALL Icons",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Hand 
            );

            if (res == DialogResult.Yes)
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.Description = "Select a folder to save ALL icons";
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        string targetDir = fbd.SelectedPath;
                        string sourcePath = ((IconMeta)iconList.Items[0].Tag).SourceFile;
                        string sourceName = Path.GetFileName(sourcePath);
                        string finalTargetDir = Path.Combine(targetDir, sourceName + "_ALL");

                        if (!Directory.Exists(finalTargetDir)) Directory.CreateDirectory(finalTargetDir);

                        this.Cursor = Cursors.WaitCursor;
                        foreach (ListViewItem item in iconList.Items)
                        {
                            var meta = (IconMeta)item.Tag;
                            string finalFile = Path.Combine(finalTargetDir, $"IconGroup_{meta.Index}.ico");
                            EliteIconExtractor.SaveFullIconGroup(meta.SourceFile, meta.Index, finalFile);
                        }
                        this.Cursor = Cursors.Default;

                        if (openFolderCheck.Checked) Process.Start(new ProcessStartInfo(finalTargetDir) { UseShellExecute = true });
                        else MessageBox.Show("All icons extracted successfully to " + finalTargetDir, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void SetupContextMenu()
        {
            iconContextMenu = new ContextMenuStrip();
            
            var menuOpen = new ToolStripMenuItem("Open (Default Viewer)");
            menuOpen.Click += (s, e) => {
                if (File.Exists(currentRightClickFile))
                    Process.Start(new ProcessStartInfo(currentRightClickFile) { UseShellExecute = true });
            };
            iconContextMenu.Items.Add(menuOpen);
            iconContextMenu.Items.Add(new ToolStripSeparator());

            var menuSendTo = new ToolStripMenuItem("Send To");
            string sendToPath = Environment.GetFolderPath(Environment.SpecialFolder.SendTo);
            if (Directory.Exists(sendToPath))
            {
                foreach (var item in Directory.GetFiles(sendToPath))
                {
                    string ext = Path.GetExtension(item).ToLower();
                    if (ext == ".lnk" || ext == ".exe" || ext == ".bat" || ext == ".cmd")
                    {
                        var subItem = new ToolStripMenuItem(Path.GetFileNameWithoutExtension(item));
                        subItem.Tag = item;
                        subItem.Click += (s, e) => {
                            string target = ((ToolStripMenuItem)s).Tag.ToString();
                            Process.Start(new ProcessStartInfo("cmd.exe", $"/c start \"\" \"{target}\" \"{currentRightClickFile}\"" ) 
                            { 
                                WindowStyle = ProcessWindowStyle.Hidden,
                                CreateNoWindow = true
                            });
                        };
                        menuSendTo.DropDownItems.Add(subItem);
                    }
                }
            }
            iconContextMenu.Items.Add(menuSendTo);
            iconContextMenu.Items.Add(new ToolStripSeparator());

            var menuLoc = new ToolStripMenuItem("Show in Explorer");
            menuLoc.Click += (s, e) => {
                if (File.Exists(currentRightClickFile))
                    Process.Start("explorer.exe", $"/select,\"{currentRightClickFile}\"");
            };
            iconContextMenu.Items.Add(menuLoc);
        }

        private void SetupTimers()
        {
            zoomTimer = new System.Windows.Forms.Timer { Interval = 250 };
            zoomTimer.Tick += ZoomTimer_Tick;

            asyncTimer = new System.Windows.Forms.Timer { Interval = 30 };
            asyncTimer.Tick += AsyncTimer_Tick;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetWindowTheme(iconList.Handle, "Explorer", null);

            LoadSettings();

            if (!string.IsNullOrEmpty(initialFilePath))
            {
                pathBox.Text = initialFilePath;
            }

            if (File.Exists(pathBox.Text))
            {
                StartAsyncLoad(pathBox.Text);
            }
        }

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Executable & Libraries|*.exe;*.dll;*.ocx;*.cpl;*.icl;*.ico;*.mui;*.mun|All Files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pathBox.Text = ofd.FileName;
                    StartAsyncLoad(ofd.FileName);
                }
            }
        }

        private void PropsBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(pathBox.Text))
            {
                ShowProperties(pathBox.Text);
            }
        }

        private void PathBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                StartAsyncLoad(pathBox.Text);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void MultiSelectBtn_Click(object sender, EventArgs e)
        {
            multiMode = !multiMode;
            if (multiMode)
            {
                multiSelectBtn.Text = "Multi-Select: ON";
                iconList.CheckBoxes = true;
                iconList.MultiSelect = true;
            }
            else
            {
                multiSelectBtn.Text = "Multi-Select: OFF";
                iconList.CheckBoxes = false;
                iconList.MultiSelect = false;
            }
        }

        private void ToggleBtn_Click(object sender, EventArgs e)
        {
            showIDs = !showIDs;
            iconList.BeginUpdate();
            for (int i = 0; i < iconList.Items.Count; i++)
            {
                iconList.Items[i].Text = showIDs ? $"ID: {i}" : "";
            }
            iconList.EndUpdate();
        }

        private void IconList_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                int newSize = imageList.ImageSize.Width + (e.Delta / 10);
                if (newSize >= 16 && newSize <= 256)
                {
                    targetZoomSize = newSize;
                    zoomTimer.Stop();
                    zoomTimer.Start();
                }
            }
        }

        private void RestoreSizeBtn_Click(object sender, EventArgs e)
        {
            targetZoomSize = 64;
            zoomTimer.Stop();
            zoomTimer.Start();
        }

        private void ZoomTimer_Tick(object sender, EventArgs e)
        {
            zoomTimer.Stop();
            bool wasLoading = asyncTimer.Enabled;
            if (wasLoading) asyncTimer.Stop();

            this.Cursor = Cursors.WaitCursor;
            iconList.BeginUpdate();
            iconList.SuspendLayout();

            imageList.Images.Clear();
            imageList.ImageSize = new Size(targetZoomSize, targetZoomSize);
            
            // Re-adding images at the new size triggers internal resampling
            foreach (var img in imageCache)
            {
                imageList.Images.Add(img);
            }

            iconList.ResumeLayout();
            iconList.EndUpdate();
            this.Cursor = Cursors.Default;

            if (wasLoading) asyncTimer.Start();
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        public const uint SHGFI_ICONLOCATION = 0x1000;

        private class IconMeta
        {
            public string SourceFile { get; set; }
            public int Index { get; set; }
        }

        private void StartAsyncLoad(string filePath)
        {
            if (!File.Exists(filePath) && !Directory.Exists(filePath)) return;

            string resolvedSource = filePath;
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(filePath, 0, out shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICONLOCATION);
            
            if (!string.IsNullOrEmpty(shinfo.szDisplayName))
            {
                string exp = Environment.ExpandEnvironmentVariables(shinfo.szDisplayName);
                if (File.Exists(exp)) resolvedSource = exp;
            }

            asyncTimer.Stop();
            this.Text = "Icon Viewer - Calculating...";

            foreach (var img in imageCache) img.Dispose();
            imageCache.Clear();
            iconList.Items.Clear();
            imageList.Images.Clear();

            targetFile = resolvedSource;
            pathBox.Text = resolvedSource;

            totalIcons = EliteIconExtractor.PrivateExtractIconsW(targetFile, 0, 0, 0, null, null, 0, 0);
            
            // Fallback to default shell32 if no icons found in resolved file
            if (totalIcons == 0)
            {
                targetFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
                pathBox.Text = targetFile;
                totalIcons = EliteIconExtractor.PrivateExtractIconsW(targetFile, 0, 0, 0, null, null, 0, 0);
            }

            extractIndex = 0;

            if (totalIcons > 0)
                asyncTimer.Start();
            else
                this.Text = "Icon Viewer - No Icons Found";
        }

        private void AsyncTimer_Tick(object sender, EventArgs e)
        {
            asyncTimer.Stop();
            int chunkSize = 50;
            int endIndex = Math.Min(extractIndex + chunkSize, (int)totalIcons);
            int countToExtract = endIndex - extractIndex;

            if (countToExtract <= 0)
            {
                this.Text = "Icon Viewer";
                return;
            }

            IntPtr[] phicon = new IntPtr[countToExtract];
            IntPtr[] piconid = new IntPtr[countToExtract];
            EliteIconExtractor.PrivateExtractIconsW(targetFile, extractIndex, 256, 256, phicon, piconid, (uint)countToExtract, 0);

            iconList.BeginUpdate();
            for (int i = 0; i < countToExtract; i++)
            {
                if (phicon[i] != IntPtr.Zero)
                {
                    int actualIndex = extractIndex + i;
                    using (Icon ico = Icon.FromHandle(phicon[i]))
                    {
                        Bitmap bmp = ico.ToBitmap();
                        imageCache.Add(bmp);
                        imageList.Images.Add(bmp);

                        string itemText = showIDs ? $"ID: {actualIndex}" : "";
                        var item = new ListViewItem(itemText, actualIndex);
                        item.Tag = new IconMeta { SourceFile = targetFile, Index = actualIndex };
                        iconList.Items.Add(item);

                        if (actualIndex == 0)
                        {
                            if (this.Icon != null) this.Icon.Dispose();
                            this.Icon = Icon.FromHandle(phicon[i]);
                        }
                        else
                        {
                            EliteIconExtractor.DestroyIcon(phicon[i]);
                        }
                    }
                }
            }
            iconList.EndUpdate();

            extractIndex = endIndex;

            if (extractIndex < totalIcons)
            {
                this.Text = $"Icon Viewer - Loading... ({extractIndex} / {totalIcons})";
                asyncTimer.Start();
            }
            else
            {
                this.Text = "Icon Viewer";
            }
        }

        private string ExtractSingleIconToTemp(string sourceFile, int index)
        {
            string safeName = Path.GetFileName(sourceFile).Replace(".", "_");
            string targetFile = Path.Combine(tempDir, $"{safeName}_IconGroup_{index}.ico");

            if (!File.Exists(targetFile))
            {
                EliteIconExtractor.SaveFullIconGroup(sourceFile, index, targetFile);
            }
            return targetFile;
        }

        private void IconList_DoubleClick(object sender, EventArgs e)
        {
            if (iconList.SelectedItems.Count > 0)
            {
                var meta = (IconMeta)iconList.SelectedItems[0].Tag;
                if (isPickMode)
                {
                    Console.WriteLine($"{meta.SourceFile},{meta.Index}");
                    Environment.Exit(0);
                }
                else
                {
                    string physFile = ExtractSingleIconToTemp(meta.SourceFile, meta.Index);
                    if (!string.IsNullOrEmpty(physFile) && File.Exists(physFile))
                    {
                        Process.Start(new ProcessStartInfo(physFile) { UseShellExecute = true });
                    }
                }
            }
        }

        private void DoneBtn_Click(object sender, EventArgs e)
        {
            if (isPickMode && iconList.SelectedItems.Count > 0)
            {
                var meta = (IconMeta)iconList.SelectedItems[0].Tag;
                Console.WriteLine($"{meta.SourceFile},{meta.Index}");
                Environment.Exit(0);
            }
            else
            {
                this.Close();
            }
        }

        private void IconList_MouseClick(object sender, MouseEventArgs e)
        {
            // Only hide context menu if we are in simple pick mode (not advanced)
            if (e.Button == MouseButtons.Right && iconList.SelectedItems.Count > 0 && !(isPickMode && !pickAdvanced))
            {
                var meta = (IconMeta)iconList.SelectedItems[0].Tag;
                currentRightClickFile = ExtractSingleIconToTemp(meta.SourceFile, meta.Index);
                if (!string.IsNullOrEmpty(currentRightClickFile) && File.Exists(currentRightClickFile))
                {
                    iconContextMenu.Show(iconList, e.Location);
                }
            }
        }

        private void ExtractBtn_Click(object sender, EventArgs e)
        {
            string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            List<ListViewItem> targets = new List<ListViewItem>();

            if (multiMode)
            {
                foreach (ListViewItem item in iconList.CheckedItems) targets.Add(item);
            }
            else
            {
                foreach (ListViewItem item in iconList.SelectedItems) targets.Add(item);
            }

            if (targets.Count == 0) return;

            string sourcePath = ((IconMeta)targets[0].Tag).SourceFile;
            string sourceName = Path.GetFileName(sourcePath);
            this.Cursor = Cursors.WaitCursor;

            if (targets.Count == 1)
            {
                int index = ((IconMeta)targets[0].Tag).Index;
                string dateStr = DateTime.Now.ToString("yyyyMMdd");
                string finalFile = Path.Combine(desktopDir, $"{sourceName}_IconGroup_{index}_{dateStr}.ico");
                EliteIconExtractor.SaveFullIconGroup(sourcePath, index, finalFile);

                if (openFolderCheck.Checked)
                {
                    Process.Start("explorer.exe", $"/select,\"{finalFile}\"");
                }
            }
            else
            {
                string dateStr = DateTime.Now.ToString("yyyyMMdd");
                string targetDir = Path.Combine(desktopDir, sourceName + "_" + dateStr);
                if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

                foreach (var item in targets)
                {
                    int index = ((IconMeta)item.Tag).Index;
                    string finalFile = Path.Combine(targetDir, $"IconGroup_{index}.ico");
                    EliteIconExtractor.SaveFullIconGroup(sourcePath, index, finalFile);
                }

                if (openFolderCheck.Checked)
                {
                    Process.Start(new ProcessStartInfo(targetDir) { UseShellExecute = true });
                }
            }
            this.Cursor = Cursors.Default;
        }
    }
}


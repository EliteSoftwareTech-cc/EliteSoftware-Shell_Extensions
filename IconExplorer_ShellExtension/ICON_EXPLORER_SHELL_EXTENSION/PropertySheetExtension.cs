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
using IconExplorer.Engine;
using System.Diagnostics;
using System.Reflection;

namespace IconExplorer.ShellExtension
{
    [ComVisible(true)]
    [Guid("7E1A63DF-014F-4C2E-A528-9DB33A4E7A68")]
    [RegistrationName("Icon Explorer")]
    [DisplayName("Icon Explorer Extension")]
    [COMServerAssociation(AssociationType.AllFiles)]
    [COMServerAssociation(AssociationType.Class, @"Directory")]
    [COMServerAssociation(AssociationType.Class, @"Directory\Background")]
    [COMServerAssociation(AssociationType.Class, @"Drive")]
    public class IconPropertySheetExtension : SharpPropertySheet
    {
        protected override bool CanShowSheet()
        {
            return SelectedItemPaths.Count() == 1;
        }

        protected override IEnumerable<SharpPropertyPage> CreatePages()
        {
            return new[] { new IconPropertyPage() };
        }
    }

    [ComVisible(true)]
    [Guid("A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D")] // Unique GUID for Context Menu
    [COMServerAssociation(AssociationType.AllFiles)]
    [COMServerAssociation(AssociationType.Class, "Directory")]
    public class IconExplorerContextMenu : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return SelectedItemPaths.Count() == 1;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();
            var item = new ToolStripMenuItem("Open in Icon Explorer");
            item.Click += (s, e) => {
                try
                {
                    string asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string appExe = Path.Combine(asmDir, "ICON_EXPLORER_APP.exe");
                    if (File.Exists(appExe)) Process.Start(appExe, $"\"{SelectedItemPaths.First()}\"");
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            };
            menu.Items.Add(item);
            return menu;
        }
    }

    public class IconPropertyPage : SharpPropertyPage
    {
        private string targetPath = "";
        
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

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern uint AssocQueryString(uint flags, uint str, string pszAssoc, string pszExtra, [Out] System.Text.StringBuilder pszOut, ref uint pcchOut);

        private string GetDefaultHandler(string filePath)
        {
            try
            {
                string extension = Path.GetExtension(filePath);
                if (string.IsNullOrEmpty(extension)) return "";

                uint pcchOut = 260;
                System.Text.StringBuilder pszOut = new System.Text.StringBuilder((int)pcchOut);
                // ASSOCSTR_EXECUTABLE = 2
                uint result = AssocQueryString(0, 2, extension, null, pszOut, ref pcchOut);
                if (result == 0) return pszOut.ToString();
            }
            catch { }
            return "";
        }

        private Label lblSource;
        private TextBox pathBox;
        private ListView iconList;
        private ImageList imageList;
        private Button extractBtn;
        private Button launchAppBtn;
        private Button toggleBtn;
        private Button multiSelectBtn;
        private Button restoreSizeBtn;
        
        private ContextMenuStrip iconContextMenu;
        private string currentRightClickFile = "";
        private string tempDir;

        private bool showIDs = false;
        private bool multiMode = false;
        private int targetZoomSize = 48;
        private System.Windows.Forms.Timer zoomTimer;
        private System.Windows.Forms.Timer asyncTimer;
        
        private uint totalIcons = 0;
        private int extractIndex = 0;
        private string resolvedSourceFile = "";
        private int resolvedIconIndex = 0;
        private List<Image> imageCache = new List<Image>();

        public IconPropertyPage()
        {
            // Use a narrower default to fit standard property sheets
            this.Size = new Size(320, 400); 
            this.Padding = new Padding(5);
            PageTitle = "Icon Explorer";
            InitializeComponent();
            SetupTempDir();
            SetupContextMenu();
            
            this.MouseWheel += IconList_MouseWheel;
        }

        protected override void OnPropertyPageInitialised(SharpPropertySheet parent)
        {
            if (parent.SelectedItemPaths.Any())
            {
                targetPath = parent.SelectedItemPaths.First();
                ResolveAndLoad();
            }
        }

        private void SetupTempDir()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "EliteIconViewer_Shell");
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
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

            var menuLoc = new ToolStripMenuItem("Show in Explorer");
            menuLoc.Click += (s, e) => {
                if (File.Exists(currentRightClickFile))
                    Process.Start("explorer.exe", $"/select,\"{currentRightClickFile}\"");
            };
            iconContextMenu.Items.Add(menuLoc);
        }
        
        private FlowLayoutPanel topPanel;
        private Button selectAllBtn;
        private ComboBox viewSelector;
        private Button extractAllBtn;

        private Button browseBtn;

        private void InitializeComponent()
        {
            this.lblSource = new Label { Text = "Resolved Icon Source:", Location = new Point(10, 5), AutoSize = true };
            this.pathBox = new TextBox 
            {
                Location = new Point(10, 22), 
                Width = 245, 
                ReadOnly = true, 
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right 
            };

            this.browseBtn = new Button
            {
                Text = "...",
                Location = new Point(260, 20),
                Size = new Size(30, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            
            // Top Button Panel
            topPanel = new FlowLayoutPanel
            {
                Location = new Point(10, 48),
                Size = new Size(280, 55),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };

            selectAllBtn = new Button { Text = "All", Size = new Size(45, 23) };
            
            viewSelector = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Size = new Size(85, 23) };
            viewSelector.Items.AddRange(new object[] { "16x16", "32x32", "48x48", "64x64", "128x128", "256x256" });
            viewSelector.SelectedIndex = 2; // 48x48 (Default)

            this.multiSelectBtn = new Button { Text = "Multi", Size = new Size(45, 23) };
            this.toggleBtn = new Button { Text = "IDs", Size = new Size(40, 23) };
            this.restoreSizeBtn = new Button { Text = "Reset", Size = new Size(45, 23) };

            topPanel.Controls.AddRange(new Control[] { viewSelector, selectAllBtn, multiSelectBtn, toggleBtn, restoreSizeBtn });

            this.imageList = new ImageList { ImageSize = new Size(48, 48), ColorDepth = ColorDepth.Depth32Bit };
            this.iconList = new ListView 
            {
                Location = new Point(10, 110), 
                Size = new Size(285, 210), 
                View = View.LargeIcon, 
                LargeImageList = this.imageList,
                MultiSelect = false,
                HideSelection = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            
            this.extractBtn = new Button { Text = "Extract", Location = new Point(10, 330), Size = new Size(65, 30), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            this.extractAllBtn = new Button { Text = "All", Location = new Point(80, 330), Size = new Size(40, 30), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            this.launchAppBtn = new Button { Text = "Full App", Location = new Point(125, 330), Size = new Size(165, 30), Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };

            this.browseBtn.Click += BrowseBtn_Click;
            this.selectAllBtn.Click += SelectAllBtn_Click;
            this.viewSelector.SelectedIndexChanged += ViewSelector_SelectedIndexChanged;
            this.multiSelectBtn.Click += MultiSelectBtn_Click;
            this.toggleBtn.Click += ToggleBtn_Click;
            this.restoreSizeBtn.Click += RestoreSizeBtn_Click;
            this.extractBtn.Click += ExtractBtn_Click;
            this.extractAllBtn.Click += ExtractAllBtn_Click;
            this.launchAppBtn.Click += LaunchAppBtn_Click;
            this.iconList.MouseClick += IconList_MouseClick;
            this.iconList.DoubleClick += IconList_DoubleClick;

            this.Controls.AddRange(new Control[] { lblSource, pathBox, browseBtn, topPanel, iconList, extractBtn, extractAllBtn, launchAppBtn });

            this.asyncTimer = new System.Windows.Forms.Timer { Interval = 30 };
            this.asyncTimer.Tick += AsyncTimer_Tick;

            this.zoomTimer = new System.Windows.Forms.Timer { Interval = 250 };
            this.zoomTimer.Tick += ZoomTimer_Tick;
            
            PropertyInfo prop = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null) prop.SetValue(iconList, true, null);
        }

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Executable & Libraries|*.exe;*.dll;*.ocx;*.cpl;*.icl;*.ico;*.mui;*.mun|All Files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    targetPath = ofd.FileName;
                    ResolveAndLoad();
                }
            }
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
                        string sourceName = Path.GetFileName(resolvedSourceFile);
                        string finalTargetDir = Path.Combine(targetDir, sourceName + "_ALL");

                        if (!Directory.Exists(finalTargetDir)) Directory.CreateDirectory(finalTargetDir);

                        Cursor.Current = Cursors.WaitCursor;
                        foreach (ListViewItem item in iconList.Items)
                        {
                            var meta = (IconMeta)item.Tag;
                            string finalFile = Path.Combine(finalTargetDir, $"IconGroup_{meta.Index}.ico");
                            EliteIconExtractor.SaveFullIconGroup(meta.SourceFile, meta.Index, finalFile);
                        }
                        Cursor.Current = Cursors.Default;

                        MessageBox.Show("All icons extracted successfully to " + finalTargetDir, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void MultiSelectBtn_Click(object sender, EventArgs e)
        {
            multiMode = !multiMode;
            if (multiMode)
            {
                multiSelectBtn.Text = "Multi: ON";
                iconList.CheckBoxes = true;
                iconList.MultiSelect = true;
            }
            else
            {
                multiSelectBtn.Text = "Multi: OFF";
                iconList.CheckBoxes = false;
                iconList.MultiSelect = false;
            }
        }

        private void ToggleBtn_Click(object sender, EventArgs e)
        {
            showIDs = !showIDs;
            iconList.BeginUpdate();
            for (int i = 0; i < iconList.Items.Count; i++)
                iconList.Items[i].Text = showIDs ? $"ID: {i}" : "";
            iconList.EndUpdate();
        }

        private void RestoreSizeBtn_Click(object sender, EventArgs e)
        {
            targetZoomSize = 64;
            zoomTimer.Stop();
            zoomTimer.Start();
        }

        private void IconList_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
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

        private void ZoomTimer_Tick(object sender, EventArgs e)
        {
            zoomTimer.Stop();
            bool wasLoading = asyncTimer.Enabled;
            if (wasLoading) asyncTimer.Stop();

            iconList.BeginUpdate();
            imageList.Images.Clear();
            imageList.ImageSize = new Size(targetZoomSize, targetZoomSize);
            foreach (var img in imageCache) imageList.Images.Add(img);
            iconList.EndUpdate();

            if (wasLoading) asyncTimer.Start();
        }
        
        private void ResolveAndLoad()
        {
            try
            {
                if (string.IsNullOrEmpty(targetPath)) return; 
                
                string ext = Path.GetExtension(targetPath).ToLower();
                bool isResourceFile = (ext == ".exe" || ext == ".dll" || ext == ".ico" || ext == ".icl" || ext == ".cpl" || ext == ".mui" || ext == ".mun" || ext == ".scr");

                SHFILEINFO shinfo = new SHFILEINFO();
                IntPtr res = SHGetFileInfo(targetPath, 0, out shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICONLOCATION);
                
                string foundPath = "";
                if (!string.IsNullOrEmpty(shinfo.szDisplayName))
                {
                    foundPath = Environment.ExpandEnvironmentVariables(shinfo.szDisplayName);
                }

                if (isResourceFile)
                {
                    resolvedSourceFile = targetPath;
                    resolvedIconIndex = 0;
                }
                else if (!string.IsNullOrEmpty(foundPath) && File.Exists(foundPath))
                {
                    resolvedSourceFile = foundPath;
                    resolvedIconIndex = shinfo.iIcon;
                }
                else
                {
                    // If no specific icon found, try to find the default handler for this file type
                    string assocPath = GetDefaultHandler(targetPath);
                    if (!string.IsNullOrEmpty(assocPath) && File.Exists(assocPath))
                    {
                        resolvedSourceFile = assocPath;
                        resolvedIconIndex = 0;
                    }
                    else
                    {
                        resolvedSourceFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
                        resolvedIconIndex = 0;
                    }
                }

                pathBox.Text = resolvedSourceFile;
                StartAsyncLoad(resolvedSourceFile);
            }
            catch (Exception ex)
            {
                pathBox.Text = "Error: " + ex.Message;
            }
        }

        private void StartAsyncLoad(string filePath)
        {
            if (!File.Exists(filePath)) return;

            asyncTimer.Stop();
            foreach (var img in imageCache) img.Dispose();
            imageCache.Clear();
            iconList.Items.Clear();
            imageList.Images.Clear();

            totalIcons = EliteIconExtractor.PrivateExtractIconsW(filePath, 0, 0, 0, null, null, 0, 0);
            extractIndex = 0;

            if (totalIcons > 0)
                asyncTimer.Start();
            else
                pathBox.Text = "No icons found in: " + filePath;
        }

        private class IconMeta { public string SourceFile { get; set; } public int Index { get; set; } }

        private void AsyncTimer_Tick(object sender, EventArgs e)
        {
            asyncTimer.Stop();
            int chunkSize = 20; 
            int endIndex = Math.Min(extractIndex + chunkSize, (int)totalIcons);
            int countToExtract = endIndex - extractIndex;

            if (countToExtract <= 0) return;

            IntPtr[] phicon = new IntPtr[countToExtract];
            IntPtr[] piconid = new IntPtr[countToExtract];
            EliteIconExtractor.PrivateExtractIconsW(resolvedSourceFile, extractIndex, 256, 256, phicon, piconid, (uint)countToExtract, 0);

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

                        string text = showIDs ? $"ID: {actualIndex}" : "";
                        var item = new ListViewItem(text, actualIndex);
                        item.Tag = new IconMeta { SourceFile = resolvedSourceFile, Index = actualIndex };
                        iconList.Items.Add(item);

                        if (actualIndex == resolvedIconIndex || actualIndex == Math.Abs(resolvedIconIndex))
                        {
                            item.Selected = true;
                            item.EnsureVisible();
                        }

                        EliteIconExtractor.DestroyIcon(phicon[i]);
                    }
                }
            }
            iconList.EndUpdate();

            extractIndex = endIndex;
            if (extractIndex < totalIcons) asyncTimer.Start();
        }

        private string ExtractToTemp(string source, int index)
        {
            string safeName = Path.GetFileName(source).Replace(".", "_");
            string target = Path.Combine(tempDir, $"{safeName}_IconGroup_{index}.ico");
            if (!File.Exists(target)) EliteIconExtractor.SaveFullIconGroup(source, index, target);
            return target;
        }

        private void IconList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && iconList.SelectedItems.Count > 0)
            {
                var meta = (IconMeta)iconList.SelectedItems[0].Tag;
                currentRightClickFile = ExtractToTemp(meta.SourceFile, meta.Index);
                if (File.Exists(currentRightClickFile)) iconContextMenu.Show(iconList, e.Location);
            }
        }

        private void IconList_DoubleClick(object sender, EventArgs e)
        {
            if (iconList.SelectedItems.Count > 0)
            {
                var meta = (IconMeta)iconList.SelectedItems[0].Tag;
                string physFile = ExtractToTemp(meta.SourceFile, meta.Index);
                if (File.Exists(physFile)) Process.Start(new ProcessStartInfo(physFile) { UseShellExecute = true });
            }
        }

        private void ExtractBtn_Click(object sender, EventArgs e)
        {
            List<ListViewItem> targets = multiMode ? iconList.CheckedItems.Cast<ListViewItem>().ToList() : iconList.SelectedItems.Cast<ListViewItem>().ToList();
            if (targets.Count == 0) return;

            string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            foreach (ListViewItem item in targets)
            {
                var meta = (IconMeta)item.Tag;
                string sourceName = Path.GetFileName(meta.SourceFile);
                string dateStr = DateTime.Now.ToString("yyyyMMdd");
                string finalFile = Path.Combine(desktopDir, $"{sourceName}_IconGroup_{meta.Index}_{dateStr}.ico");
                EliteIconExtractor.SaveFullIconGroup(meta.SourceFile, meta.Index, finalFile);
            }
            
            MessageBox.Show($"Extracted {targets.Count} icon(s) to Desktop.", "Icon Explorer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LaunchAppBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string appExe = Path.Combine(asmDir, "ICON_EXPLORER_APP.exe");
                if (File.Exists(appExe)) Process.Start(appExe, $"\"{resolvedSourceFile}\" ");
                else MessageBox.Show("Full App not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }
}

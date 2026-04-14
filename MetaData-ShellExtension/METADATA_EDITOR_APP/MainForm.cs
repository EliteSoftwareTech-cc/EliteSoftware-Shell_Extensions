#pragma warning disable CS8618, CS8600, CS8602, CS8622, CS8625
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MetadataEditor.Engine;

namespace MetadataEditor.App
{
    public class MainForm : Form
    {
        private TextBox pathBox;
        private Button browseBtn;
        private PropertyGrid propertyGrid;
        private Button editAdsBtn;
        private Button reloadBtn;

        public MainForm(string[] args = null)
        {
            InitializeComponent();
            try { this.Icon = global::System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            
            // Subscribe to schema updates to "fill in" fields as they are discovered
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

            if (args != null && args.Length > 0)
            {
                string argPath = args[0];
                if (argPath.StartsWith("\"") && argPath.EndsWith("\""))
                    argPath = argPath.Substring(1, argPath.Length - 2);
                
                if (File.Exists(argPath) || Directory.Exists(argPath))
                {
                    pathBox.Text = argPath;
                    LoadFileMetadata(argPath);
                }
                else propertyGrid.SelectedObject = new FileMetadataWrapper();
            }
            else propertyGrid.SelectedObject = new FileMetadataWrapper();
        }

        private void InitializeComponent()
        {
            this.Text = "Metadata Editor";
            this.Size = new Size(700, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(500, 500);

            Label pathLabel = new Label { 
                Text = "Target File/Folder:", 
                Location = new Point(10, 15), 
                AutoSize = true
            };
            
            pathBox = new TextBox 
            {
                Location = new Point(140, 12),
                Size = new Size(430, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.FileSystem
            };
            pathBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadFileMetadata(pathBox.Text); };

            browseBtn = new Button 
            {
                Text = "...",
                Location = new Point(580, 10),
                Size = new Size(30, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            browseBtn.Click += BrowseBtn_Click;

            reloadBtn = new Button
            {
                Text = "Reload",
                Location = new Point(615, 10),
                Size = new Size(60, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            reloadBtn.Click += (s, e) => LoadFileMetadata(pathBox.Text);

            propertyGrid = new PropertyGrid
            {
                Location = new Point(10, 45),
                Size = new Size(660, 550),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                PropertySort = PropertySort.Categorized,
                ToolbarVisible = true,
                
                // Rufus-Style Blue & White Styling
                CategoryForeColor = Color.White,
                CategorySplitterColor = Color.MediumBlue,
                LineColor = Color.MediumBlue,
                
                // Description area (Bottom Section) - Reverted to White/Black
                HelpBackColor = Color.White,
                HelpForeColor = Color.Black,
                
                CommandsBackColor = Color.White,
                CommandsForeColor = Color.Black,
                ViewBackColor = Color.White,
                ViewForeColor = Color.Black
            };

            editAdsBtn = new Button
            {
                Text = "Manage ADS (Full Editor)",
                Location = new Point(10, 610),
                Size = new Size(180, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            editAdsBtn.Click += EditAdsBtn_Click;

            Button addCustomAdsBtn = new Button
            {
                Text = "Add Custom ADS Field",
                Location = new Point(200, 610),
                Size = new Size(180, 35),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            addCustomAdsBtn.Click += (s, e) => {
                if (string.IsNullOrEmpty(pathBox.Text)) return;
                string name = Microsoft.VisualBasic.Interaction.InputBox("Enter name for custom ADS field:", "Custom ADS", "NewStream");
                if (!string.IsNullOrWhiteSpace(name))
                {
                    try {
                        AdsEngine.WriteStream(pathBox.Text, name, "");
                        LoadFileMetadata(pathBox.Text);
                    } catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            };

            this.Controls.AddRange(new Control[] { pathLabel, pathBox, browseBtn, reloadBtn, propertyGrid, editAdsBtn, addCustomAdsBtn });
        }

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "All Files|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pathBox.Text = ofd.FileName;
                    LoadFileMetadata(ofd.FileName);
                }
            }
        }

        private void LoadFileMetadata(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || (!File.Exists(path) && !Directory.Exists(path)))
            {
                propertyGrid.SelectedObject = null;
                return;
            }
            
            try
            {
                FileMetadataWrapper wrapper = new FileMetadataWrapper(path);
                propertyGrid.SelectedObject = wrapper;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load metadata:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditAdsBtn_Click(object sender, EventArgs e)
        {
            if (File.Exists(pathBox.Text) || Directory.Exists(pathBox.Text))
            {
                new AdsEditorForm(pathBox.Text).ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select a valid file or directory first to edit ADS.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}

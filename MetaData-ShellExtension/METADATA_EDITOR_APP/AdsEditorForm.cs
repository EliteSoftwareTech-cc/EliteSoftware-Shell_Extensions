#pragma warning disable CS8618
using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using MetadataEditor.Engine;

namespace MetadataEditor.App
{
    public class AdsEditorForm : Form
    {
        private string targetFile;
        private ListBox streamList;
        private TextBox streamContentBox;
        private Button addBtn, saveBtn, deleteBtn;
        private Label statusLabel;

        public AdsEditorForm(string filePath)
        {
            this.targetFile = filePath;
            InitializeComponent();
            try { this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            LoadStreams();
        }

        private void InitializeComponent()
        {
            this.Text = "Metadata & ADS Editor - " + Path.GetFileName(targetFile);
            this.Size = new Size(700, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(600, 400);

            Label lblStreams = new Label { Text = "Alternate Data Streams:", Location = new Point(10, 10), AutoSize = true };
            streamList = new ListBox { Location = new Point(10, 30), Size = new Size(200, 400), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left };
            streamList.SelectedIndexChanged += StreamList_SelectedIndexChanged;

            addBtn = new Button { Text = "Add New Stream", Location = new Point(10, 440), Size = new Size(200, 35), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            addBtn.Click += AddBtn_Click;

            Label lblContent = new Label { Text = "Stream Content:", Location = new Point(220, 10), AutoSize = true };
            streamContentBox = new TextBox
            {
                Location = new Point(220, 30),
                Size = new Size(450, 400),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            saveBtn = new Button { Text = "Save Changes", Location = new Point(220, 440), Size = new Size(220, 35), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            saveBtn.Click += SaveBtn_Click;

            deleteBtn = new Button { Text = "Delete Stream", Location = new Point(450, 440), Size = new Size(220, 35), Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            deleteBtn.Click += DeleteBtn_Click;

            statusLabel = new Label { Text = "Ready", Location = new Point(10, 485), AutoSize = true, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };

            this.Controls.AddRange(new Control[] { lblStreams, streamList, addBtn, lblContent, streamContentBox, saveBtn, deleteBtn, statusLabel });
        }

        private void LoadStreams()
        {
            streamList.Items.Clear();
            streamContentBox.Clear();
            try
            {
                var streams = AdsEngine.EnumerateStreams(targetFile);
                foreach (var stream in streams)
                {
                    streamList.Items.Add(stream.Name);
                }
                statusLabel.Text = $"Found {streams.Count} stream(s).";
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Error: " + ex.Message;
            }
        }

        private void StreamList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (streamList.SelectedItem != null)
            {
                string streamName = streamList.SelectedItem.ToString();
                try
                {
                    streamContentBox.Text = AdsEngine.ReadStream(targetFile, streamName);
                    statusLabel.Text = $"Loaded stream '{streamName}'.";
                }
                catch (Exception ex)
                {
                    streamContentBox.Text = "";
                    statusLabel.Text = "Error reading stream: " + ex.Message;
                }
            }
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            string newStreamName = Microsoft.VisualBasic.Interaction.InputBox("Enter a name for the new Alternate Data Stream:", "New ADS", "NewStream");
            if (!string.IsNullOrWhiteSpace(newStreamName))
            {
                try
                {
                    AdsEngine.WriteStream(targetFile, newStreamName, "");
                    LoadStreams();
                    streamList.SelectedItem = newStreamName;
                    statusLabel.Text = $"Created stream '{newStreamName}'.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to create stream: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (streamList.SelectedItem != null)
            {
                string streamName = streamList.SelectedItem.ToString();
                try
                {
                    AdsEngine.WriteStream(targetFile, streamName, streamContentBox.Text);
                    statusLabel.Text = $"Saved stream '{streamName}'.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save stream: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            if (streamList.SelectedItem != null)
            {
                string streamName = streamList.SelectedItem.ToString();
                DialogResult res = MessageBox.Show($"Are you sure you want to delete the stream '{streamName}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (res == DialogResult.Yes)
                {
                    try
                    {
                        if (AdsEngine.DeleteStream(targetFile, streamName))
                        {
                            statusLabel.Text = $"Deleted stream '{streamName}'.";
                            LoadStreams();
                        }
                        else
                        {
                            statusLabel.Text = $"Failed to delete stream '{streamName}'.";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting stream: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}

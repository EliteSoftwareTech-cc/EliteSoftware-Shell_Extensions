using System;
using System.Drawing;
using System.Windows.Forms;

namespace MetadataEditor.ShellExtension
{
    public class WindowsExperienceDialog : Form
    {
        public bool CreateLocalAccount { get; private set; }
        public string Username { get; private set; }
        public bool SetRegionalOptions { get; private set; }
        public bool DisableDataCollection { get; private set; }
        public bool DisableBitLocker { get; private set; }

        private TextBox userBox;
        private CheckBox localAccountChk;
        private CheckBox regionalChk;
        private CheckBox dataCollectionChk;
        private CheckBox bitLockerChk;

        public WindowsExperienceDialog(string defaultUser = "")
        {
            this.Text = "Customize Windows installation?";
            this.Size = new Size(450, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            PictureBox infoIcon = new PictureBox
            {
                Image = SystemIcons.Question.ToBitmap(),
                Location = new Point(15, 15),
                Size = new Size(32, 32),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            localAccountChk = new CheckBox { Text = "Create a local account with username:", Location = new Point(60, 15), AutoSize = true, Checked = true };
            userBox = new TextBox { Text = defaultUser, Location = new Point(280, 13), Size = new Size(140, 20) };
            
            regionalChk = new CheckBox { Text = "Set regional options to the same values as this user's", Location = new Point(60, 45), AutoSize = true, Checked = true };
            dataCollectionChk = new CheckBox { Text = "Disable data collection (Skip privacy questions)", Location = new Point(60, 75), AutoSize = true, Checked = true };
            bitLockerChk = new CheckBox { Text = "Disable BitLocker automatic device encryption", Location = new Point(60, 105), AutoSize = true, Checked = true };

            Button okBtn = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(250, 160), Size = new Size(80, 30) };
            Button cancelBtn = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(340, 160), Size = new Size(80, 30) };

            this.Controls.AddRange(new Control[] { infoIcon, localAccountChk, userBox, regionalChk, dataCollectionChk, bitLockerChk, okBtn, cancelBtn });
            this.AcceptButton = okBtn;
            this.CancelButton = cancelBtn;

            this.FormClosing += (s, e) => {
                if (this.DialogResult == DialogResult.OK)
                {
                    CreateLocalAccount = localAccountChk.Checked;
                    Username = userBox.Text;
                    SetRegionalOptions = regionalChk.Checked;
                    DisableDataCollection = dataCollectionChk.Checked;
                    DisableBitLocker = bitLockerChk.Checked;
                }
            };
        }
    }
}

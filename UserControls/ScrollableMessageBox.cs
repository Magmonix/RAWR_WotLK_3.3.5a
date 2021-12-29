using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Rawr.UserControls
{
    public partial class ScrollableMessageBox : Form
    {
        public ScrollableMessageBox() {
            InitializeComponent();
        }

        public Font MessageFont {
            set { txtMessage.Font = value; }
            get { return txtMessage.Font; }
        }

        public Color MessageForeColor {
            get { return txtMessage.ForeColor; }
            set { txtMessage.ForeColor  = value; }
        }

        public Color MessageBackColor {
            get { return txtMessage.BackColor; }
            set { txtMessage.BackColor  = value; this.BackColor = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">text inside the message box</param>
        /// <param name="caption">caption on the title bar</param>
        public void Show (string text, string caption) {
            // populate the text box with the message
            txtMessage.Text = text;
            // populate the caption with the passed in caption
            this.Text = caption;
            //Assume OK Button, by default
            ChooseButtons(MessageBoxButtons.OK);
            // the active control should be the OK button
            this.ActiveControl = this.Controls[this.Controls.Count-1];
            this.ShowDialog();
        }

        public void Show (string text, string caption, MessageBoxButtons buttonType) {
            txtMessage.Text = text;
            this.Text = caption;
            //Assume OK Button
            ChooseButtons(buttonType);
            this.ActiveControl = this.Controls[this.Controls.Count-1];
            this.ShowDialog();
        }

        public void ShowFromFile(string filename, string caption, MessageBoxButtons buttonType) {
            // read the file into the message box
            StreamReader sr = new StreamReader(filename);
            txtMessage.Text = sr.ReadToEnd();
            sr.Close();
            this.Text = caption;
            ChooseButtons(buttonType);
            this.ActiveControl = this.Controls[this.Controls.Count-1];
            this.ShowDialog();
        }

        void RemoveButtons() {
            List<Button> buttons = new List<Button>();
            foreach (Control c in this.Controls) {
                if (c is Button) {
                    buttons.Add(c as Button);
                }
            }
            foreach (Button b in buttons) {
                this.Controls.Remove(b);
            }
        }

        void ChooseButtons(MessageBoxButtons buttonType) {
            // remove existing buttons
            RemoveButtons();

            // decide which button set to add from buttonType, and add it
            switch (buttonType) {
                case MessageBoxButtons.OK: AddOKButton(); break;
                case MessageBoxButtons.YesNo: AddYesNoButtons(); break;
                case MessageBoxButtons.OKCancel: AddOkCancelButtons(); break;
                default: AddOKButton(); break; // default is an OK button
            }
        }

        private void AddOKButton() {
            Button btnOK = new Button();
            btnOK.Text = "OK";
            this.Controls.Add(btnOK);
            btnOK.Location = new Point(this.Width / 2 - 35, this.txtMessage.Bottom + 5);
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.Size = new Size(70, 20);
            btnOK.DialogResult = DialogResult.OK;
            this.AcceptButton = btnOK;
        }

        private void AddYesNoButtons() {
            Button btnYes = new Button();
            btnYes.Text = "Yes";
            this.Controls.Add(btnYes);

            // calculate the location of the buttons so that they are centered
            // at the bottom
            btnYes.Location = new Point(this.Width / 2 - 75, this.txtMessage.Bottom + 5);
            btnYes.Size = new Size(70, 20);
            btnYes.DialogResult = DialogResult.Yes;
            this.AcceptButton = btnYes;

            Button btnNo = new Button();
            btnNo.Text = "No";
            this.Controls.Add(btnNo);
            btnNo.Location = new Point(this.Width / 2 + 5, this.txtMessage.Bottom + 5);
            btnNo.Size = new Size(70, 20);
            btnNo.DialogResult = DialogResult.No;
            this.CancelButton = btnNo;
        }

        private void AddOkCancelButtons() {
            Button btnOK = new Button();
            btnOK.Text = "OK";
            this.Controls.Add(btnOK);
            btnOK.Location = new Point(this.Width / 2 - 75, this.txtMessage.Bottom + 5);
            btnOK.Size = new Size(70, 20);
            btnOK.DialogResult = DialogResult.OK;
            this.AcceptButton = btnOK;

            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            this.Controls.Add(btnCancel);
            btnCancel.Location = new Point(this.Width / 2 + 5, this.txtMessage.Bottom + 5);
            btnCancel.Size = new Size(70, 20);
            btnCancel.DialogResult = DialogResult.Cancel;
            this.CancelButton = btnCancel;
        }

        public void Show (string text) {
            txtMessage.Text = text;
            ChooseButtons(MessageBoxButtons.OK);
            this.ShowDialog();
        }

        private void ScrollableMessageBox_Resize(object sender, EventArgs e) {
            // txtMessage.SetBounds(0, 0, this.ClientRectangle.Width - 10, this.Height - 55);
        }

        private void ScrollableMessageBox_Load(object sender, EventArgs e) { }
    }
}
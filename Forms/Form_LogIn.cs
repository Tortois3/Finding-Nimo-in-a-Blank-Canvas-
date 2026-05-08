using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameForms.Forms
{
    public partial class Form_LogIn : Form
    {
        private const string CorrectPin = "analytics";
        private bool passwordVisible = false;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        public Form_LogIn()
        {
            InitializeComponent();
            FormEscapeCloseBehavior.Attach(this);
            this.FormBorderStyle = FormBorderStyle.None;

            AcceptButton = Save;
            CancelButton = Cancel;
            StartPosition = FormStartPosition.CenterParent;

            tBPin.Text = string.Empty;
            tBPin.PasswordChar = '●'; // default hidden

            Save.Click += Save_Click;
            Cancel.Click += Cancel_Click;
            clear.Click += Clear_Click;
            tBPin.KeyDown += TBPin_KeyDown;
            tBPin.TextChanged += tBPin_TextChanged;

            // Wire both eye buttons to toggle
            btnShow.Click += TogglePassword_Click;
            btnHide.Click += TogglePassword_Click;

            btnShow.BringToFront();
            UpdatePasswordButtons();
        }

        private void Save_Click(object? sender, EventArgs e)
        {
            SubmitPin();
        }

        private void Cancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void Clear_Click(object? sender, EventArgs e)
        {
            tBPin.Clear();
            tBPin.Focus();
        }

        private void clear_Click(object sender, EventArgs e)
        {
            Clear_Click(sender, e);
        }

        private void TBPin_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SubmitPin();
            }
        }

        private void SubmitPin()
        {
            string enteredPin = tBPin.Text.Trim();

            if (string.Equals(enteredPin, CorrectPin, StringComparison.Ordinal))
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            tBPin.Clear();
            tBPin.Focus();

            MessageBox.Show(
                "Wrong PIN. Please try again.",
                "Access Denied",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        // Unified toggle for show/hide PIN
        private void TogglePassword_Click(object sender, EventArgs e)
        {
            passwordVisible = !passwordVisible;
            UpdatePasswordButtons();
        }

        private void UpdatePasswordButtons()
        {
            // Show exact text when visible, mask with ● when hidden
            tBPin.PasswordChar = passwordVisible ? '\0' : '●';

            if (passwordVisible)
                btnHide.BringToFront(); // eye-open icon
            else
                btnShow.BringToFront(); // eye-closed icon
        }

        private void tBPin_TextChanged(object sender, EventArgs e)
        {
            clear.ForeColor = string.IsNullOrWhiteSpace(tBPin.Text) ? Color.Peru : Color.Orange;
        }
    }
}

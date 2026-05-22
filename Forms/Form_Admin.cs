using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace GameForms.Forms
{
    public partial class Form_Admin : Form
    {
        private const string DefaultPath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\admininfo.json";
        private bool passwordVisible = false;

        // Define allowed IDs and password
        private readonly string[] AllowedIDs = { "24-1613-660", "241613660" };
        private const string AdminPassword = "cobrado.123456CITU";

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        public Form_Admin()
        {
            InitializeComponent();
            FormLayoutHelper.Configure(this);
            FormEscapeCloseBehavior.Attach(this);
            this.FormBorderStyle = FormBorderStyle.None;

            // Start with password hidden
            tbPassword.PasswordChar = '●';
            btnShow.BringToFront();

            // Wire both buttons to toggle
            btnShow.Click += TogglePassword_Click;
            btnHide.Click += TogglePassword_Click;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            string id = tbID.Text?.Trim() ?? string.Empty;
            string password = tbPassword.Text?.Trim() ?? string.Empty;

            // Validate ID
            if (Array.IndexOf(AllowedIDs, id) < 0)
            {
                MessageBox.Show("Invalid ID. Allowed values: 24-1613-660 or 241613660.",
                                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate password
            if (!string.Equals(password, AdminPassword, StringComparison.Ordinal))
            {
                MessageBox.Show("Wrong Password. Please try again.",
                                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Save to JSON
            var adminInfo = new
            {
                ID = id,
                Password = password,
                Timestamp = DateTime.Now
            };

            try
            {
                string json = JsonSerializer.Serialize(adminInfo, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(DefaultPath, json);

                MessageBox.Show("Admin info saved successfully.",
                                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Hide();
                using Form_Creator nextForm = new Form_Creator();
                nextForm.ShowDialog(this);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save admin info: {ex.Message}",
                                "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void clear_Click(object sender, EventArgs e)
        {
            tbID.Text = "";
            tbPassword.Text = "";
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TogglePassword_Click(object sender, EventArgs e)
        {
            passwordVisible = !passwordVisible;
            tbPassword.PasswordChar = passwordVisible ? '\0' : '●';

            if (passwordVisible)
                btnHide.BringToFront(); // eye-open icon
            else
                btnShow.BringToFront(); // eye-closed icon
        }

        private void btnShow_Click(object sender, EventArgs e)
        {

        }
    }
}

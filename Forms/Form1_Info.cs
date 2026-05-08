using GameForms.Data;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameForms
{
    public partial class Form1_Info : Form
    {
        private const string DefaultDatabasePath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameDialogue.db";
        private static readonly string DefaultUserInfoPath = Path.Combine(AppContext.BaseDirectory, "userinfo.json");
        private const string LegacyUserInfoPath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\userinfo.json";
        private readonly SqlitePlayerRepository _playerRepo;
        private bool passwordVisible = false;

        public bool SaveCancel = false;
        public bool ExistingAccountLoggedIn { get; private set; }
        public UserInfo? CurrentPlayer { get; private set; }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        public IUserRepository IUserRepository
        {
            get => default;
            set
            {
            }
        }

        public Form1_Info() : this(new SqlitePlayerRepository(DefaultDatabasePath)) { }

        public Form1_Info(SqlitePlayerRepository playerRepo)
        {
            _playerRepo = playerRepo ?? throw new ArgumentNullException(nameof(playerRepo));
            InitializeComponent();
            FormEscapeCloseBehavior.Attach(this);
            FormBorderStyle = FormBorderStyle.None;

            // Default: password hidden
            tbPassword.PasswordChar = '●';
            btnShow.BringToFront();

            // Wire both buttons to the same toggle handler
            btnShow.Click += TogglePassword_Click;
            btnHide.Click += TogglePassword_Click;
        }

        private async void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Your information will not be saved. Continue?",
                "CANCEL",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    await SaveEmptyPlayerInfoAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to clear player info: {ex.Message}",
                        "Cancel Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                CurrentPlayer = null;
                PlayerSession.UseEmptyAccount();
                SaveCancel = false;
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private static async Task SaveEmptyPlayerInfoAsync()
        {
            UserInfo emptyUser = new UserInfo();
            await new FileUserRepository(DefaultUserInfoPath).SaveAsync(emptyUser);

            if (!string.Equals(DefaultUserInfoPath, LegacyUserInfoPath, StringComparison.OrdinalIgnoreCase))
                await new FileUserRepository(LegacyUserInfoPath).SaveAsync(emptyUser);
        }

        private async void Ok_Click(object sender, EventArgs e)
        {
            string nickname = tBName.Text?.Trim() ?? string.Empty;
            string rawAge = tbAge.Text?.Trim() ?? string.Empty;
            string password = tbPassword.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(nickname))
            {
                MessageBox.Show("Please enter your name.");
                return;
            }

            if (string.IsNullOrWhiteSpace(rawAge))
            {
                MessageBox.Show("Please enter your age.");
                return;
            }

            if (!int.TryParse(rawAge, System.Globalization.NumberStyles.Integer,
                              System.Globalization.CultureInfo.InvariantCulture, out int age))
            {
                MessageBox.Show("Please enter a valid number for age.");
                return;
            }

            if (age < 6 || age > 60)
            {
                MessageBox.Show("Please enter a valid age between 6 and 60.");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter your password.");
                return;
            }

            try
            {
                (UserInfo user, bool exists) = await _playerRepo.LoginOrCreateAsync(nickname, age, password);

                // Password check: must match nickname
                if (!string.Equals(password, nickname, StringComparison.Ordinal))
                {
                    MessageBox.Show("Wrong Password. Please try again.",
                                    "Validation Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return;
                }

                // Age change warning if existing account has different age
                if (exists && user.Age != age)
                {
                    DialogResult ageResult = MessageBox.Show(
                        "Changing the Age will have a slight effect in the game. Do you wish to continue?",
                        "Age Change Warning",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (ageResult == DialogResult.No)
                    {
                        return; // cancel save
                    }
                }

                CurrentPlayer = user;
                PlayerSession.SetCurrent(user);
                ExistingAccountLoggedIn = exists;
                HistoryLogService.QuickLog("SYSTEM", exists
                    ? "Logged into an existing account."
                    : "Created a new account.");
                SaveCancel = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save player info: {ex.Message}",
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void clear_Click(object sender, EventArgs e)
        {
            tBName.Text = string.Empty;
            tbAge.Text = string.Empty;
            tbPassword.Text = string.Empty;
        }

        private void Form1_Info_Load_1(object sender, EventArgs e)
        {
            tBName.Text = string.Empty;
            tbAge.Text = string.Empty;
            tbPassword.Text = string.Empty;
        }

        // Unified toggle for show/hide password
        private void TogglePassword_Click(object sender, EventArgs e)
        {
            passwordVisible = !passwordVisible;
            UpdatePasswordButtons();
        }

        private void UpdatePasswordButtons()
        {
            tbPassword.PasswordChar = passwordVisible ? '\0' : '●';

            if (passwordVisible)
                btnHide.BringToFront(); // eye-open icon
            else
                btnShow.BringToFront(); // eye-closed icon
        }
    }
}

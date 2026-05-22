using GameForms.Data;
using Microsoft.Data.Sqlite;
using GameForms.Forms;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

namespace GameForms
{
    public partial class Form2 : Form
    {
        private const string DefaultDatabasePath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameDialogue.db";
        private readonly SqlitePlayerRepository _playerRepo;
        private WindowsMediaPlayerHost? _gameIntroPlayer;
        private readonly bool _openMemoirOnShown;
        private readonly int? _startupPlayerId;
        private bool _introInitialized;
        private bool _startupDialogsShown;
        private bool _memoirAutoOpened;
        private bool _memoirAutoOpenScheduled;
        private string _introPath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\intro2.mp4";
        private const string PlayerInfoRequiredMessage = "Enter Player Info to access";

        public Form2() : this(new SqlitePlayerRepository(DefaultDatabasePath), false, null)
        {
        }

        public Form2(SqlitePlayerRepository playerRepo, bool openMemoirOnShown = false, int? startupPlayerId = null)
        {
            _playerRepo = playerRepo ?? throw new ArgumentNullException(nameof(playerRepo));
            _openMemoirOnShown = openMemoirOnShown;
            _startupPlayerId = startupPlayerId;

            InitializeComponent();
            
            memoir.Visible = openMemoirOnShown;
            USERINFO.Visible = true;

            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            Bounds = Screen.PrimaryScreen.Bounds;

            EXIT.TabStop = false;

            Shown += Form2_Shown;
            Activated += Form2_Activated;
            FormClosed += (_, _) => ReleaseIntroPlayer();

            KeyPreview = true;
            KeyDown += Form2_KeyDown;
            Resize += (_, _) => LayoutResponsiveUi();

            AlignPersistentMenuButtons();
            GameIntroHost.SendToBack();
            RestoreFrontUiOrder();

        }

        public Form1_Info Form1_Info
        {
            get => default;
            set
            {
            }
        }

        public Exit Exit
        {
            get => default;
            set
            {
            }
        }

        public Form_About Form_About
        {
            get => default;
            set
            {
            }
        }

        public Form_Welcome Form_Welcome
        {
            get => default;
            set
            {
            }
        }

        public Form4 Form4
        {
            get => default;
            set
            {
            }
        }

        public Form_Achievement Form_Achievement
        {
            get => default;
            set
            {
            }
        }

        public UserInfo UserInfo
        {
            get => default;
            set
            {
            }
        }

        public IUserRepository IUserRepository
        {
            get => default;
            set
            {
            }
        }

        public Form3 Form3
        {
            get => default;
            set
            {
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            _introPath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\intro2.mp4";
        }

        private bool LatestSaveReachedEndingScene()
        {
            if (!PlayerSession.HasActivePlayer)
                return false;

            using var conn = new SqliteConnection($"Data Source={DefaultDatabasePath}");
            conn.Open();

            const string query = @"
                SELECT SceneID
                FROM tblSaveData
                WHERE PlayerID = @playerId
                ORDER BY COALESCE(SavedAt, CURRENT_TIMESTAMP) DESC, SaveID DESC
                LIMIT 1;";

            using var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@playerId", PlayerSession.PlayerId);
            object? result = cmd.ExecuteScalar();
            if (result == null || result == DBNull.Value)
                return false;

            int sceneId = Convert.ToInt32(result);
            return sceneId == 37 || sceneId == 38;
        }

        private bool HasMemoirFeatureUnlocked()
        {
            if (!PlayerSession.HasActivePlayer)
                return false;

            using var conn = new SqliteConnection($"Data Source={DefaultDatabasePath}");
            conn.Open();

            try
            {
                using var sceneAnalyticsCmd = new SqliteCommand(@"
                    SELECT COUNT(1)
                    FROM tblSceneAnalytics
                    WHERE PlayerID = @playerId AND SceneID = 16;", conn);
                sceneAnalyticsCmd.Parameters.AddWithValue("@playerId", PlayerSession.PlayerId);
                object? analyticsResult = sceneAnalyticsCmd.ExecuteScalar();
                if (analyticsResult != null && analyticsResult != DBNull.Value && Convert.ToInt32(analyticsResult) > 0)
                    return true;
            }
            catch (SqliteException)
            {
            }

            using var saveCmd = new SqliteCommand(@"
                SELECT COALESCE(MAX(SceneID), 0)
                FROM tblSaveData
                WHERE PlayerID = @playerId;", conn);
            saveCmd.Parameters.AddWithValue("@playerId", PlayerSession.PlayerId);

            object? saveResult = saveCmd.ExecuteScalar();
            if (saveResult == null || saveResult == DBNull.Value)
                return false;

            return Convert.ToInt32(saveResult) >= 16;
        }

        private void RefreshMemoirButtonVisibility()
        {
            memoir.Visible = _openMemoirOnShown || HasMemoirFeatureUnlocked();
            RestoreFrontUiOrder();
        }

        private async Task UpdateMemoirVisibilityAsync()
        {
            // Ensure latest player state is populated 
            try
            {
                if (!PlayerSession.HasActivePlayer && !PlayerSession.IsEmptyAccount)
                {
                    UserInfo? player = _startupPlayerId.HasValue
                        ? await _playerRepo.LoadPlayerByIdAsync(_startupPlayerId.Value)
                        : await _playerRepo.LoadMostRecentPlayerAsync();

                    if (player != null)
                        PlayerSession.SetCurrent(player);
                }
            }
            catch
            {
            }

            try
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(RefreshMemoirButtonVisibility));
                }
                else
                {
                    RefreshMemoirButtonVisibility();
                }
            }
            catch
            {
            }
        }

        private async Task ShowPlayerInfoOnOpenAsync()
        {
            using var infoForm = new Form1_Info(_playerRepo);
            if (infoForm.ShowDialog(this) != DialogResult.OK)
                return;

            if (infoForm.CurrentPlayer != null)
                PlayerSession.SetCurrent(infoForm.CurrentPlayer);

            RefreshMemoirButtonVisibility();

            if (infoForm.ExistingAccountLoggedIn)
            {
                using Form_Welcome form = new Form_Welcome();
                form.ShowDialog(this);
            }

            await Task.CompletedTask;
        }

        private async void Form2_Shown(object? sender, EventArgs e)
        {
            LayoutResponsiveUi();

            if (!_introInitialized)
            {
                _introInitialized = true;

                // Avoid creating modal startup forms before the main window is actually visible.
                BringToFront();
                Activate();
                BeginInvoke(new Action(async () =>
                {
                    StartIntroLoop();
                    await UpdateMemoirVisibilityAsync();
                }));
            }

            if (_startupDialogsShown)
            {
                await UpdateMemoirVisibilityAsync();
                return;
            }

            _startupDialogsShown = true;

            if (_openMemoirOnShown && !_memoirAutoOpened)
            {
                if (!PlayerSession.HasActivePlayer && !PlayerSession.IsEmptyAccount)
                {
                    UserInfo? player = _startupPlayerId.HasValue
                        ? await _playerRepo.LoadPlayerByIdAsync(_startupPlayerId.Value)
                        : await _playerRepo.LoadMostRecentPlayerAsync();

                    if (player != null)
                        PlayerSession.SetCurrent(player);
                }

                memoir.Visible = true;
                USERINFO.Visible = true;
                RestoreFrontUiOrder();
                await UpdateMemoirVisibilityAsync();
                _memoirAutoOpened = true;

                if (PlayerSession.HasActivePlayer && !_memoirAutoOpenScheduled)
                {
                    _memoirAutoOpenScheduled = true;
                    BeginInvoke(new Action(() =>
                    {
                        BringToFront();
                        Activate();
                        RestoreFrontUiOrder();
                        OpenMemoirDialog(forceOpen: true);
                    }));
                }
                return;
            }

            await ShowPlayerInfoOnOpenAsync();
            await UpdateMemoirVisibilityAsync();
        }

        private async void Form2_Activated(object? sender, EventArgs e)
        {
            await UpdateMemoirVisibilityAsync();
        }

        private void EXIT_Click(object sender, EventArgs e)
        {
            using Exit form = new Exit();
            form.lblMessage = "Do you want to exit?";
            form.ShowDialog();
            if (form.OkCancel)
            {
                ReleaseIntroPlayer();
                Application.Exit();
            }
        }

        private void USERINFO_Click(object sender, EventArgs e)
        {
            using Form1_Info form = new Form1_Info(_playerRepo);
            if (form.ShowDialog(this) == DialogResult.OK && form.CurrentPlayer != null)
            {
                PlayerSession.SetCurrent(form.CurrentPlayer);
                RefreshMemoirButtonVisibility();
            }
        }

        private void ABOUT_Click(object sender, EventArgs e)
        {
            using Form_About form = new Form_About();
            form.ShowDialog(this);
        }

        private void HISTORY_Click(object sender, EventArgs e)
        {
            if (!EnsurePlayerSelected())
                return;

            if (PlayerSession.HasActivePlayer)
                HistoryLogService.QuickLog("SYSTEM", "Opened History menu");

            using History form = new History();
            form.ShowDialog(this);
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (!EnsurePlayerSelected())
                return;

            HistoryLogService.QuickLog("PLAYER", "Started a new game.");

            ReleaseIntroPlayer();
            Hide();

            using (var form3 = new Form3())
            {
                form3.Show();
                form3.BringToFront();
                form3.Activate();
                form3.Refresh();
                Application.DoEvents();
                await Task.Delay(10000);
                form3.Close();
            }

            if (Form4.TryLaunchGame(out string? errorMessage))
            {
                Close();
                return;
            }

            Show();
            BringToFront();
            Activate();
            StartIntroLoop();
            MessageBox.Show(
                errorMessage ?? "Try again.",
                "Game Launch Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            if (!EnsurePlayerSelected())
                return;

            if (LatestSaveReachedEndingScene())
            {
                MessageBox.Show(
                    "You have already reached an Ending. Start Game again to continue.",
                    "Continue Unavailable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            HistoryLogService.QuickLog("PLAYER", "Continued from a saved game.");

            ReleaseIntroPlayer();
            Hide();

            using (var form3 = new Form3())
            {
                form3.Show();
                form3.BringToFront();
                form3.Activate();
                form3.Refresh();
                Application.DoEvents();
                await Task.Delay(10000);
                form3.Close();
            }

            if (Form4.TryLaunchGame(out string? errorMessage, "--continue"))
            {
                Close();
                return;
            }

            Show();
            BringToFront();
            Activate();
            StartIntroLoop();
            MessageBox.Show(
                errorMessage ?? "Try again.",
                "Game Launch Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                DialogResult result = MessageBox.Show(
                    "Proceed to admin access?",
                    "Confirmation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    using Form_Admin adminForm = new Form_Admin();
                    adminForm.ShowDialog(this);
                }

                return;
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                using Exit form = new Exit();
                form.lblMessage = "Do you want to exit?";
                form.ShowDialog();
                if (form.OkCancel)
                {
                    ReleaseIntroPlayer();
                    Application.Exit();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!EnsurePlayerSelected())
                return;

            if (PlayerSession.HasActivePlayer)
                HistoryLogService.QuickLog("PLAYER", "Opened Achievements.");

            // Temporarily release the intro ActiveX player so the achievement
            // form (a top-level window) can appear above the media host. The
            // embedded WindowsMediaPlayer (an AxHost) may otherwise stay on
            // top of other windows.
            bool hadIntroPlayer = _gameIntroPlayer != null;
            try
            {
                ReleaseIntroPlayer();

                using Form_Achievement form = new Form_Achievement();
                form.ShowDialog(this);
            }
            finally
            {
                // Restore the intro if it was running before.
                if (hadIntroPlayer)
                {
                    StartIntroLoop();
                }

                BringToFront();
                Activate();
                RestoreFrontUiOrder();
            }
        }

        private void TryInitializeIntroPlayer()
        {
            try
            {
                _gameIntroPlayer = new WindowsMediaPlayerHost
                {
                    Dock = DockStyle.Fill,
                    BackColor = System.Drawing.Color.Black,
                    Enabled = true,
                    TabStop = false,
                    AutoStart = true,
                    Loop = true,
                    StretchToFit = true,
                    UiMode = "none"
                };

                for (int i = GameIntroHost.Controls.Count - 1; i >= 0; i--)
                {
                    if (GameIntroHost.Controls[i] is WindowsMediaPlayerHost)
                    {
                        Control oldMediaHost = GameIntroHost.Controls[i];
                        GameIntroHost.Controls.RemoveAt(i);
                        oldMediaHost.Dispose();
                    }
                }

                GameIntroHost.Controls.Add(_gameIntroPlayer);
                _gameIntroPlayer.SendToBack();
                RestoreFrontUiOrder();
            }
            catch
            {
                _gameIntroPlayer = null;
                for (int i = GameIntroHost.Controls.Count - 1; i >= 0; i--)
                {
                    if (GameIntroHost.Controls[i] is WindowsMediaPlayerHost)
                    {
                        Control oldMediaHost = GameIntroHost.Controls[i];
                        GameIntroHost.Controls.RemoveAt(i);
                        oldMediaHost.Dispose();
                    }
                }
                GameIntroHost.BackColor = System.Drawing.Color.Black;
                GameIntroHost.SendToBack();
                RestoreFrontUiOrder();
            }
        }

        private void SetIntroUrl(string mediaPath)
        {
            if (_gameIntroPlayer == null)
                return;

            _gameIntroPlayer.URL = mediaPath;
            _gameIntroPlayer.Loop = true;
            _gameIntroPlayer.StretchToFit = true;
            _gameIntroPlayer.UiMode = "none";
        }

        private void SetIntroAutoStart(bool enabled)
        {
            if (_gameIntroPlayer == null)
                return;

            _gameIntroPlayer.AutoStart = enabled;
        }

        private void PlayIntro()
        {
            if (_gameIntroPlayer == null || string.IsNullOrWhiteSpace(_gameIntroPlayer.URL))
                return;

            _gameIntroPlayer.PlayMedia();
        }

        private void StopIntro()
        {
            _gameIntroPlayer?.StopMedia();
        }

        private void StartIntroLoop()
        {
            TryInitializeIntroPlayer();
            SetIntroUrl(_introPath);
            SetIntroAutoStart(true);
            PlayIntro();
        }

        private void ReleaseIntroPlayer()
        {
            if (_gameIntroPlayer == null)
                return;

            try
            {
                _gameIntroPlayer.StopMedia();
                GameIntroHost.Controls.Remove(_gameIntroPlayer);
                _gameIntroPlayer.Dispose();
            }
            catch
            {
            }
            finally
            {
                _gameIntroPlayer = null;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            if (!EnsurePlayerSelected())
                return;

            using var loginForm = new Form_LogIn();
            if (loginForm.ShowDialog(this) != DialogResult.OK)
                return;

            if (PlayerSession.HasActivePlayer)
                HistoryLogService.QuickLog("SYSTEM", "Opened Analytics.");

            using var analyticsForm = new Form_Analytic();
            analyticsForm.ShowDialog(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenMemoirDialog();
        }

        private void OpenMemoirDialog(bool forceOpen = false)
        {
            if (!forceOpen && !PlayerSession.HasActivePlayer)
            {
                MessageBox.Show(
                    PlayerInfoRequiredMessage,
                    "Player Info Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            HistoryLogService.QuickLog("PLAYER", "Opened Memoir.");

            using Form_Memoir form = new Form_Memoir();
            form.ShowDialog(this);
            BringToFront();
            Activate();
            RestoreFrontUiOrder();
        }

        private bool EnsurePlayerSelected()
        {
            if (PlayerSession.HasActivePlayer)
                return true;

            MessageBox.Show(
                PlayerInfoRequiredMessage,
                "Player Info Required",
                MessageBoxButtons.OK,
            MessageBoxIcon.Information);
            return false;
        }

        private void RestoreFrontUiOrder()
        {
            GameIntroHost.SendToBack();
            USERINFO.Visible = true;
            USERINFO.BringToFront();
            HISTORY.BringToFront();
            ABOUT.BringToFront();
            EXIT.BringToFront();
            button3.BringToFront();
            button4.BringToFront();
            button5.BringToFront();
            memoir.BringToFront();
            label1.BringToFront();
        }

        private void AlignPersistentMenuButtons()
        {
            LayoutResponsiveUi();

            if (USERINFO.BackgroundImage == null)
                USERINFO.Text = "i";
        }

        private void LayoutResponsiveUi()
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
                return;

            const int leftMargin = 24;
            const int bottomMargin = 24;
            const int iconSpacing = 12;

            int bottomIconY = Math.Max(16, ClientSize.Height - EXIT.Height - bottomMargin);
            EXIT.Location = new System.Drawing.Point(leftMargin, bottomIconY);
            ABOUT.Location = new System.Drawing.Point(leftMargin, EXIT.Top - ABOUT.Height - iconSpacing);
            HISTORY.Location = new System.Drawing.Point(leftMargin, ABOUT.Top - HISTORY.Height - iconSpacing);
            USERINFO.Location = new System.Drawing.Point(leftMargin, HISTORY.Top - USERINFO.Height - iconSpacing);

            int rightMargin = 36;
            int startLeft = Math.Max(24, ClientSize.Width - button3.Width - rightMargin);
            int achievementLeft = Math.Max(24, ClientSize.Width - button4.Width - rightMargin);

            int stackTop = Math.Max(280, (int)(ClientSize.Height * 0.52f));
            const int primaryToSecondaryGap = 14;
            const int groupGap = 20;

            button3.Left = startLeft;
            button3.Top = stackTop;

            button5.Left = button3.Right - button5.Width;
            button5.Top = button3.Bottom + primaryToSecondaryGap;

            button4.Left = achievementLeft;
            button4.Top = button5.Bottom + groupGap;

            memoir.Left = button4.Right - memoir.Width;
            memoir.Top = button4.Bottom + primaryToSecondaryGap;

            label1.Left = Math.Max(24, ClientSize.Width - label1.Width - rightMargin);
            label1.Top = Math.Max(16, ClientSize.Height - label1.Height - bottomMargin);

            RestoreFrontUiOrder();
        }
    }
}

using System;
using System.Windows.Forms;

namespace GameForms
{
    public partial class Form1 : Form
    {
        private WindowsMediaPlayerHost? _gameIntroPlayer;
        private bool _introInitialized;
        private string _introPath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameIntro.mp4";

        public Form1()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.Bounds = Screen.PrimaryScreen?.Bounds ?? this.Bounds;
            this.ShowInTaskbar = true;
            this.Visible = true;

            // prevent the EXIT button from receiving initial focus via tab order
            EXIT.TabStop = false;

            this.Shown += Form1_Shown;
            this.FormClosed += (_, _) => ReleaseIntroPlayer();

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                StopToForm2();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                using (Exit form = new Exit())
                {
                    form.lblMessage = "Do you want to exit?";
                    form.ShowDialog();
                    if (form.OkCancel)
                    {
                        ReleaseIntroPlayer();
                        Application.Exit();
                    }
                }
            }
        }

        public Form2 Form2
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

        private void Form1_Load(object sender, EventArgs e)
        {
            _introPath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameIntro.mp4";
        }

        private void Form1_Shown(object? sender, EventArgs e)
        {
            if (_introInitialized)
                return;

            _introInitialized = true;
            BeginInvoke(new Action(InitializeAndPlayIntro));
        }

        private void InitializeAndPlayIntro()
        {
            TryInitializeIntroPlayer();
            SetIntroUrl(_introPath);
            SetIntroAutoStart(true);
            PlayIntro();
        }

        private void StopToForm2()
        {
            ReleaseIntroPlayer();
            this.Hide();
            Form2 form2 = new Form2();
            form2.Show();
        }

        private void START_Click(object sender, EventArgs e)
        {
            StopToForm2();
        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {
            StopToForm2();
        }

        private void EXIT_Click(object sender, EventArgs e)
        {
            using (Exit form = new Exit())
            {
                form.lblMessage = "Do you want to exit?";
                form.ShowDialog();
                if (form.OkCancel)
                {
                    ReleaseIntroPlayer();
                    Application.Exit();
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            StopToForm2();
        }

        private void BEGIN_Click(object sender, EventArgs e)
        {
            StopToForm2();
        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                using (Exit form = new Exit())
                {
                    form.lblMessage = "Do you want to exit?";
                    form.ShowDialog();
                    if (form.OkCancel)
                    {
                        ReleaseIntroPlayer();
                        Application.Exit();
                    }
                }
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

                GameIntroHost.Controls.Clear();
                GameIntroHost.Controls.Add(_gameIntroPlayer);
                GameIntroHost.SendToBack();
            }
            catch
            {
                _gameIntroPlayer = null;
                GameIntroHost.Controls.Clear();
                GameIntroHost.BackColor = System.Drawing.Color.Black;
                GameIntroHost.SendToBack();
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
    }
}

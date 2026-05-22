using System;
using System.Windows.Forms;

namespace GameForms
{
    public partial class Form1 : Form
    {
        private WindowsMediaPlayerHost? _gameIntroPlayer;
        private bool _introInitialized;
        private bool _navigatingToForm2;
        private string _introPath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameIntro.mp4";

        public Form1()
        {
            InitializeComponent();
            FormLayoutHelper.Configure(this, allowAutoScroll: false);

            WireStartEverywhere();

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.Bounds = Screen.PrimaryScreen?.Bounds ?? this.Bounds;
            this.ShowInTaskbar = true;
            this.Visible = true;

            // prevent the EXIT button from receiving initial focus via tab order
            EXIT.TabStop = false;

            this.Shown += Form1_Shown;
            this.FormClosed += (_, _) => ReleaseIntroPlayer();
            this.Resize += (_, _) => LayoutResponsiveUi();

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
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
            else
            {
                // Any other key should start Form2
                e.SuppressKeyPress = true;
                e.Handled = true;
                StopToForm2();
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
            LayoutResponsiveUi();

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
            if (_navigatingToForm2)
                return;

            _navigatingToForm2 = true;
            ReleaseIntroPlayer();
            this.Hide();
            Form2 form2 = new Form2();
            form2.FormClosed += (_, _) => Close();
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
                GameIntroHost.Dock = DockStyle.Fill;
                GameIntroHost.Location = System.Drawing.Point.Empty;
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
                GameIntroHost.Dock = DockStyle.Fill;
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

        private void WireStartEverywhere()
        {
            var skipNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "EXIT"
            };

            this.MouseDown -= Control_StartMouseDown;
            this.MouseDown += Control_StartMouseDown;

            WireControlsForStart(this.Controls, skipNames);
        }

        private void WireControlsForStart(Control.ControlCollection controls, HashSet<string> skipNames)
        {
            foreach (Control c in controls)
            {
                try
                {
                    if (string.IsNullOrEmpty(c.Name) || skipNames.Contains(c.Name))
                        continue;

                    c.Click -= Control_StartClick;
                    c.Click += Control_StartClick;

                    c.MouseDown -= Control_StartMouseDown;
                    c.MouseDown += Control_StartMouseDown;

                    if (c.HasChildren)
                        WireControlsForStart(c.Controls, skipNames);
                }
                catch
                {
                }
            }
        }

        private void Control_StartClick(object? sender, EventArgs e)
        {
            StopToForm2();
        }

        private void Control_StartMouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                StopToForm2();
        }

        private void LayoutResponsiveUi()
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
                return;

            GameIntroHost.Dock = DockStyle.Fill;

            const int leftMargin = 24;
            const int bottomMargin = 24;

            EXIT.Location = new System.Drawing.Point(
                leftMargin,
                Math.Max(16, ClientSize.Height - EXIT.Height - bottomMargin));

            START.Left = Math.Max(24, (ClientSize.Width - START.Width) / 2);
            START.Top = Math.Max(80, (int)(ClientSize.Height * 0.32f));

            pictureBox1.Width = Math.Min(ClientSize.Width - 120, Math.Max(800, (int)(ClientSize.Width * 0.62f)));
            pictureBox1.Left = Math.Max(24, (ClientSize.Width - pictureBox1.Width) / 2);
            pictureBox1.Top = START.Bottom - 10;

            BEGIN.Left = Math.Max(24, (ClientSize.Width - BEGIN.Width) / 2);
            BEGIN.Top = pictureBox1.Bottom + 8;

            Controls.SetChildIndex(GameIntroHost, Controls.Count - 1);
            START.BringToFront();
            pictureBox1.BringToFront();
            BEGIN.BringToFront();
            EXIT.BringToFront();
        }
    }
}

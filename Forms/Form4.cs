using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using static System.Windows.Forms.DataFormats;
using GameForms.Data;

namespace GameForms.Forms
{
    [SupportedOSPlatform("windows6.1")]
    public partial class Form4 : Form
    {
        private bool _gameStarted;

        public Form4()
        {
            InitializeComponent();
            FormLayoutHelper.Configure(this, allowAutoScroll: false);
            this.FormBorderStyle = FormBorderStyle.None;

            this.KeyPreview = true;
            this.Shown += Form4_Shown;
            this.KeyDown += Form4_KeyDown;
            gamePanel.Click += GamePanel_Click;
            label.Click -= label_Click;
            label.Click += GamePanel_Click;
        }

        public Panel GamePanel
        {
            get { return gamePanel!; }
        }

        public UserInfo? UserInfo
        {
            get => default;
            set
            {
            }
        }

        private void label_Click(object sender, EventArgs e)
        {
            string path = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\userinfo.json";

            if (!File.Exists(path))
                return;

            string json = File.ReadAllText(path);
            var user = System.Text.Json.JsonSerializer.Deserialize<UserInfo>(json);

            using (Form_Welcome form = new Form_Welcome())
            {
            }
        }

        private void Form4_Shown(object? sender, EventArgs e)
        {
            try
            {
                gamePanel?.Focus();
            }
            catch
            {
            }
        }

        private void Form4_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_gameStarted)
                return;

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                _gameStarted = true;

                LaunchGameProcess();
            }
        }

        private void GamePanel_Click(object? sender, EventArgs e)
        {
            if (_gameStarted)
                return;

            _gameStarted = true;
            LaunchGameProcess();
        }

        private void LaunchGameProcess()
        {
            if (!TryLaunchGame(out string? errorMessage))
            {
                _gameStarted = false;
                MessageBox.Show(
                    errorMessage ?? "Try again.",
                    "Game Launch Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (Owner != null)
                Owner.Close();
            else
                Close();
        }

        public static bool TryLaunchGame(out string? errorMessage, string arguments = "")
        {
            string? gameExePath = ResolveGameExePath();
            if (string.IsNullOrWhiteSpace(gameExePath) || !File.Exists(gameExePath))
            {
                errorMessage = "GameProj.exe could not be found. Build GameProj first, then try again.";
                return false;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = gameExePath,
                    Arguments = arguments,
                    WorkingDirectory = Path.GetDirectoryName(gameExePath)!,
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"The game could not be opened.\n\n{ex.Message}";
                return false;
            }
        }

        private static string? ResolveGameExePath()
        {
            string baseDirectory = AppContext.BaseDirectory;

            string[] candidatePaths =
            {
                Path.Combine(baseDirectory, "GameProj.exe"),
                Path.Combine(baseDirectory, "..", "..", "..", "..", "GameProj", "bin", "Debug", "net8.0-windows", "GameProj.exe"),
                Path.Combine(baseDirectory, "..", "..", "..", "..", "GameProj", "bin", "Release", "net8.0-windows", "GameProj.exe"),
                @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameProj\bin\Debug\net8.0-windows\GameProj.exe",
                @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameProj\bin\Release\net8.0-windows\GameProj.exe",
                @"C:\Users\Tiffany Mae\AppData\Local\FindingNimo\GameProj\Debug\win-x64\GameProj.exe",
                @"C:\Users\Tiffany Mae\AppData\Local\FindingNimo\GameProj\Release\win-x64\GameProj.exe"
            };

            return candidatePaths
                .Select(Path.GetFullPath)
                .FirstOrDefault(File.Exists);
        }
    }
}

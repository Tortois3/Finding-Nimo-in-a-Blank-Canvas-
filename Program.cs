using System;
using System.Windows.Forms;
using GameForms.Data;

namespace GameForms
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) =>
            {
                MessageBox.Show(
                    $"Startup error: {e.Exception.Message}",
                    "GameForms Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            try
            {
                int? startupPlayerId = ParsePlayerId(args);
                // If caller requested to open the memoir directly, launch Form2 and auto-open memoir.
                // This is used when the game (GameProj) directs the user to the memoir via --memoir.
                bool openMemoir = args != null && args.Length > 0 && Array.Exists(args, a => string.Equals(a, "--memoir", StringComparison.OrdinalIgnoreCase));
                if (openMemoir)
                {
                    // Use the same database path as Form2's default so player lookup works as expected.
                    var repo = new GameForms.Data.SqlitePlayerRepository(@"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameDialogue.db");
                    Application.Run(new Form2(repo, openMemoirOnShown: true, startupPlayerId: startupPlayerId));
                }
                else
                {
                    // Start with Form1 (intro) so users see the intro/splash first
                    Application.Run(new Form1());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Startup error: {ex.Message}",
                    "GameForms Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static int? ParsePlayerId(string[] args)
        {
            if (args == null || args.Length == 0)
                return null;

            foreach (string arg in args)
            {
                if (!arg.StartsWith("--playerId=", StringComparison.OrdinalIgnoreCase))
                    continue;

                string rawValue = arg.Substring("--playerId=".Length);
                if (int.TryParse(rawValue, out int playerId) && playerId > 0)
                    return playerId;
            }

            return null;
        }
    }
}

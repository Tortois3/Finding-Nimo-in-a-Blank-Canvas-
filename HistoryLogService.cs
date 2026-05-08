using System;
using System.IO;

namespace GameForms
{
    internal static class HistoryLogService
    {
        private static readonly object SyncRoot = new();

        public static void QuickLog(string actor, string action)
        {
            if (!PlayerSession.HasActivePlayer || string.IsNullOrWhiteSpace(action))
                return;

            string logPath = PlayerSession.GetHistoryLogPath();
            string? directory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string date = DateTime.Now.ToString("MM-dd-yyyy");
            string time = DateTime.Now.ToString("HH:mm:ss");

            lock (SyncRoot)
            {
                using var fs = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                using var sw = new StreamWriter(fs);
                sw.WriteLine($"[ {date} | {time} ]");
                sw.WriteLine($"   <{actor}> --- {action}");
                sw.WriteLine();
            }
        }

        public static string ReadHistory()
        {
            if (!PlayerSession.HasActivePlayer)
                return "No player data yet.";

            string logPath = PlayerSession.GetHistoryLogPath();
            if (!File.Exists(logPath))
                return "No history recorded yet.";

            lock (SyncRoot)
            {
                return File.ReadAllText(logPath);
            }
        }

        public static void Clear()
        {
            if (!PlayerSession.HasActivePlayer)
                return;

            string logPath = PlayerSession.GetHistoryLogPath();
            string? directory = Path.GetDirectoryName(logPath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            lock (SyncRoot)
            {
                File.WriteAllText(logPath, string.Empty);
            }
        }
    }
}

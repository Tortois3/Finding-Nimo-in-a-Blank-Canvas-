using System;
using System.IO;
using GameForms.Data;

namespace GameForms
{
    internal static class PlayerSession
    {
        public static UserInfo? CurrentPlayer { get; private set; }

        public static bool HasActivePlayer => CurrentPlayer != null && CurrentPlayer.PlayerID > 0;

        public static bool IsEmptyAccount { get; private set; }

        public static int PlayerId => CurrentPlayer?.PlayerID ?? 0;

        public static string PlayerName => CurrentPlayer?.Nickname ?? string.Empty;

        public static void SetCurrent(UserInfo? player)
        {
            CurrentPlayer = player;
            IsEmptyAccount = false;
        }

        public static void UseEmptyAccount()
        {
            CurrentPlayer = new UserInfo();
            IsEmptyAccount = true;
        }

        public static void Clear()
        {
            CurrentPlayer = null;
            IsEmptyAccount = false;
        }

        public static string GetPlayerStorageDirectory()
        {
            string safePlayerName = string.IsNullOrWhiteSpace(PlayerName) ? "guest" : SanitizeSegment(PlayerName);
            string root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FindingNimo",
                "Players",
                $"{PlayerId}_{safePlayerName}");
            Directory.CreateDirectory(root);
            return root;
        }

        public static string GetMemoirFilePath()
        {
            return Path.Combine(GetPlayerStorageDirectory(), "memoir.json");
        }

        public static string GetHistoryLogPath()
        {
            return Path.Combine(GetPlayerStorageDirectory(), "history.log");
        }

        private static string SanitizeSegment(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '_');

            return value.Trim();
        }
    }
}

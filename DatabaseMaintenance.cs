using Microsoft.Data.Sqlite;
using System;

namespace GameForms
{
    internal static class DatabaseMaintenance
    {
        private const string GameDatabasePath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameDialogue.db";

        public static void ResetProgressAndAnalytics(int playerId)
        {
            using SqliteConnection conn = new($"Data Source={GameDatabasePath}");
            conn.Open();

            using SqliteTransaction transaction = conn.BeginTransaction();

            ExecuteNonQuery(conn, transaction, "DELETE FROM tblSaveData WHERE PlayerID = @playerId;", playerId);
            ExecuteNonQuery(conn, transaction, "DELETE FROM tblSceneAnalytics WHERE PlayerID = @playerId;", playerId);
            ExecuteNonQuery(conn, transaction, "DELETE FROM tblActionAnalytics WHERE PlayerID = @playerId;", playerId);
            ExecuteNonQuery(conn, transaction, "DELETE FROM tblAttempts WHERE PlayerID = @playerId;", playerId);
            ExecuteNonQuery(conn, transaction, "DELETE FROM tblUnlockedEnding WHERE PlayerID = @playerId;", playerId);
            ExecuteNonQuery(conn, transaction, "DELETE FROM tblAnalytics WHERE PlayerID = @playerId;", playerId);

            ExecuteNonQuery(conn, transaction, @"
                UPDATE tblAchievements
                SET SaveID = NULL,
                    TimesUnlocked = 0,
                    LastUnlockedAt = NULL
                WHERE PlayerID = @playerId;", playerId);

            transaction.Commit();
        }

        private static void ExecuteNonQuery(SqliteConnection conn, SqliteTransaction transaction, string sql, int playerId)
        {
            try
            {
                using SqliteCommand cmd = new(sql, conn, transaction);
                cmd.Parameters.AddWithValue("@playerId", playerId);
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 1)
            {
                // Ignore missing tables so reset works across older/newer schema versions.
            }
        }
    }
}

using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace GameForms.Data
{
    public interface IUserRepository
    {
        Task<UserInfo?> LoadAsync();
        Task SaveAsync(UserInfo user);
    }

    public class FileUserRepository : IUserRepository
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

        public FileUserRepository(string path) => _path = path;

        public IUserRepository IUserRepository
        {
            get => default;
            set
            {
            }
        }

        public async Task<UserInfo?> LoadAsync()
        {
            if (!File.Exists(_path)) return null;
            await using var stream = File.OpenRead(_path);
            return await JsonSerializer.DeserializeAsync<UserInfo>(stream, _options);
        }

        public async Task SaveAsync(UserInfo user)
        {
            var dir = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            await using var stream = File.Create(_path);
            await JsonSerializer.SerializeAsync(stream, user, _options);
            await stream.FlushAsync();
        }
    }

    public sealed class SqlitePlayerRepository
    {
        private readonly string _databasePath;

        public SqlitePlayerRepository(string databasePath) => _databasePath = databasePath;

        public async Task EnsureTableAsync()
        {
            var dir = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            await using var conn = new SqliteConnection($"Data Source={_databasePath}");
            await conn.OpenAsync();

            const string createSql = @"
                CREATE TABLE IF NOT EXISTS tblPlayer (
                    PlayerID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Age INTEGER NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    LastLoginAt TEXT
                );";

            await using (var createCmd = new SqliteCommand(createSql, conn))
                await createCmd.ExecuteNonQueryAsync();

            await EnsureColumnExistsAsync(conn, "tblPlayer", "Age", "ALTER TABLE tblPlayer ADD COLUMN Age INTEGER NOT NULL DEFAULT 10;");
            await EnsureColumnExistsAsync(conn, "tblPlayer", "PasswordHash", "ALTER TABLE tblPlayer ADD COLUMN PasswordHash TEXT NOT NULL DEFAULT '';");
            await EnsureColumnExistsAsync(conn, "tblPlayer", "CreatedAt", "ALTER TABLE tblPlayer ADD COLUMN CreatedAt TEXT;");
            await EnsureColumnExistsAsync(conn, "tblPlayer", "LastLoginAt", "ALTER TABLE tblPlayer ADD COLUMN LastLoginAt TEXT;");
        }

        public async Task<(UserInfo User, bool Exists)> LoginOrCreateAsync(string nickname, int age, string password)
        {
            await EnsureTableAsync();
            string passwordHash = HashPassword(password);

            await using var conn = new SqliteConnection($"Data Source={_databasePath}");
            await conn.OpenAsync();

            const string selectSql = @"
                SELECT PlayerID, Name, Age
                FROM tblPlayer
                WHERE Name = @name AND PasswordHash = @passwordHash
                ORDER BY PlayerID DESC
                LIMIT 1;";

            await using var selectCmd = new SqliteCommand(selectSql, conn);
            selectCmd.Parameters.AddWithValue("@name", nickname);
            selectCmd.Parameters.AddWithValue("@passwordHash", passwordHash);

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                int playerId = reader.GetInt32(0);
                string existingName = reader.GetString(1);
                int existingAge = reader.GetInt32(2);
                await reader.CloseAsync();

                const string updateSql = "UPDATE tblPlayer SET LastLoginAt = CURRENT_TIMESTAMP WHERE PlayerID = @playerId;";
                await using var updateCmd = new SqliteCommand(updateSql, conn);
                updateCmd.Parameters.AddWithValue("@playerId", playerId);
                await updateCmd.ExecuteNonQueryAsync();

                return (new UserInfo { PlayerID = playerId, Nickname = existingName, Age = existingAge, Password = password }, true);
            }

            await reader.CloseAsync();

            const string existingNameSql = @"
                SELECT 1
                FROM tblPlayer
                WHERE Name = @name
                LIMIT 1;";

            await using (var existingNameCmd = new SqliteCommand(existingNameSql, conn))
            {
                existingNameCmd.Parameters.AddWithValue("@name", nickname);
                object? nameExists = await existingNameCmd.ExecuteScalarAsync();
                if (nameExists != null && nameExists != DBNull.Value)
                    throw new InvalidOperationException("Name already taken. Change it to continue");
            }

            const string insertSql = @"
                INSERT INTO tblPlayer (Name, Age, PasswordHash, CreatedAt, LastLoginAt)
                VALUES (@name, @age, @passwordHash, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);";

            await using var insertCmd = new SqliteCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("@name", nickname);
            insertCmd.Parameters.AddWithValue("@age", age);
            insertCmd.Parameters.AddWithValue("@passwordHash", passwordHash);
            await insertCmd.ExecuteNonQueryAsync();

            long createdPlayerId;
            await using (var lastIdCmd = new SqliteCommand("SELECT last_insert_rowid();", conn))
                createdPlayerId = (long)(await lastIdCmd.ExecuteScalarAsync() ?? 0L);
            return (new UserInfo { PlayerID = (int)createdPlayerId, Nickname = nickname, Age = age, Password = password }, false);
        }

        public async Task<UserInfo?> LoadMostRecentPlayerAsync()
        {
            await EnsureTableAsync();

            await using var conn = new SqliteConnection($"Data Source={_databasePath}");
            await conn.OpenAsync();

            const string selectSql = @"
                SELECT PlayerID, Name, Age
                FROM tblPlayer
                ORDER BY
                    CASE WHEN LastLoginAt IS NULL THEN 1 ELSE 0 END,
                    LastLoginAt DESC,
                    PlayerID DESC
                LIMIT 1;";

            await using var selectCmd = new SqliteCommand(selectSql, conn);
            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new UserInfo
            {
                PlayerID = reader.GetInt32(0),
                Nickname = reader.GetString(1),
                Age = reader.GetInt32(2),
                Password = string.Empty
            };
        }

        public async Task<UserInfo?> LoadPlayerByIdAsync(int playerId)
        {
            await EnsureTableAsync();

            if (playerId <= 0)
                return null;

            await using var conn = new SqliteConnection($"Data Source={_databasePath}");
            await conn.OpenAsync();

            const string selectSql = @"
                SELECT PlayerID, Name, Age
                FROM tblPlayer
                WHERE PlayerID = @playerId
                LIMIT 1;";

            await using var selectCmd = new SqliteCommand(selectSql, conn);
            selectCmd.Parameters.AddWithValue("@playerId", playerId);
            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new UserInfo
            {
                PlayerID = reader.GetInt32(0),
                Nickname = reader.GetString(1),
                Age = reader.GetInt32(2),
                Password = string.Empty
            };
        }

        private static async Task EnsureColumnExistsAsync(SqliteConnection conn, string tableName, string columnName, string alterSql)
        {
            await using var pragma = new SqliteCommand($"PRAGMA table_info({tableName});", conn);
            await using var reader = await pragma.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (string.Equals(reader["name"]?.ToString(), columnName, System.StringComparison.OrdinalIgnoreCase))
                    return;
            }

            await reader.CloseAsync();

            await using var alter = new SqliteCommand(alterSql, conn);
            await alter.ExecuteNonQueryAsync();
        }

        private static string HashPassword(string password)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return System.BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}

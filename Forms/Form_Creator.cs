using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GameForms.Forms
{
    public partial class Form_Creator : Form
    {
        private const string GameDatabasePath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameDialogue.db";
        private const int EndingSceneStart = 37;

        private readonly Panel ratingsPanel = new();
        private readonly ListBox suggestionList = new();
        private readonly Button closeButton = new();
        private readonly ComboBox playerComboBox = new();
        private readonly TextBox playerSummaryBox = new();
        private readonly Button refreshButton = new();
        private List<RatingBucket> ratingBuckets = new();
        private List<PlayerOption> playerOptions = new();
        private bool loadingPlayers;

        private Panel ratingsContainer = new();
        private Panel suggestionsContainer = new();
        private Label noDataLabel = new();

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        public Form_Creator()
        {
            InitializeComponent();
            FormLayoutHelper.Configure(this, allowAutoScroll: false);
            FormEscapeCloseBehavior.Attach(this);
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;

            BuildCreatorUi();
            Load += Form_Creator_Load;
        }

        private void Form_Creator_Load(object? sender, EventArgs e)
        {
            LoadCreatorData();
        }

        private void BuildCreatorUi()
        {
            BackColor = Color.Black;

            label1.Text = "CREATOR VIEW";
            label1.Location = new Point(28, 18);
            label1.Font = new Font("Pixel Operator SC", 40f, FontStyle.Bold);
            label1.AutoSize = true;
            label1.BringToFront();

            closeButton.Text = "CLOSE";
            closeButton.Font = new Font("Pixel Operator SC", 22f, FontStyle.Bold);
            closeButton.BackColor = Color.Black;
            closeButton.ForeColor = Color.Chartreuse;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Size = new Size(128, 46);
            closeButton.Location = new Point(ClientSize.Width - 164, 24);
            closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            closeButton.Click += (_, _) => Close();
            Controls.Add(closeButton);

            refreshButton.Text = "REFRESH";
            refreshButton.Font = new Font("Pixel Operator SC", 18f, FontStyle.Bold);
            refreshButton.BackColor = Color.White;
            refreshButton.ForeColor = Color.Black;
            refreshButton.FlatStyle = FlatStyle.Flat;
            refreshButton.Size = new Size(132, 42);
            refreshButton.Location = new Point(ClientSize.Width - 312, 26);
            refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refreshButton.Click += (_, _) => LoadCreatorData();
            Controls.Add(refreshButton);

            ratingsContainer = new Panel
            {
                BackColor = Color.Black,
                Location = new Point(36, 110),
                Size = new Size(560, ClientSize.Height - 150),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            Label ratingsTitle = new()
            {
                Text = "RATINGS",
                Dock = DockStyle.Top,
                Height = 48,
                ForeColor = Color.Chartreuse,
                Font = new Font("Pixel Operator SC", 28f, FontStyle.Bold)
            };

            ratingsPanel.Dock = DockStyle.Fill;
            ratingsPanel.BackColor = Color.FromArgb(12, 12, 12);
            ratingsPanel.Paint += RatingsPanel_Paint;

            ratingsContainer.Controls.Add(ratingsPanel);
            ratingsContainer.Controls.Add(ratingsTitle);

            suggestionsContainer = new Panel
            {
                BackColor = Color.Black,
                Location = new Point(630, 110),
                Size = new Size(ClientSize.Width - 666, ClientSize.Height - 150),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            TableLayoutPanel suggestionsLayout = new()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                ColumnCount = 1,
                RowCount = 4
            };
            suggestionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            suggestionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            suggestionsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            suggestionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 260));

            Label suggestionsTitle = new()
            {
                Text = "SUGGESTIONS",
                Dock = DockStyle.Fill,
                ForeColor = Color.Chartreuse,
                Font = new Font("Pixel Operator SC", 28f, FontStyle.Bold)
            };

            playerComboBox.Dock = DockStyle.Fill;
            playerComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            playerComboBox.BackColor = Color.FromArgb(12, 12, 12);
            playerComboBox.ForeColor = Color.WhiteSmoke;
            playerComboBox.Font = new Font("Pixel Operator Mono", 16f, FontStyle.Regular);
            playerComboBox.SelectedIndexChanged += PlayerComboBox_SelectedIndexChanged;

            suggestionList.Dock = DockStyle.Fill;
            suggestionList.BackColor = Color.FromArgb(12, 12, 12);
            suggestionList.ForeColor = Color.WhiteSmoke;
            suggestionList.BorderStyle = BorderStyle.None;
            suggestionList.Font = new Font("Pixel Operator Mono", 18f, FontStyle.Regular);
            suggestionList.IntegralHeight = false;
            suggestionList.HorizontalScrollbar = true;

            playerSummaryBox.Dock = DockStyle.Fill;
            playerSummaryBox.BackColor = Color.FromArgb(12, 12, 12);
            playerSummaryBox.ForeColor = Color.WhiteSmoke;
            playerSummaryBox.BorderStyle = BorderStyle.FixedSingle;
            playerSummaryBox.Font = new Font("Pixel Operator Mono", 15f, FontStyle.Regular);
            playerSummaryBox.Multiline = true;
            playerSummaryBox.ReadOnly = true;
            playerSummaryBox.ScrollBars = ScrollBars.Vertical;

            suggestionsLayout.Controls.Add(suggestionsTitle, 0, 0);
            suggestionsLayout.Controls.Add(playerComboBox, 0, 1);
            suggestionsLayout.Controls.Add(suggestionList, 0, 2);
            suggestionsLayout.Controls.Add(playerSummaryBox, 0, 3);

            noDataLabel.Text = "No Data Yet";
            noDataLabel.Dock = DockStyle.Fill;
            noDataLabel.TextAlign = ContentAlignment.MiddleCenter;
            noDataLabel.Font = new Font("Pixel Operator SC", 28f, FontStyle.Bold);
            noDataLabel.ForeColor = Color.Chartreuse;
            noDataLabel.Visible = false;

            suggestionsContainer.Controls.Add(suggestionsLayout);
            suggestionsContainer.Controls.Add(noDataLabel);

            Controls.Add(ratingsContainer);
            Controls.Add(suggestionsContainer);
        }

        private void LoadCreatorData()
        {
            using var conn = OpenConnection();
            EnsureAdminTableExists(conn);

            playerOptions = LoadPlayerOptions(conn);
            loadingPlayers = true;
            playerComboBox.BeginUpdate();
            playerComboBox.Items.Clear();
            playerComboBox.Items.Add(PlayerOption.AllPlayers);
            foreach (PlayerOption player in playerOptions)
                playerComboBox.Items.Add(player);
            playerComboBox.EndUpdate();

            if (playerComboBox.SelectedIndex < 0)
                playerComboBox.SelectedIndex = 0;
            loadingPlayers = false;

            LoadAdminFeedback(conn, SelectedPlayerId);
            LoadSelectedPlayerSummary(conn, SelectedPlayerId);
        }

        private int? SelectedPlayerId => playerComboBox.SelectedItem is PlayerOption player ? player.PlayerId : null;

        private void PlayerComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (loadingPlayers)
                return;

            using var conn = OpenConnection();
            EnsureAdminTableExists(conn);
            LoadAdminFeedback(conn, SelectedPlayerId);
            LoadSelectedPlayerSummary(conn, SelectedPlayerId);
        }

        private void LoadAdminFeedback(SqliteConnection conn, int? playerId)
        {
            ratingBuckets = Enumerable.Range(0, 11)
                .Select(value => new RatingBucket { Label = value.ToString(), Count = 0 })
                .ToList();

            using (var ratingsCmd = new SqliteCommand(@"
                SELECT Rating, COUNT(1)
                FROM tblAdmin
                WHERE Rating IS NOT NULL
                  AND TRIM(Rating) <> ''
                  AND (@playerId IS NULL OR PlayerID = @playerId)
                GROUP BY Rating;", conn))
            {
                ratingsCmd.Parameters.AddWithValue("@playerId", playerId.HasValue ? playerId.Value : DBNull.Value);
                using var reader = ratingsCmd.ExecuteReader();
                while (reader.Read())
                {
                    string rating = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
                    int count = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    if (int.TryParse(rating, NumberStyles.Integer, CultureInfo.InvariantCulture, out int r) && r >= 0 && r <= 10)
                        ratingBuckets[r].Count = count;
                }
            }

            suggestionList.BeginUpdate();
            suggestionList.Items.Clear();

            string orderByColumn = TableHasColumn(conn, "tblAdmin", "AdminID") ? "AdminID" : "ROWID";
            using (var suggestionsCmd = new SqliteCommand($@"
                SELECT COALESCE(NULLIF(TRIM(PlayerName), ''), 'Unknown Player') AS DisplayName,
                       Suggestion
                FROM tblAdmin
                WHERE Suggestion IS NOT NULL
                  AND TRIM(Suggestion) <> ''
                  AND (@playerId IS NULL OR PlayerID = @playerId)
                ORDER BY {orderByColumn} DESC;", conn))
            {
                suggestionsCmd.Parameters.AddWithValue("@playerId", playerId.HasValue ? playerId.Value : DBNull.Value);
                using var reader = suggestionsCmd.ExecuteReader();
                while (reader.Read())
                {
                    string playerName = reader.IsDBNull(0) ? "Unknown Player" : reader.GetString(0).Trim();
                    string suggestion = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
                    suggestionList.Items.Add(playerId.HasValue ? suggestion : $"{playerName}: {suggestion}");
                }
            }

            suggestionList.EndUpdate();
            suggestionList.BringToFront();

            bool hasAnyRatings = ratingBuckets.Any(b => b.Count > 0);
            bool hasAnySuggestions = suggestionList.Items.Count > 0;
            bool hasPlayers = playerComboBox.Items.Count > 1;
            bool hasAnyData = hasAnyRatings || hasAnySuggestions || hasPlayers;

            noDataLabel.Visible = !hasAnyData;
            suggestionList.Visible = hasAnyData;
            ratingsPanel.Visible = hasAnyData;
            playerSummaryBox.Visible = hasAnyData;
            playerComboBox.Visible = hasAnyData;

            ratingsPanel.Invalidate();
        }

        private void LoadSelectedPlayerSummary(SqliteConnection conn, int? playerId)
        {
            PlayerAnalyticsSummary summary = LoadPlayerAnalyticsSummary(conn, playerId);
            playerSummaryBox.Text = FormatPlayerSummary(summary);
        }

        private static List<PlayerOption> LoadPlayerOptions(SqliteConnection conn)
        {
            Dictionary<int, string> players = new();

            AddPlayersFromQuery(conn, players, "tblPlayer", @"
                SELECT PlayerID, Name
                FROM tblPlayer
                WHERE PlayerID > 0;");

            AddPlayersFromQuery(conn, players, "tblAdmin", @"
                SELECT PlayerID, PlayerName
                FROM tblAdmin
                WHERE PlayerID IS NOT NULL AND PlayerID > 0;");

            AddPlayersFromQuery(conn, players, "tblSceneAnalytics", @"
                SELECT PlayerID, PlayerName
                FROM tblSceneAnalytics
                WHERE PlayerID > 0;");

            AddPlayersFromQuery(conn, players, "tblSaveData", @"
                SELECT PlayerID, PlayerName
                FROM tblSaveData
                WHERE PlayerID > 0;");

            AddPlayersFromQuery(conn, players, "tblUnlockedEnding", @"
                SELECT PlayerID, PlayerName
                FROM tblUnlockedEnding
                WHERE PlayerID > 0;");

            return players
                .Select(kv => new PlayerOption(kv.Key, string.IsNullOrWhiteSpace(kv.Value) ? $"Player {kv.Key}" : kv.Value))
                .OrderBy(player => player.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(player => player.PlayerId)
                .ToList();
        }

        private static void AddPlayersFromQuery(SqliteConnection conn, Dictionary<int, string> players, string tableName, string sql)
        {
            if (!TableExists(conn, tableName))
                return;

            try
            {
                using var cmd = new SqliteCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int playerId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    if (playerId <= 0)
                        continue;

                    string playerName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();
                    if (!players.ContainsKey(playerId) || !string.IsNullOrWhiteSpace(playerName))
                        players[playerId] = playerName;
                }
            }
            catch (SqliteException)
            {
            }
        }

        private static PlayerAnalyticsSummary LoadPlayerAnalyticsSummary(SqliteConnection conn, int? playerId)
        {
            PlayerAnalyticsSummary summary = new()
            {
                PlayerName = playerId.HasValue ? $"Player {playerId.Value}" : "All Players"
            };

            if (playerId.HasValue)
            {
                summary.PlayerId = playerId.Value;
                summary.PlayerName = GetPlayerName(conn, playerId.Value);
            }

            if (TableExists(conn, "tblAttempts"))
            {
                using var attemptsCmd = new SqliteCommand(@"
                    SELECT COUNT(DISTINCT AttemptNumber)
                    FROM tblAttempts
                    WHERE @playerId IS NULL OR PlayerID = @playerId;", conn);
                attemptsCmd.Parameters.AddWithValue("@playerId", playerId.HasValue ? playerId.Value : DBNull.Value);
                summary.AttemptCount = Convert.ToInt32(attemptsCmd.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture);
            }

            List<SceneMetric> scenes = LoadSceneMetrics(conn, playerId);
            summary.SceneLogCount = scenes.Count;
            summary.TotalSeconds = scenes.Sum(scene => scene.DurationSeconds);

            if (summary.AttemptCount == 0)
                summary.AttemptCount = scenes.Select(scene => scene.AttemptNumber).Distinct().Count();

            summary.SkimmedScene = FindTopScene(scenes, "Skimmed");
            summary.SkippedScene = FindTopScene(scenes, "Ignored", "Idle");
            summary.CompletedScene = FindTopScene(scenes, "Read", "Explored");

            int maxSceneId = scenes.Count == 0 ? 0 : scenes.Max(scene => scene.SceneId);
            maxSceneId = Math.Max(maxSceneId, GetMaxSavedSceneId(conn, playerId));
            summary.EndingsAchieved = CountEndingsAchieved(conn, playerId);
            summary.ReachedEnding = HasReachedEnding(conn, playerId, maxSceneId);
            summary.ProgressPercent = EstimateProgressPercent(maxSceneId, summary.ReachedEnding, scenes);

            if (TableExists(conn, "tblAdmin"))
            {
                using var feedbackCmd = new SqliteCommand(@"
                    SELECT COUNT(1),
                           AVG(CASE WHEN Rating GLOB '[0-9]*' THEN CAST(Rating AS REAL) ELSE NULL END)
                    FROM tblAdmin
                    WHERE @playerId IS NULL OR PlayerID = @playerId;", conn);
                feedbackCmd.Parameters.AddWithValue("@playerId", playerId.HasValue ? playerId.Value : DBNull.Value);
                using var reader = feedbackCmd.ExecuteReader();
                if (reader.Read())
                {
                    summary.FeedbackCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                    summary.AverageRating = reader.IsDBNull(1) ? null : reader.GetDouble(1);
                }
            }

            return summary;
        }

        private static List<SceneMetric> LoadSceneMetrics(SqliteConnection conn, int? playerId)
        {
            List<SceneMetric> scenes = new();
            if (!TableExists(conn, "tblSceneAnalytics"))
                return scenes;

            using var cmd = new SqliteCommand(@"
                SELECT PlayerID, PlayerName, AttemptNumber, SceneID, SceneName, DurationSeconds, Classification
                FROM tblSceneAnalytics
                WHERE @playerId IS NULL OR PlayerID = @playerId;", conn);
            cmd.Parameters.AddWithValue("@playerId", playerId.HasValue ? playerId.Value : DBNull.Value);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                scenes.Add(new SceneMetric
                {
                    PlayerId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                    PlayerName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    AttemptNumber = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    SceneId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    SceneName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    DurationSeconds = reader.IsDBNull(5) ? 0d : reader.GetDouble(5),
                    Classification = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                });
            }

            return scenes;
        }

        private static SceneSummary FindTopScene(List<SceneMetric> scenes, params string[] classifications)
        {
            HashSet<string> targets = new(classifications, StringComparer.OrdinalIgnoreCase);
            return scenes
                .Where(scene => targets.Contains(scene.Classification))
                .GroupBy(scene => new { scene.SceneId, scene.SceneName })
                .Select(group => new SceneSummary
                {
                    SceneId = group.Key.SceneId,
                    SceneName = string.IsNullOrWhiteSpace(group.Key.SceneName) ? $"Scene {group.Key.SceneId}" : group.Key.SceneName,
                    Count = group.Count(),
                    Seconds = group.Sum(scene => scene.DurationSeconds)
                })
                .OrderByDescending(scene => scene.Count)
                .ThenByDescending(scene => scene.Seconds)
                .FirstOrDefault() ?? SceneSummary.None;
        }

        private static bool HasReachedEnding(SqliteConnection conn, int? playerId, int maxSceneId)
        {
            if (maxSceneId >= EndingSceneStart)
                return true;

            return CountEndingsAchieved(conn, playerId) > 0;
        }

        private static int CountEndingsAchieved(SqliteConnection conn, int? playerId)
        {
            if (!TableExists(conn, "tblUnlockedEnding"))
                return 0;

            using var cmd = new SqliteCommand(@"
                SELECT COUNT(DISTINCT EndingID)
                FROM tblUnlockedEnding
                WHERE @playerId IS NULL OR PlayerID = @playerId;", conn);
            cmd.Parameters.AddWithValue("@playerId", playerId.HasValue ? playerId.Value : DBNull.Value);
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture);
        }

        private static int GetMaxSavedSceneId(SqliteConnection conn, int? playerId)
        {
            if (!TableExists(conn, "tblSaveData"))
                return 0;

            using var cmd = new SqliteCommand(@"
                SELECT COALESCE(MAX(SceneID), 0)
                FROM tblSaveData
                WHERE @playerId IS NULL OR PlayerID = @playerId;", conn);
            cmd.Parameters.AddWithValue("@playerId", playerId.HasValue ? playerId.Value : DBNull.Value);
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture);
        }

        private static int EstimateProgressPercent(int maxSceneId, bool reachedEnding, List<SceneMetric> scenes)
        {
            if (reachedEnding)
                return 100;

            if (maxSceneId <= 0 && scenes.Count == 0)
                return 0;

            double sceneProgress = Math.Min(99d, Math.Max(0d, maxSceneId / (double)EndingSceneStart * 100d));
            double completionBonus = scenes.Count == 0
                ? 0d
                : scenes.Count(scene => string.Equals(scene.Classification, "Read", StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(scene.Classification, "Explored", StringComparison.OrdinalIgnoreCase))
                    / (double)scenes.Count * 8d;

            return (int)Math.Clamp(Math.Round(Math.Min(99d, sceneProgress + completionBonus)), 0d, 99d);
        }

        private static string FormatPlayerSummary(PlayerAnalyticsSummary summary)
        {
            StringBuilder builder = new();
            builder.AppendLine($"PLAYER: {summary.PlayerName}");
            builder.AppendLine($"GAMEPLAY PROGRESS: {summary.ProgressPercent}%{(summary.ReachedEnding ? " (reached an ending)" : " estimated")}");
            builder.AppendLine($"ENDINGS ACHIEVED: {summary.EndingsAchieved}");
            builder.AppendLine($"OVERALL SCREENTIME: {FormatDuration(summary.TotalSeconds)}");
            builder.AppendLine($"ATTEMPTS: {summary.AttemptCount}");
            builder.AppendLine($"SCENE LOGS: {summary.SceneLogCount}");
            builder.AppendLine($"FEEDBACK: {summary.FeedbackCount} saved" + (summary.AverageRating.HasValue ? $" | avg rating {summary.AverageRating.Value:F1}/10" : string.Empty));
            builder.AppendLine();
            builder.AppendLine($"MOST SKIMMED: {summary.SkimmedScene}");
            builder.AppendLine($"MOST SKIPPED: {summary.SkippedScene}");
            builder.AppendLine($"MOST COMPLETED: {summary.CompletedScene}");

            return builder.ToString();
        }

        private static string FormatDuration(double totalSeconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(Math.Max(0d, totalSeconds));
            if (time.TotalHours >= 1d)
                return $"{(int)time.TotalHours}h {time.Minutes}m {time.Seconds}s";

            if (time.TotalMinutes >= 1d)
                return $"{time.Minutes}m {time.Seconds}s";

            return $"{time.Seconds}s";
        }

        private void RatingsPanel_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.FromArgb(12, 12, 12));

            Rectangle bounds = new(56, 26, Math.Max(10, ratingsPanel.Width - 92), Math.Max(10, ratingsPanel.Height - 92));
            using Pen axisPen = new(Color.FromArgb(70, 70, 70), 1.5f);
            using Pen gridPen = new(Color.FromArgb(32, 32, 32), 1f);
            using Font axisFont = new("Pixel Operator Mono", 20f, FontStyle.Regular);

            e.Graphics.DrawLine(axisPen, bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom);
            e.Graphics.DrawLine(axisPen, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom);

            int maxCount = Math.Max(20, ratingBuckets.Count == 0 ? 1 : ratingBuckets.Max(bucket => bucket.Count));
            int tickCount = 4;
            for (int i = 0; i <= tickCount; i++)
            {
                float y = bounds.Bottom - (bounds.Height * (i / (float)tickCount));
                e.Graphics.DrawLine(gridPen, bounds.Left, y, bounds.Right, y);
                using Pen tickPen = new(Color.FromArgb(90, 90, 90), 1f);
                e.Graphics.DrawLine(tickPen, bounds.Left - 8, y, bounds.Left - 2, y);
            }

            float spacing = 8f;
            int bucketCount = ratingBuckets.Count;
            float barWidth = Math.Max(18f, (bounds.Width - (spacing * (bucketCount + 1))) / Math.Max(1f, bucketCount));
            float x = bounds.Left + spacing;

            for (int i = 0; i < bucketCount; i++)
            {
                RatingBucket bucket = ratingBuckets[i];
                float barHeight = bounds.Height * (bucket.Count / (float)maxCount);
                RectangleF barRect = new(x, bounds.Bottom - barHeight, barWidth, barHeight);

                using SolidBrush barBrush = new(Color.Chartreuse);
                e.Graphics.FillRectangle(barBrush, barRect);
                e.Graphics.DrawRectangle(Pens.Black, barRect.X, barRect.Y, barRect.Width, barRect.Height);

                if (bucket.Count > 0)
                {
                    using Font smallValueFont = new("Pixel Operator Mono", 16f, FontStyle.Bold);
                    TextRenderer.DrawText(e.Graphics, bucket.Count.ToString(CultureInfo.InvariantCulture), smallValueFont,
                        new Point((int)(barRect.X + (barRect.Width / 2f) - 8), (int)Math.Max(bounds.Top, barRect.Y - 26)),
                        Color.WhiteSmoke);
                }

                int tickHeight = 10;
                int tickX = (int)(barRect.X + (barRect.Width / 2f));
                using Pen tickPen = new(Color.FromArgb(200, 200, 200), 2f);
                e.Graphics.DrawLine(tickPen, tickX, bounds.Bottom + 6, tickX, bounds.Bottom + 6 + tickHeight);

                TextRenderer.DrawText(e.Graphics, bucket.Label, axisFont,
                    new Rectangle((int)barRect.X, bounds.Bottom + 18, (int)barWidth + 6, 24),
                    Color.Gainsboro,
                    TextFormatFlags.HorizontalCenter);

                x += barWidth + spacing;
            }

            using Font titleFont = new("Pixel Operator SC", 22f, FontStyle.Bold);
            Rectangle titleRect = new(bounds.Left, bounds.Bottom + 48, bounds.Width, 36);
            TextRenderer.DrawText(e.Graphics, "RATES", titleFont, titleRect, Color.Chartreuse, TextFormatFlags.HorizontalCenter);
        }

        private static SqliteConnection OpenConnection()
        {
            SqliteConnection conn = new($"Data Source={GameDatabasePath}");
            conn.Open();
            return conn;
        }

        private static void EnsureAdminTableExists(SqliteConnection conn)
        {
            using var cmd = new SqliteCommand(@"
                CREATE TABLE IF NOT EXISTS tblAdmin (
                    AdminID INTEGER PRIMARY KEY AUTOINCREMENT,
                    PlayerID INTEGER,
                    PlayerName TEXT,
                    AttemptNumber INTEGER,
                    Rating TEXT,
                    Suggestion TEXT,
                    SavedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                );", conn);
            cmd.ExecuteNonQuery();

            EnsureColumnExists(conn, "tblAdmin", "PlayerID", "ALTER TABLE tblAdmin ADD COLUMN PlayerID INTEGER;");
            EnsureColumnExists(conn, "tblAdmin", "PlayerName", "ALTER TABLE tblAdmin ADD COLUMN PlayerName TEXT;");
            EnsureColumnExists(conn, "tblAdmin", "AttemptNumber", "ALTER TABLE tblAdmin ADD COLUMN AttemptNumber INTEGER;");
            EnsureColumnExists(conn, "tblAdmin", "Rating", "ALTER TABLE tblAdmin ADD COLUMN Rating TEXT;");
            EnsureColumnExists(conn, "tblAdmin", "Suggestion", "ALTER TABLE tblAdmin ADD COLUMN Suggestion TEXT;");
            EnsureColumnExists(conn, "tblAdmin", "SavedAt", "ALTER TABLE tblAdmin ADD COLUMN SavedAt TEXT;");
        }

        private static void EnsureColumnExists(SqliteConnection conn, string tableName, string columnName, string alterSql)
        {
            if (TableHasColumn(conn, tableName, columnName))
                return;

            using var alter = new SqliteCommand(alterSql, conn);
            alter.ExecuteNonQuery();
        }

        private static bool TableExists(SqliteConnection conn, string tableName)
        {
            using var cmd = new SqliteCommand(@"
                SELECT COUNT(1)
                FROM sqlite_master
                WHERE type = 'table' AND name = @tableName;", conn);
            cmd.Parameters.AddWithValue("@tableName", tableName);
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0, CultureInfo.InvariantCulture) > 0;
        }

        private static bool TableHasColumn(SqliteConnection conn, string tableName, string columnName)
        {
            using var cmd = new SqliteCommand($"PRAGMA table_info('{tableName.Replace("'", "''")}');", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(1) && string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private sealed class PlayerOption
        {
            public static readonly PlayerOption AllPlayers = new(null, "ALL PLAYERS");

            public PlayerOption(int? playerId, string name)
            {
                PlayerId = playerId;
                Name = name;
            }

            public int? PlayerId { get; }
            public string Name { get; }

            public override string ToString()
            {
                return PlayerId.HasValue ? $"{Name} (ID {PlayerId.Value})" : Name;
            }
        }

        private sealed class RatingBucket
        {
            public string Label { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        private sealed class SceneMetric
        {
            public int PlayerId { get; set; }
            public string PlayerName { get; set; } = string.Empty;
            public int AttemptNumber { get; set; }
            public int SceneId { get; set; }
            public string SceneName { get; set; } = string.Empty;
            public double DurationSeconds { get; set; }
            public string Classification { get; set; } = string.Empty;
        }

        private sealed class SceneSummary
        {
            public static readonly SceneSummary None = new() { SceneName = "No data", Count = 0, Seconds = 0d };

            public int SceneId { get; set; }
            public string SceneName { get; set; } = string.Empty;
            public int Count { get; set; }
            public double Seconds { get; set; }

            public override string ToString()
            {
                if (Count <= 0)
                    return SceneName;

                return $"{SceneName} ({Count}x, {FormatDuration(Seconds)})";
            }
        }

        private sealed class PlayerAnalyticsSummary
        {
            public int? PlayerId { get; set; }
            public string PlayerName { get; set; } = string.Empty;
            public int ProgressPercent { get; set; }
            public bool ReachedEnding { get; set; }
            public int EndingsAchieved { get; set; }
            public double TotalSeconds { get; set; }
            public int AttemptCount { get; set; }
            public int SceneLogCount { get; set; }
            public int FeedbackCount { get; set; }
            public double? AverageRating { get; set; }
            public SceneSummary SkimmedScene { get; set; } = SceneSummary.None;
            public SceneSummary SkippedScene { get; set; } = SceneSummary.None;
            public SceneSummary CompletedScene { get; set; } = SceneSummary.None;
        }

        private static string GetPlayerName(SqliteConnection conn, int playerId)
        {
            if (TableExists(conn, "tblPlayer"))
            {
                using var playerCmd = new SqliteCommand("SELECT Name FROM tblPlayer WHERE PlayerID = @playerId LIMIT 1;", conn);
                playerCmd.Parameters.AddWithValue("@playerId", playerId);
                object? result = playerCmd.ExecuteScalar();
                if (result != null && result != DBNull.Value && !string.IsNullOrWhiteSpace(result.ToString()))
                    return result.ToString()!;
            }

            if (TableExists(conn, "tblSceneAnalytics"))
            {
                using var analyticsCmd = new SqliteCommand(@"
                    SELECT PlayerName
                    FROM tblSceneAnalytics
                    WHERE PlayerID = @playerId AND PlayerName IS NOT NULL AND TRIM(PlayerName) <> ''
                    ORDER BY Id DESC
                    LIMIT 1;", conn);
                analyticsCmd.Parameters.AddWithValue("@playerId", playerId);
                object? result = analyticsCmd.ExecuteScalar();
                if (result != null && result != DBNull.Value && !string.IsNullOrWhiteSpace(result.ToString()))
                    return result.ToString()!;
            }

            return $"Player {playerId}";
        }
    }
}

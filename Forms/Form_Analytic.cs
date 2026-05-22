using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using GameForms;

namespace GameForms.Forms
{
    public partial class Form_Analytic : Form
    {
        private const string GameDatabasePath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameDialogue.db";

        private readonly ComboBox attemptComboBox = new();
        private readonly Label summaryLabel = new();
        private readonly Panel chartPanel = new();
        private readonly TreeView actionTreeView = new();
        private readonly Button refreshButton = new();
        private readonly Button closeButton = new();

        private List<SceneAnalyticsRow> currentSceneRows = new();
        private List<ActionAnalyticsRow> currentActionRows = new();

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        public Form_Analytic()
        {
            InitializeComponent();
            FormLayoutHelper.Configure(this);
            FormEscapeCloseBehavior.Attach(this);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterParent;

            BuildAnalyticsUi();
            Load += Form_Analytic_Load;
            Shown += Form_Analytic_Shown;
        }

        private void Form_Analytic_Load(object sender, EventArgs e)
        {
            LoadAttempts();
        }

        private void Form_Analytic_Shown(object? sender, EventArgs e)
        {
            RefreshAnalyticsData();
        }

        private void BuildAnalyticsUi()
        {
            BackColor = Color.Black;

            Panel topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 92,
                BackColor = Color.Black
            };

            Label attemptLabel = new Label
            {
                Text = "Attempt",
                AutoSize = true,
                ForeColor = Color.Gainsboro,
                Font = new Font("Pixel Operator SC", 22f, FontStyle.Bold),
                Location = new Point(38, 28)
            };

            attemptComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            attemptComboBox.Font = new Font("Pixel Operator Mono", 16f, FontStyle.Regular);
            attemptComboBox.Location = new Point(170, 27);
            attemptComboBox.Size = new Size(170, 34);
            attemptComboBox.SelectedIndexChanged += AttemptComboBox_SelectedIndexChanged;

            refreshButton.Text = "Refresh";
            refreshButton.Font = new Font("Pixel Operator SC", 18f, FontStyle.Bold);
            refreshButton.BackColor = Color.White;
            refreshButton.ForeColor = Color.Black;
            refreshButton.FlatStyle = FlatStyle.Flat;
            refreshButton.Location = new Point(368, 24);
            refreshButton.Size = new Size(118, 40);
            refreshButton.Click += (_, _) => RefreshAnalyticsData();

            closeButton.Text = "Close";
            closeButton.Font = new Font("Pixel Operator SC", 18f, FontStyle.Bold);
            closeButton.BackColor = Color.Black;
            closeButton.ForeColor = Color.Chartreuse;
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.Location = new Point(1128, 24);
            closeButton.Size = new Size(104, 40);
            closeButton.Click += (_, _) => Close();

            summaryLabel.AutoSize = false;
            summaryLabel.ForeColor = Color.Silver;
            summaryLabel.Font = new Font("Pixel Operator Mono", 14f, FontStyle.Regular);
            summaryLabel.Location = new Point(520, 26);
            summaryLabel.Size = new Size(585, 42);

            topBar.Controls.Add(attemptLabel);
            topBar.Controls.Add(attemptComboBox);
            topBar.Controls.Add(refreshButton);
            topBar.Controls.Add(closeButton);
            topBar.Controls.Add(summaryLabel);

            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(32, 12, 32, 32),
                BackColor = Color.Black
            };

            Panel chartContainer = new Panel
            {
                Dock = DockStyle.Left,
                Width = 700,
                Padding = new Padding(0, 8, 18, 0),
                BackColor = Color.Black
            };

            Label chartTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 36,
                Text = "Scene Time Per Attempt",
                ForeColor = Color.Chartreuse,
                Font = new Font("Pixel Operator SC", 22f, FontStyle.Bold)
            };

            chartPanel.Dock = DockStyle.Fill;
            chartPanel.BackColor = Color.FromArgb(12, 12, 12);
            chartPanel.Paint += ChartPanel_Paint;

            chartContainer.Controls.Add(chartPanel);
            chartContainer.Controls.Add(chartTitle);

            Panel treeContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18, 8, 0, 0),
                BackColor = Color.Black
            };

            Label treeTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 36,
                Text = "Action Tree",
                ForeColor = Color.Chartreuse,
                Font = new Font("Pixel Operator SC", 22f, FontStyle.Bold)
            };

            actionTreeView.Dock = DockStyle.Fill;
            actionTreeView.BackColor = Color.FromArgb(12, 12, 12);
            actionTreeView.ForeColor = Color.WhiteSmoke;
            actionTreeView.BorderStyle = BorderStyle.None;
            actionTreeView.Font = new Font("Consolas", 10f, FontStyle.Regular);

            treeContainer.Controls.Add(actionTreeView);
            treeContainer.Controls.Add(treeTitle);

            contentPanel.Controls.Add(treeContainer);
            contentPanel.Controls.Add(chartContainer);

            Controls.Add(contentPanel);
            Controls.Add(topBar);
            label1.BringToFront();
        }

        private void LoadAttempts()
        {
            attemptComboBox.BeginUpdate();
            attemptComboBox.Items.Clear();

            foreach (int attemptNumber in QueryAttemptNumbers())
                attemptComboBox.Items.Add(attemptNumber);

            attemptComboBox.EndUpdate();

            if (attemptComboBox.Items.Count > 0 && attemptComboBox.SelectedIndex < 0)
                attemptComboBox.SelectedIndex = 0;
        }

        private void AttemptComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            RefreshAnalyticsData();
        }

        private void RefreshAnalyticsData()
        {
            if (!PlayerSession.HasActivePlayer)
            {
                currentSceneRows = new List<SceneAnalyticsRow>();
                currentActionRows = new List<ActionAnalyticsRow>();
                summaryLabel.Text = "No player selected yet.";
                actionTreeView.Nodes.Clear();
                chartPanel.Invalidate();
                return;
            }

            if (attemptComboBox.SelectedItem is not int attemptNumber)
            {
                currentSceneRows = new List<SceneAnalyticsRow>();
                currentActionRows = new List<ActionAnalyticsRow>();
                summaryLabel.Text = "No attempt data yet.";
                actionTreeView.Nodes.Clear();
                chartPanel.Invalidate();
                return;
            }

            currentSceneRows = LoadSceneAnalytics(attemptNumber);
            currentActionRows = LoadActionAnalytics(attemptNumber);

            double totalSeconds = currentSceneRows.Sum(row => row.DurationSeconds);
            int readCount = currentSceneRows.Count(row => string.Equals(row.Classification, "Read", StringComparison.OrdinalIgnoreCase));
            int skimCount = currentSceneRows.Count(row => string.Equals(row.Classification, "Skimmed", StringComparison.OrdinalIgnoreCase));
            int ignoredCount = currentSceneRows.Count(row => string.Equals(row.Classification, "Ignored", StringComparison.OrdinalIgnoreCase));

            summaryLabel.Text = $"Attempt {attemptNumber}: {currentSceneRows.Count} scene logs, {currentActionRows.Count} actions, {totalSeconds:F0}s total, Read {readCount}, Skimmed {skimCount}, Ignored {ignoredCount}";
            BuildActionTree(attemptNumber);
            chartPanel.Invalidate();
        }

        private List<int> QueryAttemptNumbers()
        {
            List<int> attempts = new();
            if (!PlayerSession.HasActivePlayer)
                return attempts;

            using var conn = OpenConnection();

            try
            {
                using var cmd = new SqliteCommand(@"
                    SELECT AttemptNumber
                    FROM tblAttempts
                    WHERE PlayerID = @playerId
                    ORDER BY AttemptNumber DESC;", conn);
                cmd.Parameters.AddWithValue("@playerId", PlayerSession.PlayerId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    attempts.Add(reader.GetInt32(0));
            }
            catch (SqliteException)
            {
                using var fallbackCmd = new SqliteCommand(@"
                    SELECT DISTINCT AttemptNumber
                    FROM tblSceneAnalytics
                    WHERE PlayerID = @playerId
                    ORDER BY AttemptNumber DESC;", conn);
                fallbackCmd.Parameters.AddWithValue("@playerId", PlayerSession.PlayerId);
                using var fallbackReader = fallbackCmd.ExecuteReader();
                while (fallbackReader.Read())
                    attempts.Add(fallbackReader.GetInt32(0));
            }

            return attempts;
        }

        private List<SceneAnalyticsRow> LoadSceneAnalytics(int attemptNumber)
        {
            List<SceneAnalyticsRow> rows = new();
            if (!PlayerSession.HasActivePlayer)
                return rows;

            using var conn = OpenConnection();
            using var cmd = new SqliteCommand(@"
                SELECT SceneID, SceneName, DurationSeconds, DialogueLineCount, DialogueAdvanceCount, InteractCount, ActionCount, Classification
                FROM tblSceneAnalytics
                WHERE AttemptNumber = @attemptNumber AND PlayerID = @playerId
                ORDER BY Id;", conn);
            cmd.Parameters.AddWithValue("@attemptNumber", attemptNumber);
            cmd.Parameters.AddWithValue("@playerId", PlayerSession.PlayerId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new SceneAnalyticsRow
                {
                    SceneID = reader.GetInt32(0),
                    SceneName = reader.GetString(1),
                    DurationSeconds = reader.GetDouble(2),
                    DialogueLineCount = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    DialogueAdvanceCount = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                    InteractCount = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    ActionCount = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                    Classification = reader.IsDBNull(7) ? "Unknown" : reader.GetString(7)
                });
            }
            return rows;
        }

        private List<ActionAnalyticsRow> LoadActionAnalytics(int attemptNumber)
        {
            List<ActionAnalyticsRow> rows = new();
            if (!PlayerSession.HasActivePlayer)
                return rows;

            using var conn = OpenConnection();
            using var cmd = new SqliteCommand(@"
                SELECT SceneID, SceneName, ActionType, ActionValue, Outcome, LoggedAt
                FROM tblActionAnalytics
                WHERE AttemptNumber = @attemptNumber AND PlayerID = @playerId
                ORDER BY Id;", conn);
            cmd.Parameters.AddWithValue("@attemptNumber", attemptNumber);
            cmd.Parameters.AddWithValue("@playerId", PlayerSession.PlayerId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(new ActionAnalyticsRow
                {
                    SceneID = reader.GetInt32(0),
                    SceneName = reader.GetString(1),
                    ActionType = reader.GetString(2),
                    ActionValue = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Outcome = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    LoggedAt = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                });
            }
            return rows;
        }

        private void BuildActionTree(int attemptNumber)
        {
            actionTreeView.BeginUpdate();
            actionTreeView.Nodes.Clear();

            TreeNode root = new($"Attempt {attemptNumber}");
            foreach (var sceneGroup in currentActionRows.GroupBy(row => new { row.SceneID, row.SceneName }))
            {
                TreeNode sceneNode = new($"{sceneGroup.Key.SceneName}");
                foreach (ActionAnalyticsRow action in sceneGroup)
                {
                    string actionText = string.IsNullOrWhiteSpace(action.ActionValue)
                        ? action.ActionType
                        : $"{action.ActionType}: {action.ActionValue}";

                    if (!string.IsNullOrWhiteSpace(action.Outcome))
                        actionText += $" -> {action.Outcome}";

                    sceneNode.Nodes.Add(new TreeNode(actionText));
                }

                root.Nodes.Add(sceneNode);
            }

            root.Expand();
            actionTreeView.Nodes.Add(root);
            actionTreeView.EndUpdate();
        }

        private void ChartPanel_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(Color.FromArgb(12, 12, 12));

            Rectangle chartBounds = new Rectangle(56, 20, Math.Max(10, chartPanel.Width - 92), Math.Max(10, chartPanel.Height - 68));
            using Pen axisPen = new(Color.FromArgb(70, 70, 70), 1.4f);
            using Pen gridPen = new(Color.FromArgb(32, 32, 32), 1f);
            using SolidBrush labelBrush = new(Color.Gainsboro);
            using Font axisFont = new("Pixel Operator Mono", 12f, FontStyle.Regular);

            e.Graphics.DrawLine(axisPen, chartBounds.Left, chartBounds.Bottom, chartBounds.Right, chartBounds.Bottom);
            e.Graphics.DrawLine(axisPen, chartBounds.Left, chartBounds.Top, chartBounds.Left, chartBounds.Bottom);

            if (currentSceneRows.Count == 0)
            {
                using Font emptyFont = new("Pixel Operator SC", 22f, FontStyle.Bold);
                TextRenderer.DrawText(e.Graphics, "No analytics recorded yet.", emptyFont, chartBounds, Color.DimGray,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            double maxSeconds = Math.Max(10d, currentSceneRows.Max(row => row.DurationSeconds));
            for (int i = 0; i <= 4; i++)
            {
                float y = chartBounds.Bottom - (chartBounds.Height * (i / 4f));
                e.Graphics.DrawLine(gridPen, chartBounds.Left, y, chartBounds.Right, y);
                string tickText = $"{(maxSeconds * i / 4d):F0}s";
                Size tickSize = TextRenderer.MeasureText(tickText, axisFont);
                TextRenderer.DrawText(e.Graphics, tickText, axisFont, new Point(chartBounds.Left - tickSize.Width - 8, (int)(y - (tickSize.Height / 2f))), Color.Gray);
            }

            int barCount = currentSceneRows.Count;
            float barSpacing = 18f;
            float barWidth = Math.Max(24f, (chartBounds.Width - (barSpacing * (barCount + 1))) / barCount);
            float x = chartBounds.Left + barSpacing;

            using Font sceneFont = new("Pixel Operator Mono", 11f, FontStyle.Regular);
            foreach (SceneAnalyticsRow row in currentSceneRows)
            {
                float barHeight = (float)(chartBounds.Height * (row.DurationSeconds / maxSeconds));
                RectangleF barRect = new(x, chartBounds.Bottom - barHeight, barWidth, barHeight);

                using SolidBrush barBrush = new(GetClassificationColor(row.Classification));
                e.Graphics.FillRectangle(barBrush, barRect);
                e.Graphics.DrawRectangle(Pens.Black, barRect.X, barRect.Y, barRect.Width, barRect.Height);

                string durationLabel = $"{row.DurationSeconds:F0}s";
                Size durationSize = TextRenderer.MeasureText(durationLabel, axisFont);
                TextRenderer.DrawText(e.Graphics, durationLabel, axisFont,
                    new Point((int)(barRect.X + ((barRect.Width - durationSize.Width) / 2f)), (int)Math.Max(chartBounds.Top, barRect.Y - durationSize.Height - 2)),
                    Color.WhiteSmoke);

                string sceneLabel = row.SceneName.Replace("Scene ", "S");
                Rectangle sceneRect = new((int)x - 8, chartBounds.Bottom + 6, (int)barWidth + 16, 36);
                TextRenderer.DrawText(e.Graphics, sceneLabel, sceneFont, sceneRect, Color.Gainsboro,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);

                x += barWidth + barSpacing;
            }
        }

        private static Color GetClassificationColor(string classification)
        {
            return classification switch
            {
                "Read" => Color.FromArgb(74, 222, 128),
                "Skimmed" => Color.FromArgb(250, 204, 21),
                "Ignored" => Color.FromArgb(248, 113, 113),
                "Explored" => Color.FromArgb(96, 165, 250),
                _ => Color.FromArgb(156, 163, 175)
            };
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
            using var pragma = new SqliteCommand($"PRAGMA table_info('{tableName.Replace("'", "''")}');", conn);
            using var reader = pragma.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(1) && string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            reader.Close();
            using var alter = new SqliteCommand(alterSql, conn);
            alter.ExecuteNonQuery();
        }

        private sealed class SceneAnalyticsRow
        {
            public int SceneID { get; set; }
            public string SceneName { get; set; } = string.Empty;
            public double DurationSeconds { get; set; }
            public int DialogueLineCount { get; set; }
            public int DialogueAdvanceCount { get; set; }
            public int InteractCount { get; set; }
            public int ActionCount { get; set; }
            public string Classification { get; set; } = string.Empty;
        }

        private sealed class ActionAnalyticsRow
        {
            public int SceneID { get; set; }
            public string SceneName { get; set; } = string.Empty;
            public string ActionType { get; set; } = string.Empty;
            public string ActionValue { get; set; } = string.Empty;
            public string Outcome { get; set; } = string.Empty;
            public string LoggedAt { get; set; } = string.Empty;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // rating chosen in combo (use SelectedItem if available, otherwise Text)
            ComboBox ratingCombo = null;
            try { ratingCombo = (ComboBox)typeof(Form_Analytic).GetField("combo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)?.GetValue(this); } catch { }
            ratingCombo ??= attemptComboBox;
            string rating = ratingCombo.SelectedItem?.ToString() ?? ratingCombo.Text ?? string.Empty;
            string suggestionText = suggestion.Text.Trim();

            if (string.IsNullOrWhiteSpace(rating) && string.IsNullOrWhiteSpace(suggestionText))
            {
                MessageBox.Show("Nothing to save yet.", "Analytics", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using var conn = OpenConnection();
                EnsureAdminTableExists(conn);

                using var cmd = new SqliteCommand(@"
                    INSERT INTO tblAdmin (PlayerID, PlayerName, AttemptNumber, Rating, Suggestion)
                    VALUES (@playerId, @playerName, @attemptNumber, @rating, @suggestion);", conn);

                if (PlayerSession.HasActivePlayer)
                {
                    cmd.Parameters.AddWithValue("@playerId", PlayerSession.PlayerId);
                    cmd.Parameters.AddWithValue("@playerName", PlayerSession.PlayerName);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@playerId", DBNull.Value);
                    cmd.Parameters.AddWithValue("@playerName", "Admin");
                }

                object? selectedAttempt = attemptComboBox.SelectedItem;
                if (selectedAttempt is int attemptNumber)
                    cmd.Parameters.AddWithValue("@attemptNumber", attemptNumber);
                else
                    cmd.Parameters.AddWithValue("@attemptNumber", DBNull.Value);

                cmd.Parameters.AddWithValue("@rating", string.IsNullOrWhiteSpace(rating) ? (object)DBNull.Value : rating);
                cmd.Parameters.AddWithValue("@suggestion", string.IsNullOrWhiteSpace(suggestionText) ? (object)DBNull.Value : suggestionText);
                cmd.ExecuteNonQuery();

                combo.SelectedIndex = -1;
                suggestion.Clear();
                if (PlayerSession.HasActivePlayer)
                    HistoryLogService.QuickLog("PLAYER", "Submitted analytics feedback.");

                MessageBox.Show("Feedback saved.", "Analytics", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to save feedback right now.\n\n{ex.Message}",
                    "Analytics Save Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

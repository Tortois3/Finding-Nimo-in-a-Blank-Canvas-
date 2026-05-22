using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;
using GameForms;

namespace GameForms.Forms
{
    public partial class Form_Achievement : Form
    {
        private const string DefaultDatabasePath = @"C:\Users\Tiffany Mae\Documents\PROJECT PROPOSAL\GameDialogue.db";
        private readonly Dictionary<int, PictureBox> _endingBaseSlots = new();
        private readonly Dictionary<int, Label> _endingLabels = new();
        private readonly System.Windows.Forms.Timer _refreshTimer = new() { Interval = 1200 };
        private string _lastAchievementSnapshot = string.Empty;
        private Dictionary<int, AchievementStatus> _currentAchievementStatuses = new();
        private static readonly Dictionary<int, string> DefaultEndingNames = new()
        {
            [7] = "Where to go...",
            [8] = "One is not dead when they're remembered",
            [9] = "Are you regretting this?",
            [10] = "Familiariarity X",
            [11] = "E N D",
            [12] = "Out of Comfort Zone"
        };

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        public Form_Achievement()
        {
            InitializeComponent();
            FormLayoutHelper.Configure(this);
            FormEscapeCloseBehavior.Attach(this);
            this.FormBorderStyle = FormBorderStyle.None;

            this.Shown += Form_Achievement_Shown;

            this.KeyPreview = true;
            this.KeyDown += Form2_KeyDown;
            this.Activated += (_, _) => RefreshAchievementBoard();

            BuildPuzzleMappings();
            _refreshTimer.Tick += (_, _) => RefreshAchievementBoard();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form_Achievement_Load(object sender, EventArgs e)
        {
            RefreshAchievementBoard();
            _refreshTimer.Start();
        }

        private void Form_Achievement_Shown(object? sender, EventArgs e)
        {
            PositionAtRightSide();
        }

        private void Form_Achievement_Load_1(object sender, EventArgs e)
        {
            Form_Achievement_Load(sender, e);
        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _refreshTimer.Stop();
            base.OnFormClosed(e);
        }

        private void BuildPuzzleMappings()
        {
            pictureBox1.BackColor = Color.WhiteSmoke;
            _endingBaseSlots[1] = p0;
            _endingBaseSlots[2] = p1;
            _endingBaseSlots[3] = p2;
            _endingBaseSlots[4] = p3;
            _endingBaseSlots[5] = p4;
            _endingBaseSlots[6] = p5;
            _endingBaseSlots[7] = p6;
            _endingBaseSlots[8] = p7;
            _endingBaseSlots[9] = p8;
            _endingBaseSlots[10] = p9;
            _endingBaseSlots[11] = p10;
            _endingBaseSlots[12] = p11;

            _endingLabels[1] = End1;
            _endingLabels[2] = End2;
            _endingLabels[3] = End3;
            _endingLabels[4] = End4;
            _endingLabels[5] = End5;
            _endingLabels[6] = End6;
            _endingLabels[7] = End7;
            _endingLabels[8] = End8;
            _endingLabels[9] = End9;
            _endingLabels[10] = End10;
            _endingLabels[11] = End11;
            _endingLabels[12] = End12;

            foreach (PictureBox slot in _endingBaseSlots.Values)
            {
                NormalizePuzzleArt(slot);
                slot.Cursor = Cursors.Default;
            }

            HideRevealSlots();

            foreach (KeyValuePair<int, Label> entry in _endingLabels)
            {
                Label caption = entry.Value;
                caption.BackColor = Color.Transparent;
                caption.ForeColor = Color.Chartreuse;
                caption.AutoSize = false;
                caption.TextAlign = ContentAlignment.MiddleCenter;
                caption.Cursor = Cursors.Default;
                caption.Tag = entry.Key;
                caption.Click -= AchievementSlot_Click;
                caption.Click += AchievementSlot_Click;
            }

            foreach (KeyValuePair<int, PictureBox> entry in _endingBaseSlots)
            {
                PictureBox slot = entry.Value;
                slot.Tag = entry.Key;
                slot.Click -= AchievementSlot_Click;
                slot.Click += AchievementSlot_Click;
            }

            WireRevealSlotClicks();
        }

        private void RefreshAchievementBoard()
        {
            Dictionary<int, AchievementStatus> achievementStatuses = LoadAchievementStatuses();
            string snapshot = BuildAchievementSnapshot(achievementStatuses);

            if (string.Equals(_lastAchievementSnapshot, snapshot, StringComparison.Ordinal))
                return;

            _lastAchievementSnapshot = snapshot;
            _currentAchievementStatuses = achievementStatuses;

            for (int endingId = 1; endingId <= 12; endingId++)
            {
                bool isUnlocked = achievementStatuses.TryGetValue(endingId, out AchievementStatus? status) && status.IsUnlocked;
                if (_endingBaseSlots.TryGetValue(endingId, out PictureBox? baseSlot))
                {
                    baseSlot.Cursor = isUnlocked ? Cursors.Hand : Cursors.Default;

                    if (_endingLabels.TryGetValue(endingId, out Label? caption))
                    {
                        caption.Text = isUnlocked && status != null
                            ? ResolveDisplayAchievementName(endingId, status.AchievementName)
                            : "???";
                        caption.MaximumSize = new Size(Math.Max(baseSlot.Width + 56, 120), 0);
                        caption.Width = Math.Max(baseSlot.Width + 56, 120);
                        caption.Height = Math.Max(caption.PreferredHeight + 6, 32);
                        caption.Location = new Point(
                            baseSlot.Left + ((baseSlot.Width - caption.Width) / 2),
                            baseSlot.Bottom + 6);
                        caption.Cursor = isUnlocked ? Cursors.Hand : Cursors.Default;
                    }
                }
            }
        }

        private static Dictionary<int, AchievementStatus> LoadAchievementStatuses()
        {
            Dictionary<int, AchievementStatus> achievementStatuses = new();

            if (!PlayerSession.HasActivePlayer || !File.Exists(DefaultDatabasePath))
                return achievementStatuses;

            try
            {
                using var conn = new SqliteConnection($"Data Source={DefaultDatabasePath}");
                conn.Open();

                string query = HasEndingIdColumn(conn)
                    ? @"
                        SELECT COALESCE(EndingID, AchievementID) AS EndingSlot,
                               AchievementName,
                               COALESCE(TimesUnlocked, 0) AS TimesUnlocked
                        FROM tblAchievements
                        WHERE PlayerID = @playerId
                        ORDER BY COALESCE(EndingID, AchievementID);"
                    : @"
                        SELECT AchievementID AS EndingSlot,
                               AchievementName,
                               COALESCE(TimesUnlocked, 0) AS TimesUnlocked
                        FROM tblAchievements
                        WHERE PlayerID = @playerId
                        ORDER BY AchievementID;";

                using var cmd = new SqliteCommand(query, conn);
                cmd.Parameters.AddWithValue("@playerId", PlayerSession.PlayerId);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int endingSlot = reader.GetInt32(0);
                    string achievementName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    int timesUnlocked = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                    achievementStatuses[endingSlot] = new AchievementStatus(achievementName, timesUnlocked > 0);
                }
            }
            catch
            {
                return new Dictionary<int, AchievementStatus>();
            }

            return achievementStatuses;
        }

        private static bool HasEndingIdColumn(SqliteConnection conn)
        {
            using var pragmaCmd = new SqliteCommand("PRAGMA table_info(tblAchievements);", conn);
            using var reader = pragmaCmd.ExecuteReader();

            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), "EndingID", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string BuildAchievementSnapshot(Dictionary<int, AchievementStatus> achievementStatuses)
        {
            StringBuilder snapshot = new();

            for (int endingId = 1; endingId <= 12; endingId++)
            {
                AchievementStatus? status = achievementStatuses.TryGetValue(endingId, out AchievementStatus? existingStatus)
                    ? existingStatus
                    : null;
                snapshot.Append(endingId)
                    .Append(':')
                    .Append(status?.IsUnlocked == true ? '1' : '0')
                    .Append(':')
                    .Append(ResolveDisplayAchievementName(endingId, status?.AchievementName ?? string.Empty))
                    .Append('|');
            }

            return snapshot.ToString();
        }

        private static void NormalizePuzzleArt(PictureBox slot)
        {
            // Intentionally do nothing — preserve the PictureBox properties (BackColor,
            // BackgroundImage, Image, SizeMode) exactly as defined in the designer so
            // visuals remain identical to the raw WinForms design.
            return;
        }

        private void HideRevealSlots()
        {
            // Intentionally left blank — do not alter visibility, z-order or images of
            // the reveal slots. Designer-specified visuals must remain unchanged.
        }

        private void WireRevealSlotClicks()
        {
            foreach (PictureBox slot in GetRevealSlots())
            {
                if (slot.Tag is not int endingId)
                    slot.Tag = GetRevealEndingId(slot);

                slot.Click -= AchievementSlot_Click;
                slot.Click += AchievementSlot_Click;
            }
        }

        private IEnumerable<PictureBox> GetRevealSlots()
        {
            yield return p0;
            yield return p1;
            yield return p2;
            yield return p3;
            yield return p4;
            yield return p5;
            yield return p6;
            yield return p7;
            yield return p8;
            yield return p9;
            yield return p10;
            yield return p11;
        }

        private static int GetRevealEndingId(PictureBox slot)
        {
            return slot.Name switch
            {
                "p0" => 1,
                "p1" => 2,
                "p2" => 3,
                "p3" => 4,
                "p4" => 5,
                "p5" => 6,
                "p6" => 7,
                "p7" => 8,
                "p8" => 9,
                "p9" => 10,
                "p10" => 11,
                "p11" => 12,
                _ => 0
            };
        }

        private void AchievementSlot_Click(object? sender, EventArgs e)
        {
            if (sender is not Control control || control.Tag is not int endingId)
                return;

            if (!_currentAchievementStatuses.TryGetValue(endingId, out AchievementStatus? status) || !status.IsUnlocked)
                return;

            if (!Form4.TryLaunchGame(out string? errorMessage, $"--ending={endingId}"))
            {
                MessageBox.Show(
                    errorMessage ?? "The ending could not be opened.",
                    "Ending Launch Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (Owner != null)
                Owner.Close();

            Close();
        }

        private void PositionAtRightSide()
        {
            Rectangle targetBounds = Owner?.Bounds ?? Screen.FromControl(this).WorkingArea;
            int x = targetBounds.Right - Width;
            int y = targetBounds.Top;

            if (x < 0)
                x = 0;
            if (y < 0)
                y = 0;

            Location = new Point(x, y);
        }

        private static string ResolveDisplayAchievementName(int endingId, string achievementName)
        {
            if (!string.IsNullOrWhiteSpace(achievementName))
                return achievementName;

            return DefaultEndingNames.TryGetValue(endingId, out string? fallbackName)
                ? fallbackName
                : "???";
        }

        private sealed class AchievementStatus
        {
            public AchievementStatus(string achievementName, bool isUnlocked)
            {
                AchievementName = achievementName;
                IsUnlocked = isUnlocked;
            }

            public string AchievementName { get; }
            public bool IsUnlocked { get; }
        }

        private void puzzle0_Click(object sender, EventArgs e)
        {

        }
    }
}

using System;
using System.IO;
using System.Windows.Forms;
using GameForms;

namespace GameForms.Forms
{
    public partial class History : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        public Form_History Form_History
        {
            get => default;
            set
            {
            }
        }

        public History()
        {
            InitializeComponent();
            FormLayoutHelper.Configure(this);
            FormEscapeCloseBehavior.Attach(this);
            this.FormBorderStyle = FormBorderStyle.None;
        }

        private void button1_Click(object sender, EventArgs e) //see log
        {
            if (PlayerSession.HasActivePlayer)
                HistoryLogService.QuickLog("SYSTEM", "Opened History log");

            using (Form_History form = new Form_History())
            {
                this.Close();
                form.ShowDialog();
             }
        }

        private void Ok_Click(object sender, EventArgs e) //delete log
        {
            if (!PlayerSession.HasActivePlayer)
            {
                MessageBox.Show("No player data to reset yet.");
                Close();
                return;
            }

            GameForms.DatabaseMaintenance.ResetProgressAndAnalytics(PlayerSession.PlayerId);
            HistoryLogService.Clear();

            MessageBox.Show("History, save progress, achievements, and analytics were reset successfully.");
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

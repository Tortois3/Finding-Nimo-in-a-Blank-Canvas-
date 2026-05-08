using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GameForms;

namespace GameForms.Forms
{
    public partial class Form_History : Form
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

        public Form_History()
        {
            InitializeComponent();
            FormEscapeCloseBehavior.Attach(this);
            this.FormBorderStyle = FormBorderStyle.None;

            this.Load += Form_History_Load;
        }

        public void SaveProgress(string checkpointName)
        {
            HistoryLogService.QuickLog("SYSTEM", $"Progress saved at checkpoint: {checkpointName}");
        }

        public string LoadProgress()
        {
            return string.Empty;
        }

        public void QuickLog(string user, string action)
        {
            HistoryLogService.QuickLog(user, action);
        }

        public void QuickLogSafe(string user, string action)
        {
            HistoryLogService.QuickLog(user, action);
        }

        public string[] ReadLogSafe()
        {
            string history = HistoryLogService.ReadHistory();
            return history.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }

        private void panel_Paint(object sender, PaintEventArgs e)
        {
            date.Text = DateTime.Now.ToString("MM/dd/yyyy");
            time.Text = DateTime.Now.ToString("HH:mm:ss");
            log.Text = HistoryLogService.ReadHistory();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form_History_Load(object sender, EventArgs e)
        { }

    }
}

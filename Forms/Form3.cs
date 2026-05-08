using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameForms
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.Bounds = Screen.PrimaryScreen.Bounds;

            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000; // half a second
            // Attach the exact handler name below. Make sure there's only one handler with this exact name.
            timer1.Tick -= timer1_Tick; // safe to remove any previous attachment
            timer1.Tick += timer1_Tick;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (progressBar1.Value < progressBar1.Maximum)
            {
                progressBar1.Increment(10);
            }

            if (progressBar1.Value >= progressBar1.Maximum)
            {
                timer1.Stop();
                Close();
            }
        }
    }
}

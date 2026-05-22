using MaterialSkin;
using MaterialSkin.Controls;
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
    public partial class Exit : Form
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


        public Exit()
        {
            InitializeComponent();
            FormLayoutHelper.Configure(this);
            FormEscapeCloseBehavior.Attach(this);
            this.FormBorderStyle = FormBorderStyle.None;
        }

        public bool OkCancel = false;

        public string lblMessage
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        private void Form_OkCancel_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)//cancel
        {
            OkCancel = false;
            this.Close();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            OkCancel = true;
            this.Close();
        }

        private void panel9_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}

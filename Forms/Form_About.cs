using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameForms.Forms
{
    public partial class Form_About : Form
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

        public Form_About()
        {
            InitializeComponent();
            FormEscapeCloseBehavior.Attach(this);
            this.FormBorderStyle = FormBorderStyle.None;
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

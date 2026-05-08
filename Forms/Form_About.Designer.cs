namespace GameForms.Forms
{
    partial class Form_About
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_About));
            panel4 = new Panel();
            panel1 = new Panel();
            panel2 = new Panel();
            panel7 = new Panel();
            panel3 = new Panel();
            panel5 = new Panel();
            panel10 = new Panel();
            Ok = new Button();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            pictureBox1 = new PictureBox();
            panel8 = new Panel();
            panel9 = new Panel();
            panel6 = new Panel();
            panel4.SuspendLayout();
            panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel8.SuspendLayout();
            panel9.SuspendLayout();
            SuspendLayout();
            // 
            // panel4
            // 
            panel4.BackColor = SystemColors.ButtonHighlight;
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(panel1);
            panel4.Cursor = Cursors.PanNW;
            panel4.Dock = DockStyle.Top;
            panel4.Location = new Point(0, 0);
            panel4.Name = "panel4";
            panel4.Size = new Size(776, 10);
            panel4.TabIndex = 3;
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.ActiveCaptionText;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Cursor = Cursors.PanNW;
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(774, 10);
            panel1.TabIndex = 3;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ButtonHighlight;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Cursor = Cursors.PanNW;
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 10);
            panel2.Name = "panel2";
            panel2.Size = new Size(776, 10);
            panel2.TabIndex = 4;
            // 
            // panel7
            // 
            panel7.BackColor = SystemColors.ActiveCaptionText;
            panel7.BackgroundImageLayout = ImageLayout.None;
            panel7.Dock = DockStyle.Bottom;
            panel7.Location = new Point(0, 790);
            panel7.Name = "panel7";
            panel7.Size = new Size(776, 10);
            panel7.TabIndex = 5;
            // 
            // panel3
            // 
            panel3.BackColor = SystemColors.ButtonHighlight;
            panel3.BackgroundImageLayout = ImageLayout.None;
            panel3.Dock = DockStyle.Bottom;
            panel3.Location = new Point(0, 780);
            panel3.Name = "panel3";
            panel3.Size = new Size(776, 10);
            panel3.TabIndex = 6;
            // 
            // panel5
            // 
            panel5.AutoScroll = true;
            panel5.BackColor = SystemColors.ActiveCaptionText;
            panel5.Controls.Add(panel10);
            panel5.Controls.Add(Ok);
            panel5.Controls.Add(label3);
            panel5.Controls.Add(label2);
            panel5.Controls.Add(label1);
            panel5.Controls.Add(pictureBox1);
            panel5.Controls.Add(panel8);
            panel5.Cursor = Cursors.PanNW;
            panel5.Dock = DockStyle.Fill;
            panel5.Location = new Point(0, 20);
            panel5.Name = "panel5";
            panel5.Size = new Size(776, 760);
            panel5.TabIndex = 7;
            // 
            // panel10
            // 
            panel10.BackColor = SystemColors.ButtonHighlight;
            panel10.BorderStyle = BorderStyle.FixedSingle;
            panel10.Dock = DockStyle.Left;
            panel10.Location = new Point(14, 0);
            panel10.Name = "panel10";
            panel10.Size = new Size(10, 1431);
            panel10.TabIndex = 12;
            // 
            // Ok
            // 
            Ok.BackColor = Color.AliceBlue;
            Ok.Cursor = Cursors.Hand;
            Ok.Font = new Font("Pixel Operator SC", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Ok.ForeColor = Color.DarkGreen;
            Ok.Location = new Point(576, 1353);
            Ok.Name = "Ok";
            Ok.Size = new Size(151, 47);
            Ok.TabIndex = 11;
            Ok.Text = "Back";
            Ok.UseVisualStyleBackColor = false;
            Ok.Click += Ok_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Pixel Operator SC", 16.1999989F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.ForeColor = Color.LawnGreen;
            label3.Location = new Point(22, 486);
            label3.Name = "label3";
            label3.Size = new Size(728, 945);
            label3.TabIndex = 10;
            label3.Text = resources.GetString("label3.Text");
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Cursor = Cursors.PanNW;
            label2.Font = new Font("Pixel Operator Mono", 28.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.Lime;
            label2.Location = new Point(224, 399);
            label2.Name = "label2";
            label2.Size = new Size(342, 47);
            label2.TabIndex = 9;
            label2.Text = "ABOUT THE GAME";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Cursor = Cursors.PanNW;
            label1.Font = new Font("Pixel Operator SC", 36F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Gold;
            label1.Location = new Point(246, 26);
            label1.Name = "label1";
            label1.Size = new Size(303, 60);
            label1.TabIndex = 0;
            label1.Text = "SNAKYWORKS";
            // 
            // pictureBox1
            // 
            pictureBox1.BackgroundImage = Properties.Resources.Screenshot_2026_03_28_205431;
            pictureBox1.Location = new Point(25, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(724, 410);
            pictureBox1.TabIndex = 8;
            pictureBox1.TabStop = false;
            // 
            // panel8
            // 
            panel8.BackColor = SystemColors.ActiveCaptionText;
            panel8.BorderStyle = BorderStyle.FixedSingle;
            panel8.Controls.Add(panel9);
            panel8.Dock = DockStyle.Left;
            panel8.Location = new Point(0, 0);
            panel8.Name = "panel8";
            panel8.Size = new Size(14, 1431);
            panel8.TabIndex = 4;
            // 
            // panel9
            // 
            panel9.BackColor = SystemColors.ButtonHighlight;
            panel9.BorderStyle = BorderStyle.FixedSingle;
            panel9.Controls.Add(panel6);
            panel9.Dock = DockStyle.Left;
            panel9.Location = new Point(0, 0);
            panel9.Name = "panel9";
            panel9.Size = new Size(10, 1429);
            panel9.TabIndex = 5;
            // 
            // panel6
            // 
            panel6.BackColor = SystemColors.ActiveCaptionText;
            panel6.BorderStyle = BorderStyle.FixedSingle;
            panel6.Dock = DockStyle.Left;
            panel6.Location = new Point(0, 0);
            panel6.Name = "panel6";
            panel6.Size = new Size(10, 1427);
            panel6.TabIndex = 6;
            // 
            // Form_About
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(776, 800);
            Controls.Add(panel5);
            Controls.Add(panel3);
            Controls.Add(panel7);
            Controls.Add(panel2);
            Controls.Add(panel4);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form_About";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form_About";
            panel4.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel8.ResumeLayout(false);
            panel9.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel4;
        private Panel panel1;
        private Panel panel2;
        private Panel panel7;
        private Panel panel3;
        private Panel panel5;
        private Panel panel8;
        private Panel panel9;
        private Label label1;
        private PictureBox pictureBox1;
        private Label label2;
        private Label label3;
        private Panel panel10;
        private Button Ok;
        private Panel panel6;
    }
}
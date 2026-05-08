namespace GameForms.Forms
{
    partial class Form_Welcome
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
            label1 = new Label();
            panel1 = new Panel();
            panel3 = new Panel();
            panel4 = new Panel();
            panel2 = new Panel();
            panel10 = new Panel();
            panel8 = new Panel();
            panel9 = new Panel();
            panel5 = new Panel();
            panel6 = new Panel();
            panel7 = new Panel();
            Ok = new Button();
            panel1.SuspendLayout();
            panel3.SuspendLayout();
            panel2.SuspendLayout();
            panel8.SuspendLayout();
            panel5.SuspendLayout();
            panel6.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Cursor = Cursors.PanNW;
            label1.Font = new Font("Pixel Operator SC", 36F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Chartreuse;
            label1.Location = new Point(68, 100);
            label1.Name = "label1";
            label1.Size = new Size(572, 60);
            label1.TabIndex = 0;
            label1.Text = "Welcome Back, Player!";
            label1.Click += label1_Click;
            // 
            // panel1
            // 
            panel1.BackColor = Color.SeaGreen;
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(panel3);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(682, 17);
            panel1.TabIndex = 2;
            // 
            // panel3
            // 
            panel3.BackColor = SystemColors.ActiveCaptionText;
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel3.Controls.Add(panel4);
            panel3.Dock = DockStyle.Top;
            panel3.Location = new Point(0, 0);
            panel3.Name = "panel3";
            panel3.Size = new Size(680, 17);
            panel3.TabIndex = 1;
            // 
            // panel4
            // 
            panel4.BackColor = Color.LawnGreen;
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Cursor = Cursors.PanNW;
            panel4.Dock = DockStyle.Top;
            panel4.Location = new Point(0, 0);
            panel4.Name = "panel4";
            panel4.Size = new Size(678, 10);
            panel4.TabIndex = 2;
            // 
            // panel2
            // 
            panel2.BackColor = SystemColors.ActiveCaptionText;
            panel2.Controls.Add(panel10);
            panel2.Controls.Add(panel8);
            panel2.Controls.Add(panel5);
            panel2.Controls.Add(Ok);
            panel2.Controls.Add(label1);
            panel2.Cursor = Cursors.PanNW;
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(682, 353);
            panel2.TabIndex = 3;
            // 
            // panel10
            // 
            panel10.BackColor = Color.Chartreuse;
            panel10.BorderStyle = BorderStyle.FixedSingle;
            panel10.Dock = DockStyle.Right;
            panel10.Location = new Point(672, 0);
            panel10.Name = "panel10";
            panel10.Size = new Size(10, 336);
            panel10.TabIndex = 5;
            // 
            // panel8
            // 
            panel8.BackColor = SystemColors.ActiveCaptionText;
            panel8.BorderStyle = BorderStyle.FixedSingle;
            panel8.Controls.Add(panel9);
            panel8.Dock = DockStyle.Left;
            panel8.Location = new Point(0, 0);
            panel8.Name = "panel8";
            panel8.Size = new Size(14, 336);
            panel8.TabIndex = 4;
            // 
            // panel9
            // 
            panel9.BackColor = Color.Chartreuse;
            panel9.BorderStyle = BorderStyle.FixedSingle;
            panel9.Dock = DockStyle.Left;
            panel9.Location = new Point(0, 0);
            panel9.Name = "panel9";
            panel9.Size = new Size(10, 334);
            panel9.TabIndex = 5;
            // 
            // panel5
            // 
            panel5.BackColor = SystemColors.ActiveCaptionText;
            panel5.BorderStyle = BorderStyle.FixedSingle;
            panel5.Controls.Add(panel6);
            panel5.Dock = DockStyle.Bottom;
            panel5.Location = new Point(0, 336);
            panel5.Name = "panel5";
            panel5.Size = new Size(682, 17);
            panel5.TabIndex = 3;
            // 
            // panel6
            // 
            panel6.BackColor = SystemColors.ActiveCaptionText;
            panel6.BorderStyle = BorderStyle.FixedSingle;
            panel6.Controls.Add(panel7);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(0, -2);
            panel6.Name = "panel6";
            panel6.Size = new Size(680, 17);
            panel6.TabIndex = 2;
            // 
            // panel7
            // 
            panel7.BackColor = Color.GreenYellow;
            panel7.BackgroundImageLayout = ImageLayout.None;
            panel7.Dock = DockStyle.Bottom;
            panel7.Location = new Point(0, 5);
            panel7.Name = "panel7";
            panel7.Size = new Size(678, 10);
            panel7.TabIndex = 3;
            // 
            // Ok
            // 
            Ok.BackColor = Color.AliceBlue;
            Ok.Cursor = Cursors.Hand;
            Ok.Font = new Font("Pixel Operator SC", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Ok.ForeColor = Color.DarkGreen;
            Ok.Location = new Point(264, 210);
            Ok.Name = "Ok";
            Ok.Size = new Size(151, 47);
            Ok.TabIndex = 2;
            Ok.Text = "I'm Back!";
            Ok.UseVisualStyleBackColor = false;
            Ok.Click += Ok_Click;
            // 
            // Form_Welcome
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(682, 353);
            Controls.Add(panel1);
            Controls.Add(panel2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form_Welcome";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form_Welcome";
            panel1.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel8.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel6.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private Panel panel1;
        private Panel panel3;
        private Panel panel4;
        private Panel panel2;
        private Panel panel10;
        private Panel panel8;
        private Panel panel9;
        private Panel panel5;
        private Panel panel6;
        private Panel panel7;
        private Button Ok;
    }
}
namespace GameForms.Forms
{
    partial class Form_LogIn
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_LogIn));
            panel12 = new Panel();
            panel11 = new Panel();
            panel6 = new Panel();
            panel8 = new Panel();
            panel5 = new Panel();
            label5 = new Label();
            panel = new Panel();
            clear = new Label();
            label2 = new Label();
            label1 = new Label();
            Save = new Button();
            Cancel = new Button();
            btnShow = new Button();
            btnHide = new Button();
            tBPin = new TextBox();
            panel9 = new Panel();
            panel2 = new Panel();
            panel10 = new Panel();
            panel1 = new Panel();
            panel7 = new Panel();
            panel3 = new Panel();
            panel4 = new Panel();
            panel6.SuspendLayout();
            panel.SuspendLayout();
            panel1.SuspendLayout();
            panel3.SuspendLayout();
            panel4.SuspendLayout();
            SuspendLayout();
            // 
            // panel12
            // 
            panel12.BackColor = Color.Gold;
            panel12.BackgroundImageLayout = ImageLayout.None;
            panel12.Dock = DockStyle.Top;
            panel12.Location = new Point(10, 0);
            panel12.Name = "panel12";
            panel12.Size = new Size(980, 10);
            panel12.TabIndex = 12;
            // 
            // panel11
            // 
            panel11.BackColor = Color.MistyRose;
            panel11.BorderStyle = BorderStyle.FixedSingle;
            panel11.Dock = DockStyle.Right;
            panel11.Location = new Point(990, 0);
            panel11.Name = "panel11";
            panel11.Size = new Size(10, 580);
            panel11.TabIndex = 11;
            // 
            // panel6
            // 
            panel6.BackColor = Color.Gold;
            panel6.BackgroundImageLayout = ImageLayout.None;
            panel6.Controls.Add(panel8);
            panel6.Dock = DockStyle.Bottom;
            panel6.Location = new Point(10, 580);
            panel6.Name = "panel6";
            panel6.Size = new Size(990, 10);
            panel6.TabIndex = 10;
            // 
            // panel8
            // 
            panel8.BackColor = SystemColors.Control;
            panel8.BorderStyle = BorderStyle.FixedSingle;
            panel8.Dock = DockStyle.Right;
            panel8.Location = new Point(980, 0);
            panel8.Name = "panel8";
            panel8.Size = new Size(10, 10);
            panel8.TabIndex = 6;
            // 
            // panel5
            // 
            panel5.BackColor = Color.MistyRose;
            panel5.BorderStyle = BorderStyle.FixedSingle;
            panel5.Dock = DockStyle.Left;
            panel5.Location = new Point(0, 0);
            panel5.Name = "panel5";
            panel5.Size = new Size(10, 590);
            panel5.TabIndex = 9;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Cursor = Cursors.Hand;
            label5.Font = new Font("Pixel Operator Mono", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label5.ForeColor = Color.Peru;
            label5.Location = new Point(180, 269);
            label5.Name = "label5";
            label5.Size = new Size(43, 23);
            label5.TabIndex = 17;
            label5.Text = "Pin";
            // 
            // panel
            // 
            panel.Controls.Add(label5);
            panel.Controls.Add(clear);
            panel.Controls.Add(label2);
            panel.Controls.Add(label1);
            panel.Controls.Add(Save);
            panel.Controls.Add(Cancel);
            panel.Controls.Add(btnShow);
            panel.Controls.Add(btnHide);
            panel.Controls.Add(tBPin);
            panel.Location = new Point(15, 61);
            panel.Name = "panel";
            panel.Size = new Size(953, 483);
            panel.TabIndex = 16;
            // 
            // clear
            // 
            clear.AutoSize = true;
            clear.Cursor = Cursors.Hand;
            clear.Font = new Font("Pixel Operator Mono", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            clear.ForeColor = Color.Peru;
            clear.Location = new Point(406, 363);
            clear.Name = "clear";
            clear.Size = new Size(142, 23);
            clear.TabIndex = 15;
            clear.Text = "Clear Fields";
            clear.Click += clear_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Pixel Operator Mono", 16.1999989F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.Peru;
            label2.Location = new Point(406, 110);
            label2.Name = "label2";
            label2.Size = new Size(147, 27);
            label2.TabIndex = 14;
            label2.Text = "analytics";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("BankGothic Md BT", 64.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Orange;
            label1.Location = new Point(2, 23);
            label1.Name = "label1";
            label1.Size = new Size(944, 114);
            label1.TabIndex = 8;
            label1.Text = "SNAKYWORKS";
            // 
            // Save
            // 
            Save.BackColor = Color.AliceBlue;
            Save.Cursor = Cursors.Hand;
            Save.Font = new Font("Pixel Operator SC", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Save.ForeColor = Color.DarkGreen;
            Save.Location = new Point(597, 350);
            Save.Name = "Save";
            Save.Size = new Size(151, 47);
            Save.TabIndex = 2;
            Save.Text = "Submit";
            Save.UseVisualStyleBackColor = false;
            Save.Click += Save_Click;
            // 
            // Cancel
            // 
            Cancel.Cursor = Cursors.Hand;
            Cancel.Font = new Font("Pixel Operator SC", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            Cancel.ForeColor = Color.SaddleBrown;
            Cancel.Location = new Point(202, 350);
            Cancel.Name = "Cancel";
            Cancel.Size = new Size(151, 47);
            Cancel.TabIndex = 1;
            Cancel.Text = "Cancel";
            Cancel.UseVisualStyleBackColor = true;
            Cancel.Click += Cancel_Click;
            // 
            // btnShow
            // 
            btnShow.Image = (Image)resources.GetObject("btnShow.Image");
            btnShow.Location = new Point(763, 222);
            btnShow.Name = "btnShow";
            btnShow.Size = new Size(53, 55);
            btnShow.TabIndex = 18;
            btnShow.UseVisualStyleBackColor = true;
            // 
            // btnHide
            // 
            btnHide.BackColor = Color.White;
            btnHide.FlatStyle = FlatStyle.Flat;
            btnHide.ForeColor = SystemColors.ActiveCaptionText;
            btnHide.Image = (Image)resources.GetObject("btnHide.Image");
            btnHide.Location = new Point(764, 222);
            btnHide.Name = "btnHide";
            btnHide.Size = new Size(52, 55);
            btnHide.TabIndex = 19;
            btnHide.UseVisualStyleBackColor = false;
            // 
            // tBPin
            // 
            tBPin.BackColor = SystemColors.MenuText;
            tBPin.Font = new Font("Pixel Operator Mono", 25.8000011F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tBPin.ForeColor = SystemColors.Info;
            tBPin.Location = new Point(180, 222);
            tBPin.Multiline = false;
            tBPin.Name = "tBPin";
            tBPin.PasswordChar = '■';
            tBPin.Size = new Size(636, 58);
            tBPin.TabIndex = 6;
            // 
            // panel9
            // 
            panel9.BackColor = Color.DarkGoldenrod;
            panel9.BorderStyle = BorderStyle.FixedSingle;
            panel9.Dock = DockStyle.Left;
            panel9.Location = new Point(0, 10);
            panel9.Name = "panel9";
            panel9.Size = new Size(10, 580);
            panel9.TabIndex = 12;
            // 
            // panel2
            // 
            panel2.BackColor = Color.DarkGoldenrod;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel2.Dock = DockStyle.Right;
            panel2.Location = new Point(990, 10);
            panel2.Name = "panel2";
            panel2.Size = new Size(10, 580);
            panel2.TabIndex = 11;
            // 
            // panel10
            // 
            panel10.BackColor = SystemColors.Control;
            panel10.BorderStyle = BorderStyle.FixedSingle;
            panel10.Dock = DockStyle.Right;
            panel10.Location = new Point(990, 0);
            panel10.Name = "panel10";
            panel10.Size = new Size(10, 10);
            panel10.TabIndex = 6;
            // 
            // panel1
            // 
            panel1.BackColor = Color.MistyRose;
            panel1.BackgroundImageLayout = ImageLayout.None;
            panel1.Controls.Add(panel10);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 590);
            panel1.Name = "panel1";
            panel1.Size = new Size(1000, 10);
            panel1.TabIndex = 10;
            // 
            // panel7
            // 
            panel7.BackColor = Color.MistyRose;
            panel7.BackgroundImageLayout = ImageLayout.None;
            panel7.Dock = DockStyle.Bottom;
            panel7.Location = new Point(0, -2);
            panel7.Name = "panel7";
            panel7.Size = new Size(998, 10);
            panel7.TabIndex = 4;
            // 
            // panel3
            // 
            panel3.BackColor = SystemColors.ActiveCaptionText;
            panel3.Controls.Add(panel);
            panel3.Controls.Add(panel12);
            panel3.Controls.Add(panel11);
            panel3.Controls.Add(panel6);
            panel3.Controls.Add(panel5);
            panel3.Cursor = Cursors.PanNW;
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(0, 10);
            panel3.Name = "panel3";
            panel3.Size = new Size(1000, 590);
            panel3.TabIndex = 13;
            // 
            // panel4
            // 
            panel4.BackColor = SystemColors.ButtonHighlight;
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel4.Controls.Add(panel7);
            panel4.Cursor = Cursors.PanNW;
            panel4.Dock = DockStyle.Top;
            panel4.Location = new Point(0, 0);
            panel4.Name = "panel4";
            panel4.Size = new Size(1000, 10);
            panel4.TabIndex = 9;
            // 
            // Form_LogIn
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 600);
            Controls.Add(panel9);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(panel3);
            Controls.Add(panel4);
            Name = "Form_LogIn";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form_LogIn";
            panel6.ResumeLayout(false);
            panel.ResumeLayout(false);
            panel.PerformLayout();
            panel1.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel4.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel12;
        private Panel panel11;
        private Panel panel6;
        private Panel panel8;
        private Panel panel5;
        private Label label5;
        private Panel panel;
        private Label clear;
        private Label label2;
        private Label label1;
        private TextBox tBPin;
        private Button Save;
        private Button Cancel;
        private Panel panel9;
        private Panel panel2;
        private Panel panel10;
        private Panel panel1;
        private Panel panel7;
        private Panel panel3;
        private Panel panel4;
        private Button btnHide;
        private Button btnShow;
    }
}

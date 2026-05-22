namespace GameForms
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            EXIT = new Button();
            START = new Label();
            pictureBox1 = new PictureBox();
            BEGIN = new Label();
            GameIntroHost = new Panel();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // EXIT
            // 
            EXIT.BackColor = Color.Black;
            EXIT.BackgroundImage = (Image)resources.GetObject("EXIT.BackgroundImage");
            EXIT.BackgroundImageLayout = ImageLayout.Center;
            EXIT.FlatStyle = FlatStyle.Flat;
            EXIT.Location = new Point(28, 734);
            EXIT.Margin = new Padding(3, 2, 3, 2);
            EXIT.Name = "EXIT";
            EXIT.Size = new Size(66, 56);
            EXIT.TabIndex = 0;
            EXIT.UseVisualStyleBackColor = false;
            EXIT.Click += EXIT_Click;
            // 
            // START
            // 
            START.AutoSize = true;
            START.BackColor = SystemColors.MenuText;
            START.Cursor = Cursors.PanNW;
            START.FlatStyle = FlatStyle.Popup;
            START.Font = new Font("Pixel Operator SC", 130.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            START.ForeColor = Color.Snow;
            START.Location = new Point(392, 280);
            START.Name = "START";
            START.Size = new Size(832, 174);
            START.TabIndex = 1;
            START.Text = "START GAME";
            START.Click += START_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Black;
            pictureBox1.BackgroundImage = (Image)resources.GetObject("pictureBox1.BackgroundImage");
            pictureBox1.Location = new Point(341, 425);
            pictureBox1.Margin = new Padding(3, 2, 3, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1005, 18);
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // BEGIN
            // 
            BEGIN.AutoSize = true;
            BEGIN.BackColor = Color.Black;
            BEGIN.Font = new Font("Pixel Operator SC", 40.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            BEGIN.ForeColor = SystemColors.ControlDarkDark;
            BEGIN.Location = new Point(682, 446);
            BEGIN.Name = "BEGIN";
            BEGIN.Size = new Size(325, 54);
            BEGIN.TabIndex = 3;
            BEGIN.Text = "CLICK TO BEGIN";
            BEGIN.Click += BEGIN_Click;
            // 
            // GameIntroHost
            // 
            GameIntroHost.BackColor = Color.Black;
            GameIntroHost.Location = new Point(-17, -4);
            GameIntroHost.Margin = new Padding(3, 2, 3, 2);
            GameIntroHost.Name = "GameIntroHost";
            GameIntroHost.Size = new Size(1773, 880);
            GameIntroHost.TabIndex = 26;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(1650, 791);
            Controls.Add(GameIntroHost);
            Controls.Add(BEGIN);
            Controls.Add(pictureBox1);
            Controls.Add(START);
            Controls.Add(EXIT);
            Cursor = Cursors.PanNW;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Finding ‘Nimo’ In A Silent Canvas";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button EXIT;
        private Label START;
        private PictureBox pictureBox1;
        private Label BEGIN;
        private Panel GameIntroHost;
    }
}

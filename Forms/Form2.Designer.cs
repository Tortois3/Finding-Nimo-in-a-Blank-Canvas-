namespace GameForms
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            pictureBox2 = new PictureBox();
            EXIT = new Button();
            ABOUT = new Button();
            HISTORY = new Button();
            button3 = new Button();
            button4 = new Button();
            button5 = new Button();
            USERINFO = new Button();
            label1 = new Label();
            GameIntroHost = new Panel();
            memoir = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = SystemColors.ActiveCaptionText;
            pictureBox2.Location = new Point(-19, 1022);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(1960, 74);
            pictureBox2.TabIndex = 11;
            pictureBox2.TabStop = false;
            // 
            // EXIT
            // 
            EXIT.BackColor = Color.Black;
            EXIT.BackgroundImage = (Image)resources.GetObject("EXIT.BackgroundImage");
            EXIT.BackgroundImageLayout = ImageLayout.Center;
            EXIT.FlatStyle = FlatStyle.Flat;
            EXIT.Location = new Point(40, 954);
            EXIT.Name = "EXIT";
            EXIT.Size = new Size(75, 75);
            EXIT.TabIndex = 12;
            EXIT.UseVisualStyleBackColor = false;
            EXIT.Click += EXIT_Click;
            // 
            // ABOUT
            // 
            ABOUT.BackColor = Color.Black;
            ABOUT.BackgroundImage = (Image)resources.GetObject("ABOUT.BackgroundImage");
            ABOUT.BackgroundImageLayout = ImageLayout.Center;
            ABOUT.FlatStyle = FlatStyle.Flat;
            ABOUT.Location = new Point(40, 867);
            ABOUT.Name = "ABOUT";
            ABOUT.Size = new Size(75, 75);
            ABOUT.TabIndex = 17;
            ABOUT.UseVisualStyleBackColor = false;
            ABOUT.Click += ABOUT_Click;
            // 
            // HISTORY
            // 
            HISTORY.BackColor = Color.Black;
            HISTORY.BackgroundImage = (Image)resources.GetObject("HISTORY.BackgroundImage");
            HISTORY.BackgroundImageLayout = ImageLayout.Center;
            HISTORY.FlatStyle = FlatStyle.Flat;
            HISTORY.Location = new Point(40, 775);
            HISTORY.Name = "HISTORY";
            HISTORY.Size = new Size(75, 75);
            HISTORY.TabIndex = 18;
            HISTORY.UseVisualStyleBackColor = false;
            HISTORY.Click += HISTORY_Click;
            // 
            // button3
            // 
            button3.BackColor = Color.Transparent;
            button3.Font = new Font("Pixel Operator SC", 48F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button3.Location = new Point(1449, 556);
            button3.Name = "button3";
            button3.Size = new Size(413, 89);
            button3.TabIndex = 20;
            button3.Text = "START GAME";
            button3.TextImageRelation = TextImageRelation.TextAboveImage;
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button4
            // 
            button4.Font = new Font("Pixel Operator SC", 48F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button4.Location = new Point(1421, 739);
            button4.Name = "button4";
            button4.Size = new Size(441, 89);
            button4.TabIndex = 21;
            button4.Text = "ACHIEVEMENT";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // button5
            // 
            button5.BackColor = Color.LimeGreen;
            button5.FlatStyle = FlatStyle.Flat;
            button5.Font = new Font("Pixel Operator SC", 30F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button5.Location = new Point(1647, 645);
            button5.Name = "button5";
            button5.Size = new Size(215, 61);
            button5.TabIndex = 22;
            button5.Text = "CONTINUE";
            button5.UseVisualStyleBackColor = false;
            button5.Click += button5_Click;
            // 
            // USERINFO
            // 
            USERINFO.BackColor = Color.Black;
            USERINFO.BackgroundImage = (Image)resources.GetObject("USERINFO.BackgroundImage");
            USERINFO.BackgroundImageLayout = ImageLayout.Center;
            USERINFO.FlatStyle = FlatStyle.Flat;
            USERINFO.Location = new Point(102, 682);
            USERINFO.Name = "USERINFO";
            USERINFO.Size = new Size(75, 75);
            USERINFO.TabIndex = 23;
            USERINFO.UseVisualStyleBackColor = false;
            USERINFO.Click += USERINFO_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Cursor = Cursors.PanNW;
            label1.Font = new Font("Pixel Operator SC", 36F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.DimGray;
            label1.Location = new Point(1559, 963);
            label1.Name = "label1";
            label1.Size = new Size(303, 60);
            label1.TabIndex = 24;
            label1.Text = "SNAKYWORKS";
            label1.Click += label1_Click;
            // 
            // GameIntroHost
            // 
            GameIntroHost.BackColor = Color.Black;
            GameIntroHost.Location = new Point(-62, 0);
            GameIntroHost.Name = "GameIntroHost";
            GameIntroHost.Size = new Size(2026, 1173);
            GameIntroHost.TabIndex = 25;
            // 
            // memoir
            // 
            memoir.BackColor = Color.LimeGreen;
            memoir.FlatStyle = FlatStyle.Flat;
            memoir.Font = new Font("Pixel Operator SC", 30F, FontStyle.Bold, GraphicsUnit.Point, 0);
            memoir.Location = new Point(1709, 834);
            memoir.Name = "memoir";
            memoir.Size = new Size(215, 61);
            memoir.TabIndex = 26;
            memoir.Text = "MEMOIRS";
            memoir.UseVisualStyleBackColor = false;
            memoir.Click += button1_Click;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(1886, 1055);
            Controls.Add(label1);
            Controls.Add(button5);
            Controls.Add(button4);
            Controls.Add(button3);
            Controls.Add(HISTORY);
            Controls.Add(ABOUT);
            Controls.Add(EXIT);
            Controls.Add(pictureBox2);
            Controls.Add(GameIntroHost);
            Controls.Add(USERINFO);
            Controls.Add(memoir);
            Cursor = Cursors.PanNW;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form2";
            Text = "Finding ‘Nimo’ In A Silent Canvas";
            WindowState = FormWindowState.Maximized;
            Load += Form2_Load;
            Controls.SetChildIndex(memoir, 0);
            Controls.SetChildIndex(USERINFO, 0);
            Controls.SetChildIndex(GameIntroHost, 0);
            Controls.SetChildIndex(pictureBox2, 0);
            Controls.SetChildIndex(EXIT, 0);
            Controls.SetChildIndex(ABOUT, 0);
            Controls.SetChildIndex(HISTORY, 0);
            Controls.SetChildIndex(button3, 0);
            Controls.SetChildIndex(button4, 0);
            Controls.SetChildIndex(button5, 0);
            Controls.SetChildIndex(label1, 0);
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox2;
        private Button EXIT;
        private PictureBox pictureBox1;
        private Button ABOUT;
        private Button HISTORY;
        private Button button3;
        private Button button4;
        private Button button5;
        private PictureBox pictureBox3;
        private Button USERINFO;
        private Label label1;
        private Panel GameIntroHost;
        private Button memoir;
    }
}

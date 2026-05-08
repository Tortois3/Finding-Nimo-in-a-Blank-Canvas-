namespace GameForms.Forms
{
    partial class Form_Analytic
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Analytic));
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            combo = new ComboBox();
            groupBox1 = new GroupBox();
            save = new Button();
            suggestion = new TextBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Cursor = Cursors.PanNW;
            label1.Font = new Font("Pixel Operator SC", 36F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Chartreuse;
            label1.Location = new Point(258, 63);
            label1.Name = "label1";
            label1.Size = new Size(733, 60);
            label1.TabIndex = 2;
            label1.Text = "Player Engagement Analytics";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Pixel Operator SC", 19.7999973F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = SystemColors.ButtonHighlight;
            label2.Location = new Point(32, 46);
            label2.Name = "label2";
            label2.Size = new Size(131, 33);
            label2.TabIndex = 3;
            label2.Text = "RATINGS*";
            label2.Click += label2_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Pixel Operator SC", 19.7999973F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.ForeColor = SystemColors.ButtonHighlight;
            label3.Location = new Point(26, 85);
            label3.Name = "label3";
            label3.Size = new Size(191, 33);
            label3.TabIndex = 4;
            label3.Text = "suggestions*";
            label3.Click += label3_Click;
            // 
            // combo
            // 
            combo.Font = new Font("Pixel Operator Mono", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            combo.FormattingEnabled = true;
            combo.Items.AddRange(new object[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" });
            combo.Location = new Point(248, 51);
            combo.Name = "combo";
            combo.Size = new Size(64, 28);
            combo.TabIndex = 5;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(save);
            groupBox1.Controls.Add(suggestion);
            groupBox1.Controls.Add(combo);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label2);
            groupBox1.Font = new Font("Pixel Operator SC", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            groupBox1.ForeColor = Color.Red;
            groupBox1.Location = new Point(753, 452);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(505, 267);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "Help me improve by filling these out:";
            // 
            // save
            // 
            save.Font = new Font("Pixel Operator SC", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            save.Location = new Point(428, 229);
            save.Name = "save";
            save.Size = new Size(71, 32);
            save.TabIndex = 7;
            save.Text = "SAVE";
            save.UseVisualStyleBackColor = true;
            save.Click += button1_Click;
            // 
            // suggestion
            // 
            suggestion.BackColor = SystemColors.InfoText;
            suggestion.ForeColor = Color.White;
            suggestion.Location = new Point(26, 121);
            suggestion.Multiline = true;
            suggestion.Name = "suggestion";
            suggestion.Size = new Size(450, 125);
            suggestion.TabIndex = 6;
            // 
            // Form_Analytic
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(1272, 732);
            Controls.Add(groupBox1);
            Controls.Add(label1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form_Analytic";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form_Analytic";
            Load += Form_Analytic_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private ComboBox combo;
        private GroupBox groupBox1;
        private TextBox suggestion;
        private Button save;
    }
}
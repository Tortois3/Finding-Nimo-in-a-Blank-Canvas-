namespace GameForms.Forms
{
    partial class Form_Creator
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
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = Color.Transparent;
            label1.Cursor = Cursors.PanNW;
            label1.Font = new Font("Pixel Operator SC", 72F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Chartreuse;
            label1.Location = new Point(-2, 619);
            label1.Name = "label1";
            label1.Size = new Size(512, 119);
            label1.TabIndex = 3;
            label1.Text = "SNAKY!SAN";
            // 
            // Form_Creator
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(1272, 732);
            Controls.Add(label1);
            Name = "Form_Creator";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form_Creator";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
    }
}
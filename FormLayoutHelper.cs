using System;
using System.Drawing;
using System.Windows.Forms;

namespace GameForms
{
    internal static class FormLayoutHelper
    {
        public static void Configure(Form form, bool allowAutoScroll = true)
        {
            if (form == null)
                return;

            form.AutoScaleMode = AutoScaleMode.Dpi;

            if (allowAutoScroll)
                form.AutoScroll = true;

            form.Shown -= Form_Shown;
            form.Shown += Form_Shown;
        }

        private static void Form_Shown(object? sender, EventArgs e)
        {
            if (sender is not Form form)
                return;

            ApplyFitToScreen(form);
        }

        private static void ApplyFitToScreen(Form form)
        {
            if (form.WindowState == FormWindowState.Maximized)
                return;

            Rectangle workingArea = Screen.FromControl(form).WorkingArea;
            const int margin = 24;
            int maxWidth = Math.Max(320, workingArea.Width - (margin * 2));
            int maxHeight = Math.Max(240, workingArea.Height - (margin * 2));

            if (form.Width <= maxWidth && form.Height <= maxHeight)
            {
                form.StartPosition = FormStartPosition.CenterScreen;
                return;
            }

            float widthScale = maxWidth / (float)Math.Max(1, form.Width);
            float heightScale = maxHeight / (float)Math.Max(1, form.Height);
            float scale = Math.Min(widthScale, heightScale);

            if (scale <= 0f || scale >= 1f)
            {
                form.StartPosition = FormStartPosition.CenterScreen;
                return;
            }

            Size originalSize = form.Size;
            form.SuspendLayout();
            form.Scale(new SizeF(scale, scale));
            form.ClientSize = new Size(
                Math.Min(maxWidth, form.ClientSize.Width),
                Math.Min(maxHeight, form.ClientSize.Height));
            form.MinimumSize = new Size(
                Math.Min(form.Width, originalSize.Width),
                Math.Min(form.Height, originalSize.Height));
            form.ResumeLayout(true);
            form.StartPosition = FormStartPosition.CenterScreen;
        }
    }
}

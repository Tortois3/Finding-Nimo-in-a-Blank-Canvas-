using System.Windows.Forms;

namespace GameForms
{
    internal static class FormEscapeCloseBehavior
    {
        public static void Attach(Form form)
        {
            form.KeyPreview = true;
            form.KeyDown -= Form_KeyDownCloseOnEscape;
            form.KeyDown += Form_KeyDownCloseOnEscape;
        }

        private static void Form_KeyDownCloseOnEscape(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Escape || sender is not Form form)
                return;

            e.SuppressKeyPress = true;
            e.Handled = true;
            form.Close();
        }
    }
}

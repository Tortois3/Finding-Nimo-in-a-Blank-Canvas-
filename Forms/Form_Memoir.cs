using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using GameForms;

namespace GameForms.Forms
{
    public partial class Form_Memoir : Form
    {
        private Panel[] _orderedPages = Array.Empty<Panel>();
        private TextBox[] _memoirTextBoxes = Array.Empty<TextBox>();
        private int _currentPageIndex;
        private bool _isLoadingMemoir;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                return cp;
            }
        }

        public Form_Memoir()
        {
            InitializeComponent();
            InitializeMemoirLayout();
            WireMemoirPersistence();
            Load += Form_Memoir_Load;
            FormEscapeCloseBehavior.Attach(this);

            this.FormBorderStyle = FormBorderStyle.None;


        }

        private void InitializeMemoirLayout()
        {
            _orderedPages = new[] { page0, page_1, page2, page3, page4, page5 };
            _memoirTextBoxes = new[] { textBox1, textBox3, textBox2, textBox4, textBox5 };

            Rectangle pageBounds = page0.Bounds;
            foreach (Panel page in _orderedPages)
            {
                page.Parent = this;
                page.Bounds = pageBounds;
                page.Visible = false;
            }

            panelAbout.Parent = this;
            panelAbout.Bounds = pageBounds;
            panelAbout.Visible = false;

            page0.BringToFront();
            ABOUT.BringToFront();
            label1.BringToFront();

            button4.Click += (_, _) => ShowMemoirPage(3);
            button2.Click -= button2_Click;
            button2.Click += (_, _) => ShowMemoirPage(1);
            next.Click -= next_Click;
            next.Click += (_, _) => ShowMemoirPage(2);
            button3.Click -= button3_Click;
            button3.Click += (_, _) => ShowMemoirPage(4);
            button5.Click -= button5_Click;
            button5.Click += (_, _) => ShowMemoirPage(5);
            button8.Click -= button8_Click_1;
            button8.Click += (_, _) => ShowMemoirPage(0);
            button11.Click -= button11_Click;
            button11.Click += (_, _) => ShowMemoirPage(2);
            button7.Click -= button7_Click;
            button7.Click += (_, _) => ShowMemoirPage(3);
            button10.Click -= button10_Click;
            button10.Click += (_, _) => ShowMemoirPage(4);
            button6.Click -= button6_Click;
            button6.Click += (_, _) => Close();
            button1.Click -= button1_Click;
            button1.Click += (_, _) => ShowMemoirPage(_currentPageIndex);

            ShowMemoirPage(0);
        }

        private void WireMemoirPersistence()
        {
            foreach (TextBox textBox in _memoirTextBoxes)
            {
                textBox.TextChanged -= MemoirTextBox_TextChanged;
                textBox.TextChanged += MemoirTextBox_TextChanged;
            }
        }

        private void ABOUT_Click(object sender, EventArgs e)
        {
            HideAllMemoirPanels();
            panelAbout.Show();
            panelAbout.BringToFront();
        }

        private void button2_Click(object sender, EventArgs e) { }

        private void textBox1_TextChanged(object sender, EventArgs e) { }

        private void next_Click(object sender, EventArgs e) { }

        private void button3_Click(object sender, EventArgs e) { }

        private void button5_Click(object sender, EventArgs e) { }

        private void button1_Click(object sender, EventArgs e) { }

        private void Form_Memoir_Load(object sender, EventArgs e)
        {
            LoadMemoirContent();
            ShowMemoirPage(0);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveMemoirContent();
            base.OnFormClosing(e);
        }

        private void MemoirTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (_isLoadingMemoir || !PlayerSession.HasActivePlayer)
                return;

            SaveMemoirContent();
        }

        private void ShowMemoirPage(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= _orderedPages.Length)
                return;

            _currentPageIndex = pageIndex;
            HideAllMemoirPanels();
            _orderedPages[pageIndex].Show();
            _orderedPages[pageIndex].BringToFront();
            ABOUT.BringToFront();
            label1.BringToFront();
        }

        private void HideAllMemoirPanels()
        {
            foreach (Panel page in _orderedPages)
                page.Hide();

            panelAbout.Hide();
        }

        private void LoadMemoirContent()
        {
            _isLoadingMemoir = true;
            try
            {
                foreach (TextBox textBox in _memoirTextBoxes)
                    textBox.Text = string.Empty;

                if (!PlayerSession.HasActivePlayer)
                    return;

                string path = PlayerSession.GetMemoirFilePath();
                if (!File.Exists(path))
                    return;

                MemoirEntry[]? entries = JsonSerializer.Deserialize<MemoirEntry[]>(File.ReadAllText(path));
                if (entries == null)
                    return;

                Dictionary<int, string> values = entries.ToDictionary(entry => entry.PageNumber, entry => entry.Text ?? string.Empty);
                textBox1.Text = GetMemoirText(values, 1);
                textBox3.Text = GetMemoirText(values, 2);
                textBox2.Text = GetMemoirText(values, 3);
                textBox4.Text = GetMemoirText(values, 4);
                textBox5.Text = GetMemoirText(values, 5);
            }
            finally
            {
                _isLoadingMemoir = false;
            }
        }

        private void SaveMemoirContent()
        {
            if (!PlayerSession.HasActivePlayer)
                return;

            string path = PlayerSession.GetMemoirFilePath();
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            MemoirEntry[] entries =
            {
                new MemoirEntry(1, textBox1.Text),
                new MemoirEntry(2, textBox3.Text),
                new MemoirEntry(3, textBox2.Text),
                new MemoirEntry(4, textBox4.Text),
                new MemoirEntry(5, textBox5.Text)
            };

            string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private static string GetMemoirText(Dictionary<int, string> values, int pageNumber)
        {
            return values.TryGetValue(pageNumber, out string? text) ? text : string.Empty;
        }

        private sealed class MemoirEntry
        {
            public MemoirEntry() { }

            public MemoirEntry(int pageNumber, string text)
            {
                PageNumber = pageNumber;
                Text = text;
            }

            public int PageNumber { get; set; }
            public string Text { get; set; } = string.Empty;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)//page 4 prev page
        {
            ShowMemoirPage(3);
        }

        private void button10_Click(object sender, EventArgs e) //page 5 prev page
        {
            ShowMemoirPage(4);
        }

        private void button11_Click(object sender, EventArgs e) //page 3 prev page
        {
            ShowMemoirPage(2);
        }

        private void button8_Click_1(object sender, EventArgs e) //page 2 prev page
        {
            ShowMemoirPage(0);
        }
    }
}

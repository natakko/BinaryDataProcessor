using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KonturTask
{
    public partial class Form1 : Form
    {
        private TextBox txt1 = new TextBox { Top = 20, Left = 20, Width = 200, Text = "Выберите файл 1..." };
        private Button btnBrowse1 = new Button { Text = "...", Top = 20, Left = 230, Width = 30 };
        private Button btnGo1 = new Button { Text = "Пуск задачи 1", Top = 50, Left = 20, Width = 240 };

        private TextBox txt2 = new TextBox { Top = 100, Left = 20, Width = 200, Text = "Выберите файл 2..." };
        private Button btnBrowse2 = new Button { Text = "...", Top = 100, Left = 230, Width = 30 };
        private Button btnGo2 = new Button { Text = "Пуск задачи 2", Top = 130, Left = 20, Width = 240 };

        private ProgressBar pb = new ProgressBar { Top = 180, Left = 20, Width = 240 };

        public Form1()
        {
            this.Text = "Контур Тестовое"; this.Width = 300; this.Height = 260;
            this.Controls.AddRange(new Control[] { txt1, btnBrowse1, btnGo1, txt2, btnBrowse2, btnGo2, pb });

            btnBrowse1.Click += (s, e) => SelectFile(txt1);
            btnBrowse2.Click += (s, e) => SelectFile(txt2);
            btnGo1.Click += async (s, e) => await Run(1, txt1.Text, "Result1.csv");
            btnGo2.Click += async (s, e) => await Run(2, txt2.Text, "Result2.csv");
        }

        private void SelectFile(TextBox t)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK) t.Text = ofd.FileName;
        }

        async Task Run(int task, string input, string output)
        {
            if (!File.Exists(input)) { MessageBox.Show("Файл не выбран или не существует!"); return; }
            pb.Style = ProgressBarStyle.Marquee;
            try
            {
                if (task == 1) await Task.Run(() => Process1(input, output));
                else await Task.Run(() => Process2(input, output));
                MessageBox.Show($"Успех! Файл сохранен рядом с исходным.");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { pb.Style = ProgressBarStyle.Blocks; }
        }

        void Process1(string input, string output)
        {
            var outPath = Path.Combine(Path.GetDirectoryName(input), output);
            using var reader = new BinaryReader(File.Open(input, FileMode.Open));
            using var writer = new StreamWriter(File.Open(outPath, FileMode.Create), Encoding.UTF8);
            writer.WriteLine("Packet;Channel 1;Channel 2;Channel 3;Channel 4;Channel 5;Channel 6");
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                reader.ReadUInt64(); uint p = reader.ReadUInt32(); reader.ReadUInt32();
                double[] s = new double[6];
                for (int i = 0; i < 60; i++) for (int c = 0; c < 6; c++) s[c] += reader.ReadInt32();
                writer.WriteLine($"{p};{string.Join(";", s.Select(v => (long)(v / 60.0)))}");
            }
        }

        void Process2(string input, string output)
        {
            var outPath = Path.Combine(Path.GetDirectoryName(input), output);
            using var reader = new BinaryReader(File.Open(input, FileMode.Open));
            using var writer = new StreamWriter(File.Open(outPath, FileMode.Create), Encoding.UTF8);
            writer.WriteLine("IsEnabled1;Value11;Value12;Value13;IsEnabled2;Value21;Value22");
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                uint d = reader.ReadUInt32();
                writer.WriteLine($"{(d & 1) == 1};{(d >> 1) & 7};{(d >> 4) & 7};{(d >> 7) & 0x1FF};{((d >> 16) & 1) == 1};{(d >> 17) & 0x7FF};{(d >> 28) & 7}");
            }
        }
    }
}

using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace IColorify
{
    public partial class MainForm : MetroSuite.MetroForm
    {
        private int _imageAlteredPixels = 0;

        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            new Thread(UpdateThread).Start();
        }

        private void UpdateThread()
        {
            while (true)
            {
                Thread.Sleep(1);
                label2.Text = $"{guna2TextBox1.Text.Length.ToString()} / 261870";
                label3.Text = $"{_imageAlteredPixels} / 262144";
                label5.Text = guna2TextBox3.Text.Length.ToString();
                label7.Text = guna2TextBox2.Text.Length.ToString();
            }
        }

        private void MainForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = guna2GradientButton1.Text;
            openFileDialog1.FileName = "";

            if (!openFileDialog1.ShowDialog().Equals(System.Windows.Forms.DialogResult.OK))
            {
                return;
            }

            LoadImage(openFileDialog1.FileName);
        }

        private void guna2GradientButton2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = guna2GradientButton2.Text;
            saveFileDialog1.FileName = "";

            if (!saveFileDialog1.ShowDialog().Equals(System.Windows.Forms.DialogResult.OK))
            {
                return;
            }

            pictureBox1.BackgroundImage.Save(saveFileDialog1.FileName);
        }

        private void guna2GradientButton8_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = guna2GradientButton8.Text;
            openFileDialog1.FileName = "";

            if (!openFileDialog1.ShowDialog().Equals(System.Windows.Forms.DialogResult.OK))
            {
                return;
            }

            guna2TextBox1.Text = File.ReadAllText(openFileDialog1.FileName);
        }

        private void guna2GradientButton7_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Title = guna2GradientButton7.Text;
            saveFileDialog1.FileName = "";

            if (!saveFileDialog1.ShowDialog().Equals(System.Windows.Forms.DialogResult.OK))
            {
                return;
            }

            File.WriteAllText(saveFileDialog1.FileName, guna2TextBox1.Text);
        }

        private void guna2GradientButton4_Click(object sender, EventArgs e)
        {
            LoadImage(Clipboard.GetImage());
        }

        private void guna2GradientButton3_Click(object sender, EventArgs e)
        {
            Clipboard.SetData(DataFormats.Bitmap, pictureBox1.BackgroundImage);
        }

        private void guna2GradientButton6_Click(object sender, EventArgs e)
        {
            guna2TextBox1.Text = Clipboard.GetText();
        }

        private void guna2GradientButton5_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(guna2TextBox1.Text);
        }

        private void LoadImage(string filePath)
        {
            LoadImage(Image.FromFile(filePath));
        }

        private void LoadImage(Image image)
        {
            LoadImage((Bitmap)image);
        }

        private void LoadImage(Bitmap bitmap)
        {
            pictureBox1.BackgroundImage = bitmap;
            _imageAlteredPixels = IColorifyCore.GetAlteredPixels(bitmap);
        }

        private void guna2GradientButton10_Click(object sender, EventArgs e)
        {
            Tuple<Bitmap, int> encoded = IColorifyCore.Encode(guna2TextBox1.Text, guna2TextBox2.Text, guna2TextBox3.Text);
            pictureBox1.BackgroundImage = encoded.Item1;
            _imageAlteredPixels = encoded.Item2;
        }

        private void guna2GradientButton9_Click(object sender, EventArgs e)
        {
            guna2TextBox1.Text = IColorifyCore.Decode((Bitmap) pictureBox1.BackgroundImage, guna2TextBox2.Text, guna2TextBox3.Text);
        }
    }
}
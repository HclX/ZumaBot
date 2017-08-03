using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ZumaBot
{
    public partial class MainForm : Form
    {
        Zuma.Player _gameState = new Zuma.Player(new Zuma.GameInfo("D:\\Games\\Zuma"));
        UInt32 _frameIndex;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnCapture_Click(object sender, EventArgs e)
        {
            try
            {
                Directory.Delete("screenshot", true);
            }
            catch (Exception)
            {
            }
            
            timerCapture.Interval = 1000;
            timerCapture.Start();
        }

        private void saveScreenShot(UInt32 index, Bitmap ss)
        {
            Directory.CreateDirectory("screenshot");
            var fname = Path.Combine("screenshot", string.Format("{0}.jpg", index));
            ss.Save(fname);
        }

        private bool loadScreenShot(UInt32 index)
        {
            var fname = Path.Combine("screenshot", string.Format("{0}.jpg", index));
            if (!File.Exists(fname))
            {
                return false;
            }

            var ss = new Bitmap(fname);
            _gameState.Simulate(currentScreen, ss);

            textLevelId.Text = _gameState.LevelId;
            textLifeCount.Text = _gameState.LifeCount;

            return true;
        }

        private void timerCapture_Tick(object sender, EventArgs e)
        {
            _gameState.Play(currentScreen);
            textLevelId.Text = _gameState.LevelId;
            textLifeCount.Text = _gameState.LifeCount;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timerCapture.Stop();

            _frameIndex = 0;
            if (!loadScreenShot(_frameIndex))
            {
                MessageBox.Show("No screenshot captured!");
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (_frameIndex > 0)
            {
                _frameIndex--;
                if (!loadScreenShot(_frameIndex))
                {
                    _frameIndex++;
                    MessageBox.Show("No screenshot captured!");
                }
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            _frameIndex++;
            if (!loadScreenShot(_frameIndex))
            {
                _frameIndex--;
                MessageBox.Show("No screenshot captured!");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnBrowseDir_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {

            }
        }
    }
}

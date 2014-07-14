using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Tennis01
{
    public partial class GameConfig : Form
    {
        public GameConfig()
        {
            InitializeComponent();
            MyInit();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            GameWidth = 640;
            IsFullScreen = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            GameWidth = 960;
            IsFullScreen = false;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            GameWidth = 1280;
            IsFullScreen = false;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            GameWidth = 1600;
            IsFullScreen = false;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            GameWidth = 960;
            IsFullScreen = true;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Dispose();
        }

    }
}

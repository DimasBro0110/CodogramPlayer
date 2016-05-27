using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;

namespace CodogrammPlayer
{

    public partial class Form1 : Form
    {
        public Dictionary<int, string> channelsDesc = new Dictionary<int, string>();

        public void downloadParameters()
        {
            try
            {
                Encoding enc = Encoding.GetEncoding(1251);
                string line = "";
                using (StreamReader sr = new StreamReader("FILES_INIT\\CHANNELS_DESCRIBE.txt", enc))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] str_parsed = line.Split(' ');
                        int num_channel = Convert.ToInt32(str_parsed[0]);
                        channelsDesc.Add(num_channel, str_parsed[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public Form1()
        {
            InitializeComponent();
            downloadParameters();
        }

        private void btn_KOD_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(NumericUpDown_Channel_Number.Value);
            string channel_describe = channelsDesc[channel_num];
        }

        private void btn_NORMA_Click(object sender, EventArgs e)
        {

        }
    }
}

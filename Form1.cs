using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;

namespace CodogrammPlayer
{
    public struct WAVEHEADER
    {
        public byte[] riffID; 
        public uint size;  
        public byte[] wavID;  
        public byte[] fmtID;  
        public uint fmtSize; 
        public ushort format; 
        public ushort channels; 
        public uint sampleRate; 
        public uint bytePerSec; 
        public ushort blockSize; 
        public ushort bit;  
        public byte[] dataID; 
        public uint dataSize; 
    };


    public partial class Form1 : Form
    {
        public Dictionary<int, String> numberChannels = new Dictionary<int, string>();
        public Dictionary<int, String> cods_3_8_Describe = new Dictionary<int, string>();
        public List<short> bit_thread = new List<short>();
        public WAVEHEADER header;
        public SoundPlayer sp = new SoundPlayer();

        void fromFileDowloader(Dictionary<int, String> channels, Dictionary<int, String> cods_3_8)
        {
            Encoding enc = Encoding.GetEncoding(1251);
            StreamReader streamReader = null;
            StreamReader streamReader_2 = null;
            try
            {
                streamReader = new StreamReader("FILES_INIT\\CHANNELS_DESCRIBE.txt", enc);
                streamReader_2 = new StreamReader("FILES_INIT\\NUMBER_3_7_GROUP.txt", enc);

                string line = "";

                while((line = streamReader.ReadLine()) != null)
                {
                    string[] splitted = line.Split(' ');
                    int num_channel = Convert.ToInt32(splitted[0]);
                    channels.Add(num_channel, splitted[1]);
                }

                line = "";

                while ((line = streamReader_2.ReadLine()) != null)
                {
                    string[] splitted = line.Split(' ');
                    int num = Convert.ToInt32(splitted[0]);
                    cods_3_8.Add(num, splitted[1]);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error Occured", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        List<short> to_bit_converter(string bit_str)
        {
            bool isDown = true;
            List<short> temp = new List<short>();
            Int16 ch_opos = -0x4000, ch_poss = 0x4000;

            foreach(char ch in bit_str)
            {
                if (ch == '1' && isDown == true) // seq 11110000
                {

                    for (int i = 0; i < 4; i++)
                    {
                        temp.Add(ch_poss);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        temp.Add(ch_opos);
                    }
                    isDown = true;
                }
                else if (ch == '1' && isDown == false)
                {

                    for (int i = 0; i < 4; i++)
                    {
                        temp.Add(ch_opos);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        temp.Add(ch_poss);
                    }
                    isDown = false;
                }
                else if (ch == '0' && isDown == true)
                {

                    for (int i = 0; i < 4; i++)
                    {
                        temp.Add(ch_opos);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        temp.Add(ch_poss);
                    }
                    isDown = false;
                }
                else if (ch == '0' && isDown == false)
                {

                    for (int i = 0; i < 4; i++)
                    {
                        temp.Add(ch_poss);
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        temp.Add(ch_opos);
                    }
                    isDown = true;
                }
            }

            return temp;
        }

        void waveFileCreation(List<short> temp, WAVEHEADER header)
        {
            FileStream fs = null;
            BinaryWriter bw = null;
            Encoding enc = Encoding.BigEndianUnicode;

            try
            {
                fs = new FileStream("WAVS\\current.wav", FileMode.Create);
                bw = new BinaryWriter(fs);

                bw.Write(header.riffID);
                bw.Write(header.size);
                bw.Write(header.wavID);
                bw.Write(header.fmtID);
                bw.Write(header.fmtSize);
                bw.Write(header.format);
                bw.Write(header.channels);
                bw.Write(header.sampleRate);
                bw.Write(header.bytePerSec);
                bw.Write(header.blockSize);
                bw.Write(header.bit);
                bw.Write(header.dataID);
                bw.Write(header.dataSize);

                for (int i = 0; i < temp.Count; i++)
                {
                    bw.Write(temp[i]);
                }

                bw.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public WAVEHEADER WAVEHEADER_INIT()
        {
            FileStream fs = null; // new FileStream("WAVS\\av_afu.wav", FileMode.Open, FileAccess.Read);
            BinaryReader br = null;
            WAVEHEADER Header = new WAVEHEADER();

            try
            {
                fs = new FileStream("WAVS\\av_afu.wav", FileMode.Open, FileAccess.Read);
                br = new BinaryReader(fs);

                Header.riffID = br.ReadBytes(4);
                Header.size = br.ReadUInt32();
                Header.wavID = br.ReadBytes(4);
                Header.fmtID = br.ReadBytes(4);
                Header.fmtSize = br.ReadUInt32();
                Header.format = br.ReadUInt16();
                Header.channels = br.ReadUInt16();
                Header.sampleRate = br.ReadUInt32();
                Header.bytePerSec = br.ReadUInt32();
                Header.blockSize = br.ReadUInt16();
                Header.bit = br.ReadUInt16();
                Header.dataID = br.ReadBytes(4);
                Header.dataSize = br.ReadUInt32();

                br.Close();
                fs.Close();

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return Header;

        }

        public Form1()
        {
            InitializeComponent();
            fromFileDowloader(numberChannels, cods_3_8_Describe);
            
            header = WAVEHEADER_INIT();           
        }

        private void btn_KOD_Click(object sender, EventArgs e)
        {

            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);
            if (channel_num > 0 && channel_num < 13)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[3];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[0];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[0];
                string WholeCode = header_part + channel_part + 
                    third_part + fourth_part + 
                    fifth_part + sixth_part + seventh_part;
                WholeCode += WholeCode;
                bit_thread = to_bit_converter(WholeCode);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            this.sp.Stop();
        }

        private void btn_IZB_VIZOV_Click(object sender, EventArgs e)
        {
            int number_AC_1 = Convert.ToInt32(this.NumericUpDown_AC_1.Value);
            int number_AC_2 = Convert.ToInt32(this.NumericUpDown_AC_2.Value);
            int number_AC_3 = Convert.ToInt32(this.NumericUpDown_AC_3.Value);
            int number_AC_4 = Convert.ToInt32(this.NumericUpDown_AC_4.Value);
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if ((number_AC_1 >= 0 && number_AC_1 < 10) && (number_AC_2 >= 0 && number_AC_2 < 10) &&
                 (number_AC_3 >= 0 && number_AC_3 < 10) && (number_AC_4 >= 0 && number_AC_4 < 10))
            {
                if (channel_num > 0 && channel_num < 13)
                {
                    this.sp.Stop();

                    string channel_part = numberChannels[channel_num];
                    string header_part = "1111100";
                    string third_part = cods_3_8_Describe[0];
                    string fourth_part = cods_3_8_Describe[number_AC_1];
                    string fifth_part = cods_3_8_Describe[number_AC_2];
                    string sixth_part = cods_3_8_Describe[number_AC_3];
                    string seventh_part = cods_3_8_Describe[number_AC_4];
                    string WholeCod = header_part + channel_part + 
                        third_part + fourth_part + 
                        fifth_part + sixth_part + seventh_part;
                    WholeCod += WholeCod;
                    
                    bit_thread = to_bit_converter(WholeCod);
                    waveFileCreation(bit_thread, header);

                    this.sp.SoundLocation = @"WAVS\\current.wav";
                    this.sp.PlayLooping();
                }
                else
                {
                    MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("The number of AC channels must be between 0 and 9", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_IZB_VIZOV_CIRK_Click(object sender, EventArgs e)
        {
            int number_AC_3 = Convert.ToInt32(this.NumericUpDown_AC_3.Value);
            int number_AC_4 = Convert.ToInt32(this.NumericUpDown_AC_4.Value);
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if ((number_AC_3 >= 0 && number_AC_3 <= 9) && (number_AC_3 >= 0 && number_AC_4 <= 9))
            {
                if (channel_num >= 0 && channel_num <= 12)
                {
                    this.sp.Stop();

                    string channel_part = numberChannels[channel_num];
                    string header_part = "1111100";
                    string third_part = cods_3_8_Describe[0];
                    string fourth_part = cods_3_8_Describe[9];
                    string fifth_part = cods_3_8_Describe[9];
                    string sixth_part = cods_3_8_Describe[number_AC_3];
                    string seventh_part = cods_3_8_Describe[number_AC_4];
                    string WholeCod = header_part + channel_part +
                        third_part + fourth_part +
                        fifth_part + sixth_part + seventh_part;
                    WholeCod += WholeCod;

                    bit_thread = to_bit_converter(WholeCod);
                    waveFileCreation(bit_thread, header);

                    this.sp.SoundLocation = @"WAVS\\current.wav";
                    this.sp.PlayLooping();
                }
                else
                {
                    MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("The number of AC channels must be between 0 and 9", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_SOC_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[7];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[0];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[0];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_SZKO_Click(object sender, EventArgs e)
        {
            int number_AC_1 = Convert.ToInt32(this.NumericUpDown_AC_1.Value);
            int number_AC_2 = Convert.ToInt32(this.NumericUpDown_AC_2.Value);
            int number_AC_3 = Convert.ToInt32(this.NumericUpDown_AC_3.Value);
            int number_AC_4 = Convert.ToInt32(this.NumericUpDown_AC_4.Value);
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if ((number_AC_1 >= 0 && number_AC_1 < 10) && (number_AC_2 >= 0 && number_AC_2 < 10) &&
                 (number_AC_3 >= 0 && number_AC_3 < 10) && (number_AC_4 >= 0 && number_AC_4 < 10))
            {
                if(channel_num >= 1 && channel_num <= 12)
                {
                    this.sp.Stop();

                    string channel_part = numberChannels[channel_num];
                    string header_part = "1111100";
                    string third_part = cods_3_8_Describe[2];
                    string fourth_part = cods_3_8_Describe[number_AC_1];
                    string fifth_part = cods_3_8_Describe[number_AC_2];
                    string sixth_part = cods_3_8_Describe[number_AC_3];
                    string seventh_part = cods_3_8_Describe[number_AC_4];
                    string WholeCod = header_part + channel_part +
                        third_part + fourth_part +
                        fifth_part + sixth_part + seventh_part;
                    WholeCod += WholeCod;

                    bit_thread = to_bit_converter(WholeCod);
                    waveFileCreation(bit_thread, header);

                    this.sp.SoundLocation = @"WAVS\\current.wav";
                    this.sp.PlayLooping();
                }
                else
                {
                    MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("The number of AC channels must be between 0 and 9", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_SZKA_Click(object sender, EventArgs e)
        {
            int number_AC_1 = Convert.ToInt32(this.NumericUpDown_AC_1.Value);
            int number_AC_2 = Convert.ToInt32(this.NumericUpDown_AC_2.Value);
            int number_AC_3 = Convert.ToInt32(this.NumericUpDown_AC_3.Value);
            int number_AC_4 = Convert.ToInt32(this.NumericUpDown_AC_4.Value);
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if ((number_AC_1 >= 0 && number_AC_1 < 10) && (number_AC_2 >= 0 && number_AC_2 < 10) &&
                 (number_AC_3 >= 0 && number_AC_3 < 10) && (number_AC_4 >= 0 && number_AC_4 < 10))
            {
                if (channel_num >= 0 && channel_num <= 12)
                {
                    this.sp.Stop();

                    string channel_part = numberChannels[channel_num];
                    string header_part = "1111100";
                    string third_part = cods_3_8_Describe[9];
                    string fourth_part = cods_3_8_Describe[number_AC_1];
                    string fifth_part = cods_3_8_Describe[number_AC_2];
                    string sixth_part = cods_3_8_Describe[number_AC_3];
                    string seventh_part = cods_3_8_Describe[number_AC_4];
                    string WholeCod = header_part + channel_part +
                        third_part + fourth_part +
                        fifth_part + sixth_part + seventh_part;
                    WholeCod += WholeCod;

                    bit_thread = to_bit_converter(WholeCod);
                    waveFileCreation(bit_thread, header);

                    this.sp.SoundLocation = @"WAVS\\current.wav";
                    this.sp.PlayLooping();
                }
                else
                {
                    MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("The number of AC channels must be between 0 and 9", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_KTO_Click(object sender, EventArgs e)
        {
            int number_AC_1 = Convert.ToInt32(this.NumericUpDown_AC_1.Value);
            int number_AC_2 = Convert.ToInt32(this.NumericUpDown_AC_2.Value);
            int number_AC_3 = Convert.ToInt32(this.NumericUpDown_AC_3.Value);
            int number_AC_4 = Convert.ToInt32(this.NumericUpDown_AC_4.Value);
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if ((number_AC_1 >= 0 && number_AC_1 < 10) && (number_AC_2 >= 0 && number_AC_2 < 10) &&
                 (number_AC_3 >= 0 && number_AC_3 < 10) && (number_AC_4 >= 0 && number_AC_4 < 10))
            {
                if (channel_num >= 0 && channel_num <= 12)
                {
                    this.sp.Stop();

                    string channel_part = numberChannels[channel_num];
                    string header_part = "1111100";
                    string third_part = cods_3_8_Describe[9];
                    string fourth_part = cods_3_8_Describe[number_AC_1];
                    string fifth_part = cods_3_8_Describe[number_AC_2];
                    string sixth_part = cods_3_8_Describe[number_AC_3];
                    string seventh_part = cods_3_8_Describe[number_AC_4];
                    string WholeCod = header_part + channel_part +
                        third_part + fourth_part +
                        fifth_part + sixth_part + seventh_part;
                    WholeCod += WholeCod;

                    bit_thread = to_bit_converter(WholeCod);
                    waveFileCreation(bit_thread, header);

                    this.sp.SoundLocation = @"WAVS\\current.wav";
                    this.sp.PlayLooping();
                }
                else
                {
                    MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("The number of AC channels must be between 0 and 9", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_SOA_Click(object sender, EventArgs e)
        {
            int number_AC_1 = Convert.ToInt32(this.NumericUpDown_AC_1.Value);
            int number_AC_2 = Convert.ToInt32(this.NumericUpDown_AC_2.Value);
            int number_AC_3 = Convert.ToInt32(this.NumericUpDown_AC_3.Value);
            int number_AC_4 = Convert.ToInt32(this.NumericUpDown_AC_4.Value);
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if ((number_AC_1 >= 0 && number_AC_1 < 10) && (number_AC_2 >= 0 && number_AC_2 < 10) &&
                 (number_AC_3 >= 0 && number_AC_3 < 10) && (number_AC_4 >= 0 && number_AC_4 < 10))
            {
                if (channel_num >= 0 && channel_num <= 12)
                {
                    this.sp.Stop();

                    string channel_part = numberChannels[channel_num];
                    string header_part = "1111100";
                    string third_part = cods_3_8_Describe[1];
                    string fourth_part = cods_3_8_Describe[number_AC_1];
                    string fifth_part = cods_3_8_Describe[number_AC_2];
                    string sixth_part = cods_3_8_Describe[number_AC_3];
                    string seventh_part = cods_3_8_Describe[number_AC_4];
                    string WholeCod = header_part + channel_part +
                        third_part + fourth_part +
                        fifth_part + sixth_part + seventh_part;
                    WholeCod += WholeCod;

                    bit_thread = to_bit_converter(WholeCod);
                    waveFileCreation(bit_thread, header);

                    this.sp.SoundLocation = @"WAVS\\current.wav";
                    this.sp.PlayLooping();
                }
                else
                {
                    MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("The number of AC channels must be between 0 and 9", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_SINHR_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[0];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[0];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[0];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_NORMA_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[1];
                string seventh_part = cods_3_8_Describe[3];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_M1_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[6];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[0];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[0];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_M2_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[4];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[0];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[0];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_SM_1_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[5];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[0];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[0];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_SM2_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[1];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[0];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[0];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_SNI_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[0];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[8];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_SOI_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[0];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[9];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_ZPR_S_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[1];
                string seventh_part = cods_3_8_Describe[3];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_ZPR_REGH_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[3];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[0];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_REGH_1_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[1];
                string fifth_part = cods_3_8_Describe[3];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[0];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_REGH_2_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[2];
                string fifth_part = cods_3_8_Describe[3];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[0];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_REGH_4_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[4];
                string fifth_part = cods_3_8_Describe[3];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[0];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_REGH_5_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[5];
                string fifth_part = cods_3_8_Describe[3];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[0];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_REGH_6_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[6];
                string fifth_part = cods_3_8_Describe[3];
                string sixth_part = cods_3_8_Describe[0];
                string seventh_part = cods_3_8_Describe[0];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_KPV_Click(object sender, EventArgs e)
        {
            int number_AC_1 = Convert.ToInt32(this.NumericUpDown_AC_1.Value);
            int number_AC_2 = Convert.ToInt32(this.NumericUpDown_AC_2.Value);
            int number_AC_3 = Convert.ToInt32(this.NumericUpDown_AC_3.Value);
            int number_AC_4 = Convert.ToInt32(this.NumericUpDown_AC_4.Value);
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if ((number_AC_1 >= 0 && number_AC_1 < 10) && (number_AC_2 >= 0 && number_AC_2 < 10) &&
                 (number_AC_3 >= 0 && number_AC_3 < 10) && (number_AC_4 >= 0 && number_AC_4 < 10))
            {
                if (channel_num >= 0 && channel_num <= 12)
                {
                    this.sp.Stop();

                    string channel_part = numberChannels[channel_num];
                    string header_part = "1111100";
                    string third_part = cods_3_8_Describe[9];
                    string fourth_part = cods_3_8_Describe[number_AC_1];
                    string fifth_part = cods_3_8_Describe[number_AC_2];
                    string sixth_part = cods_3_8_Describe[number_AC_3];
                    string seventh_part = cods_3_8_Describe[number_AC_4];
                    string WholeCod = header_part + channel_part +
                        third_part + fourth_part +
                        fifth_part + sixth_part + seventh_part;
                    WholeCod += WholeCod;

                    bit_thread = to_bit_converter(WholeCod);
                    waveFileCreation(bit_thread, header);

                    this.sp.SoundLocation = @"WAVS\\current.wav";
                    this.sp.PlayLooping();
                }
                else
                {
                    MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("The number of AC channels must be between 0 and 9", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_PDT_VKL_PRM_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[3];
                string sixth_part = cods_3_8_Describe[1];
                string seventh_part = cods_3_8_Describe[1];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_VKL_PRM_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[3];
                string sixth_part = cods_3_8_Describe[1];
                string seventh_part = cods_3_8_Describe[1];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_VKL_K1_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[1];
                string seventh_part = cods_3_8_Describe[5];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_VKL_PRD_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[1];
                string seventh_part = cods_3_8_Describe[1];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_VIKL_PRD_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[1];
                string seventh_part = cods_3_8_Describe[2];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_VIKL_K_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[1];
                string seventh_part = cods_3_8_Describe[6];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_PDT_VKL_PRD_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[1];
                string seventh_part = cods_3_8_Describe[1];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_PDT_VIKL_PRD_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[1];
                string seventh_part = cods_3_8_Describe[2];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_PDT_VKL_K1_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[1];
                string seventh_part = cods_3_8_Describe[5];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_PDT_VIKL_K_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[1];
                string seventh_part = cods_3_8_Describe[6];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_POGH_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[2];
                string seventh_part = cods_3_8_Describe[1];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_VD_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[2];
                string seventh_part = cods_3_8_Describe[2];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_AV_AFU_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[2];
                string seventh_part = cods_3_8_Describe[3];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_AV_FP_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[2];
                string seventh_part = cods_3_8_Describe[4];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_AV_PRD_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[2];
                string seventh_part = cods_3_8_Describe[6];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_AV_PRM_Click(object sender, EventArgs e)
        {
            int channel_num = Convert.ToInt32(this.NumericUpDown_Channel_Number.Value);

            if (channel_num >= 1 && channel_num <= 12)
            {
                this.sp.Stop();

                string channel_part = numberChannels[channel_num];
                string header_part = "1111100";
                string third_part = cods_3_8_Describe[8];
                string fourth_part = cods_3_8_Describe[0];
                string fifth_part = cods_3_8_Describe[1];
                string sixth_part = cods_3_8_Describe[2];
                string seventh_part = cods_3_8_Describe[7];
                string WholeCod = header_part + channel_part +
                    third_part + fourth_part +
                    fifth_part + sixth_part + seventh_part;
                WholeCod += WholeCod;

                bit_thread = to_bit_converter(WholeCod);
                waveFileCreation(bit_thread, header);

                this.sp.SoundLocation = @"WAVS\\current.wav";
                this.sp.PlayLooping();
            }
            else
            {
                MessageBox.Show("The number of channel must be between 1 and 12", "Error Occured",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}

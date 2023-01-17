using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace CMSC_169_Project_1
{
    public partial class MainForm : Form
    {
        #region Global Variables
        //Global Variables
        Bitmap pcxBMP;
        int[,] rVAL, gVAL, bVAL;
        #endregion

        #region Initalization
        public MainForm()
        {
            InitializeComponent();

            //Initially make Strip Menu Items false unless a image is loaded
            histogramToolStripMenuItem.Enabled = false;
            processToolStripMenuItem.Enabled = false;
            filterToolStripMenuItem.Enabled = false;
            restorationToolStripMenuItem.Enabled = false;
        }
        #endregion

        //If user clicks File > Open
        //Allows user to open a new file and getting the PXC Header information
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "C:\\Users\\Pinkosh\\Desktop\\CMSC 162";
                openFileDialog.Filter = "pcx files (*.pcx)|*.pcx|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    string filePath = openFileDialog.FileName;
                    PCXHeaderEncoder(filePath);
                    ColorPalette(filePath);
                    ProcessRGB();

                    //Enable Strip Menu Items
                    histogramToolStripMenuItem.Enabled = true;
                    processToolStripMenuItem.Enabled = true;
                    filterToolStripMenuItem.Enabled = true;
                    restorationToolStripMenuItem.Enabled = true;
                }
            }
        }

        private HistogramForm HistogramFormInstance;
        private void HistogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HistogramFormInstance = new HistogramForm
            {
                Form2Image = pictureBox1.Image  
            };
            HistogramFormInstance.Show();
        }

        private ProcessForm ProcessFormInstance;
        private void ProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessFormInstance = new ProcessForm 
            { 
                ProcessFormImage = pictureBox1.Image
        };
            _ = pictureBox1.Image;

            ProcessFormInstance.Show();
        }

        public void PCXHeaderEncoder(string filePath)
        {
            using (BinaryReader bReader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                byte[] buffer = new byte[2];

                //Get the data from PCX file 
                int manufac = bReader.ReadByte();
                int version = bReader.ReadByte();
                int encoding = bReader.ReadByte();
                int BPP = bReader.ReadByte();
                int xMin = bReader.ReadInt16();
                int yMin = bReader.ReadInt16();
                int xMax = bReader.ReadInt16();
                int yMax = bReader.ReadInt16();
                int HDPI = bReader.ReadInt16();
                int VDPI = bReader.ReadInt16();

                bReader.ReadBytes(48);
                bReader.ReadByte();

                int numCP = bReader.ReadByte();

                int BPL = bReader.ReadInt16();
                int paletteInfo = bReader.ReadByte();

                int hSS = bReader.ReadByte();
                int vSS = bReader.ReadByte();

                bReader.ReadBytes(54);

                //Display in all labels
                label1.Text = "Manufacturer: Zshoft .pcx (" + manufac + ")";
                label2.Text = "Version: " + version;
                label3.Text = "Encoding: " + encoding;
                label4.Text = "Bits per Pixel: " + BPP.ToString();
                label5.Text = "Image Dimensions: " + xMin + " " + yMin + " " + xMax + " " + yMax;
                label6.Text = "HDPI: " + HDPI;
                label7.Text = "VDPI: " + VDPI;
                label8.Text = "Number of Color Planes: " + numCP;
                label9.Text = "Bytes Per Line: " + BPL;
                label10.Text = "Palette Information: " + paletteInfo;
                label11.Text = "Horizontal Screen Size: " + hSS;
                label12.Text = "Vertical Screen Size: " + vSS;
            }
        }

        private FilterForm FilterFormInstance;


        private void SpatialFilteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FilterFormInstance = new FilterForm
            {
                /* set pictureBox in Guide05 */
                FilterFormImage = pictureBox1.Image
            };
            FilterFormInstance.Show();
        }

        private ImageForm ImageFormInstance;
        private void RestorationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageFormInstance = new ImageForm
            {
                /* set pictureBox in Guide05 */
                ImageFormImage = pictureBox1.Image
            };
            ImageFormInstance.Show();
        }

        //Function to create the color palette and image
        public void ColorPalette(string filePath)
        {
            //Color Pallete
            byte[] PCXBytes = File.ReadAllBytes(filePath);
            List<Color> CP = new List<Color>();
            bool CPEmpty = true;
            if (PCXBytes.Length > 768)
            {
                if (PCXBytes[PCXBytes.Length - 768 - 1] == 0x0C || PCXBytes[PCXBytes.Length - 768 - 1] == 0x6E)
                {
                    CPEmpty = false;
                    for (int i = PCXBytes.Length - 768; i < PCXBytes.Length; i += 3)
                    {
                        CP.Add(Color.FromArgb(PCXBytes[i], PCXBytes[i + 1], PCXBytes[i + 2]));
                    }
                }
            }

            //Color 
            Color[] savepalette = new Color[256 * 256];
            List<byte> palettevalue = new List<byte>();

            //Starting position is 128
            int position = 128;
            byte rCount = 0;
            byte rValue = 0;

            //Checks the type of byte (either 1-byte or 2-byte)
            do
            {
                byte Byte = PCXBytes[position++];
                if ((Byte & 0xC0) == 0xC0 && position < PCXBytes.Length)
                {
                    rCount = (byte)(Byte & 0x3F);
                    rValue = PCXBytes[position++];
                }
                else
                {
                    rCount = 1;
                    rValue = Byte;
                }
                for (int j = 0; j < rCount; j++)
                {
                    palettevalue.Add(rValue);
                }
            } while (position < PCXBytes.Length);

            //Forming the image
            pcxBMP = new Bitmap(256, 256);
            if (!CPEmpty)
            {
                for (int i = 0; i < 256 * 256; i++)
                {
                    savepalette[i] = CP[palettevalue[i]];
                    int y = i / 256;
                    int x = i - (256 * y);
                    pcxBMP.SetPixel(x, y, savepalette[i]);
                }
            }

            pictureBox1.Image = new Bitmap(pcxBMP);

            //Forming the color palette map
            Bitmap ColorMap = new Bitmap(128, 128);
            int z=0;
            for (int i = 0; i < 80; i += 5)
            {
                for (int j = 0; j < 80; j += 5)
                {
                    using (Graphics gfx = Graphics.FromImage(ColorMap))
                    using (SolidBrush brush = new SolidBrush(CP[z]))
                    {
                        gfx.FillRectangle(brush, i, j, 5, 5);
                    }
                    z++;
                }
            }

            _ = new Bitmap(ColorMap);
            pictureBox2.Image = new Bitmap(ColorMap);
        }
        //Preparation of RGB components
        private void ProcessRGB()
        {
            rVAL = new int[256, 256];
            gVAL = new int[256, 256];
            bVAL = new int[256, 256];

            //Collects intensity level per pixel of each channel
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    rVAL[i, j] = pcxBMP.GetPixel(i, j).R;
                    gVAL[i, j] = pcxBMP.GetPixel(i, j).G;
                    bVAL[i, j] = pcxBMP.GetPixel(i, j).B;
                }
            }
        }
    }
}


using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace CMSC_169_Project_1
{
    public partial class ProcessForm : Form
    {
        //Global Variables
        Bitmap pcxBMP;
        Image ProcessFormOriginal;

        public ProcessForm()
        {
            InitializeComponent();
            trackBar1.Enabled = false;
            trackBar2.Enabled = false;

        }

        //Function to take the image from the initial form
        public Image ProcessFormImage
        {
            get { return pictureBox1.Image; }
            set
            {
                pictureBox1.Image = value;
                pcxBMP = new Bitmap(value);
                ProcessFormOriginal = pictureBox1.Image;
            }
        }



        private void Button1_Click(object sender, EventArgs e)
        {
            //Disable Trackers (Not Needed)
            trackBar1.Enabled = false;
            trackBar2.Enabled = false;

            Bitmap gImage = new Bitmap(pictureBox1.Image);

            for (int x = 0; x < gImage.Width; x++)
            {
                for (int y = 0; y < gImage.Height; y++)
                {
                    Color oldColor = gImage.GetPixel(x, y);


                    int changeGray = (oldColor.R + oldColor.G + oldColor.B) / 3;
                    Color newColor = Color.FromArgb(oldColor.A, changeGray, changeGray, changeGray);

                    gImage.SetPixel(x, y, newColor);
                }
            }

            pictureBox1.Image = gImage;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            //Disable Trackers (Not Needed)
            trackBar1.Enabled = false;
            trackBar2.Enabled = false;

            Bitmap nImage = new Bitmap(pictureBox1.Image);

            for (int y = 0; y < nImage.Height; y++)
            {
                for (int x = 0; x < nImage.Width; x++)
                {
                    Color pixel = nImage.GetPixel(x, y);

                    int r = 255 - pixel.R;
                    int g = 255 - pixel.G;
                    int b = 255 - pixel.B;

                    nImage.SetPixel(x, y,
                        Color.FromArgb(pixel.A, r, g, b));
                }
            }

            pictureBox1.Image = nImage;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            TrackBar1_Scroll(sender, e);
            trackBar1.Enabled = true;
            trackBar2.Enabled = false;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            TrackBar2_Scroll(sender, e);
            trackBar1.Enabled = false;
            trackBar2.Enabled = true;
        }

        private void TrackBar2_Scroll(object sender, EventArgs e)
        {
            Bitmap gammaImage = new Bitmap(pcxBMP);

            float gamma = trackBar2.Value * 0.1f;

            ImageAttributes imageAttribute = new ImageAttributes();
            imageAttribute.SetGamma(gamma, ColorAdjustType.Bitmap);

            Graphics g = Graphics.FromImage(gammaImage);

            g.DrawImage(pcxBMP, new Rectangle(0, 0, gammaImage.Width, gammaImage.Height),
                0, 0, pcxBMP.Width, pcxBMP.Height, GraphicsUnit.Pixel, imageAttribute);

            pictureBox1.Image = gammaImage;
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            Bitmap bwImage = new Bitmap(pcxBMP);

            ColorMatrix grayMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] { 0.299f, 0.299f, 0.299f, 0, 0},
                    new float[] { 0.587f, 0.587f, 0.587f, 0, 0},
                    new float[] { 0.114f, 0.114f, 0.114f, 0, 0},
                    new float[] { 0, 0, 0, 1, 0},
                    new float[] { 0, 0, 0, 0, 1}
                }
            );

            ImageAttributes imageAttribute = new ImageAttributes();
            imageAttribute.SetColorMatrix(grayMatrix);
            imageAttribute.SetThreshold(trackBar1.Value / 255.0f); // adjusting the threshold

            Graphics g = Graphics.FromImage(bwImage);

            g.DrawImage(pcxBMP, new Rectangle(0, 0, pcxBMP.Width, pcxBMP.Height),
                0, 0, pcxBMP.Width, pcxBMP.Height, GraphicsUnit.Pixel, imageAttribute);

            pictureBox1.Image = bwImage;
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = ProcessFormOriginal;
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace CMSC_169_Project_1
{
    public partial class ImageForm : Form
    {
        # region Global Variables
        Bitmap pcxBMP;
        #endregion

        #region Initialization
        public ImageForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Functions

        //Function to take the image from the initial form
        public Image ImageFormImage
        {
            get { return pictureBox1.Image; }
            set
            {
                pictureBox1.Image = ToGrayscale((Bitmap)value); //Change to grayscale
                pcxBMP = ToGrayscale((Bitmap)value);
            }
        }

        //PSNR Calculation
        private double PSNR(int max, double mse)
        {
            return 20 * Math.Log10(max) - 10 * Math.Log10(mse);
        }

        //Convert to Grayscale Initial Image
        private static Bitmap ToGrayscale(Bitmap image)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);

            using (Graphics g = Graphics.FromImage(result))
            {
                ColorMatrix gMatrix = new ColorMatrix(
                    new float[][]
                    {
                        new float[] { 0.299f, 0.299f, 0.299f, 0, 0},
                        new float[] { 0.587f, 0.587f, 0.587f, 0, 0},
                        new float[] { 0.114f, 0.114f, 0.114f, 0, 0},
                        new float[] { 0, 0, 0, 1, 0},
                        new float[] { 0, 0, 0, 0, 1}
                    }
                );

                using (ImageAttributes imageAttribute = new ImageAttributes())
                {
                    imageAttribute.SetColorMatrix(gMatrix);
                    g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                        0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttribute);
                }
            }
            return result;
        }

        private void SaltAndPepper(int a, int b)
        {
            Bitmap saltPepper = (Bitmap)pictureBox1.Image.Clone();
            Random r = new Random(); //generate random

            double totalSum = 0;

            for (int x = 0; x < saltPepper.Width; x++)
            {
                for (int y = 0; y < saltPepper.Height; y++)
                {
                    Color c = saltPepper.GetPixel(x, y);

                    double difference;

                    int max = 100;
                    int noise = r.Next(max + 1); //generate random num
                    if (noise == 0)
                    {
                        saltPepper.SetPixel(x, y, Color.FromArgb(c.A, a, a, a));
                        difference = c.R - 0;
                    }
                    else if (noise == max)
                    {
                        saltPepper.SetPixel(x, y, Color.FromArgb(c.A, b, b, b));
                        difference = c.R - 255;
                    }
                    else
                    {
                        saltPepper.SetPixel(x, y, Color.FromArgb(c.A, c.R, c.G, c.B));
                        difference = c.R - c.R;
                    }
                    totalSum += Math.Pow(difference, 2);
                }
            }
            double mse = totalSum / (saltPepper.Width * saltPepper.Height);
            double ratio = PSNR(255, mse);

            pictureBox1.Image = new Bitmap(saltPepper);
            imageBox1.Text = "With Noise: PSNR = " + ratio.ToString();
        }

        //Mean Filter
        private void MeanFilter()
        {
            Bitmap averagingFilter = (Bitmap)pictureBox1.Image.Clone();

            double sumColor = 0;
            double sumMSE = 0;
            double difference = 0;
            for (int x = 0; x < averagingFilter.Width - 5; x++)
            {
                for (int y = 0; y < averagingFilter.Height - 5; y++)
                {
                    for (int i = x; i < x + 5; i++)
                    {
                        for (int j = y; j < y + 5; j++)
                        {
                            Color c = averagingFilter.GetPixel(i, j);
                            sumColor += c.R;
                            difference += c.R;
                        }
                    }
                    int nColor = (int)Math.Round(sumColor / 25, 10);
                    difference /= nColor;
                    sumMSE += Math.Pow(difference, 2);

                    averagingFilter.SetPixel(x + 1, y + 1, Color.FromArgb(nColor, nColor, nColor));
                    sumColor = 0;
                    difference = 0;
                }
            }
            double mse = sumMSE / (averagingFilter.Width * averagingFilter.Height);
            double ratio = PSNR(255, mse);

            pictureBox2.Image = averagingFilter;
            imageBox2.Text = "Average Filter 5: PSNR = " + ratio.ToString();
        }

        //Median Cross Function
        private void MedianCross()
        {
            Bitmap MedianCross = (Bitmap)pictureBox1.Image.Clone();
            List<int> colorNeighbor = new List<int>();

            double sumMSE = 0;
            double difference = 0;
            for (int x = 0; x < MedianCross.Width - 5; x++)
            {
                for (int y = 0; y < MedianCross.Height - 5; y++)
                {
                    for (int i = x; i < x + 5; i++)
                    {
                        if (i == x + 2)
                        {
                            for (int j = y; j < y + 5; j++)
                            {
                                Color c = MedianCross.GetPixel(i, j);
                                colorNeighbor.Add(c.R);
                                difference += c.R;
                            }
                        }
                        else
                        {
                            Color c = MedianCross.GetPixel(i, y + 3);
                            colorNeighbor.Add(c.R);
                            difference += c.R;
                        }
                    }
                    colorNeighbor.Sort();
                    int nColor = colorNeighbor[(colorNeighbor.Count - 1) / 2];
                    difference /= nColor;
                    sumMSE += Math.Pow(difference, 2);

                    MedianCross.SetPixel(x + 1, y + 1, Color.FromArgb(nColor, nColor, nColor));

                    colorNeighbor.Clear();
                    difference = 0;
                }
            }
            double mse = sumMSE / (MedianCross.Width * MedianCross.Height);
            double ratio = PSNR(255, mse);

            pictureBox3.Image = MedianCross;
            imageBox3.Text = "P-Median Cross 5: PSNR = " + ratio.ToString();
        }

        //Median Square Function
        private void MedianSquare()
        {
            Bitmap MedianSquare = (Bitmap)pictureBox1.Image.Clone();
            List<int> colorNeighbor = new List<int>();

            double sumMSE = 0;
            double difference = 0;
            for (int x = 0; x < MedianSquare.Width - 5; x++)
            {
                for (int y = 0; y < MedianSquare.Height - 5; y++)
                {
                    for (int i = x; i < x + 5; i++)
                    {
                        for (int j = y; j < y + 5; j++)
                        {
                            Color c = MedianSquare.GetPixel(i, j);
                            colorNeighbor.Add(c.R);
                            difference += c.R;
                        }
                    }
                    colorNeighbor.Sort();
                    int nColor = colorNeighbor[(colorNeighbor.Count - 1) / 2];
                    difference /= nColor;
                    sumMSE += Math.Pow(difference, 2);

                    MedianSquare.SetPixel(x + 1, y + 1, Color.FromArgb(nColor, nColor, nColor));

                    colorNeighbor.Clear();
                    difference = 0;
                }
            }
            double mse = sumMSE / (MedianSquare.Width * MedianSquare.Height);
            double ratio = PSNR(255, mse);

            pictureBox4.Image = MedianSquare;
            imageBox4.Text = "P-Median Square 5: PSNR = " + ratio.ToString();
        }

 
        #endregion

        #region On-click Events
        //Salt Noise
        private void Button1_Click(object sender, EventArgs e)
        {
            SaltAndPepper(255, 255);
        }

        //Add Pepper Noise
        private void button2_Click(object sender, EventArgs e)
        {
            SaltAndPepper(0, 0);
        }

        //Salt and Pepper Noise
        private void button3_Click(object sender, EventArgs e)
        {
            SaltAndPepper(0, 255);
        }

        //Mean Filter
        private void button4_Click(object sender, EventArgs e)
        {
            MeanFilter();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MedianCross();
            MedianSquare();
        }

        //Set to Original Image
        private void Button8_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(pcxBMP);
            imageBox1.Text = "Original Grayscale";
        }

        //Compress Image
        private void button6_Click(object sender, EventArgs e)
        {

        }
    }
    #endregion
}



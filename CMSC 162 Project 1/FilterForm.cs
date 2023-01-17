using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace CMSC_169_Project_1
{
    public partial class FilterForm : Form
    {
        Bitmap pcxBMP;
        public FilterForm()
        {
            InitializeComponent();
            groupBox5.Visible = false;
        }

        //Function to take the image from the initial form
        public Image FilterFormImage
        {
            get { return pictureBox1.Image; }
            set
            {
                pictureBox1.Image = ToGrayscale((Bitmap)value); //Change to grayscale
                pcxBMP = ToGrayscale((Bitmap)value);
            }
        }

        //Set to original image
        private void Button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(pcxBMP);
            groupBox1.Text = "orig Grayscale";
        }

        //Salt and Pepper Noise
        private void Button2_Click(object sender, EventArgs e)
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
                        saltPepper.SetPixel(x, y, Color.FromArgb(c.A, 0, 0, 0));
                        difference = c.R - 0;
                    }
                    else if (noise == max)
                    {
                        saltPepper.SetPixel(x, y, Color.FromArgb(c.A, 255, 255, 255));
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
            groupBox1.Text = "With Noise: PSNR = " + ratio.ToString();
        }

        //LowPass Filters
        private void Button3_Click(object sender, EventArgs e)
        {
            MeanFilter();
            MedianCross();
            MedianSquare();

            groupBox5.Visible = false;
        }

        //Highpass Filters
        private void Button4_Click(object sender, EventArgs e)
        {
            // get blurred image from average filter
            MeanFilter();
            Bitmap blur = (Bitmap)pictureBox2.Image.Clone();

            LaplacianFiltering();
            UnsharpMasking(blur);

            groupBox5.Visible = true;
        }

        //Gradient using Sobel Feldmann Operator
        private void Button5_Click(object sender, EventArgs e)
        {
            SobelX();
            SobelY();
            SfMagnitiude();

            groupBox5.Visible = false;
        }

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

        private double PSNR(int max, double mse)
        {
            return 20 * Math.Log10(max) - 10 * Math.Log10(mse);
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
            groupBox2.Text = "Average Filter 5: PSNR = " + ratio.ToString();
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
            groupBox3.Text = "P-Median Cross 5: PSNR = " + ratio.ToString();
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
            groupBox4.Text = "P-Median Square 5: PSNR = " + ratio.ToString();
        }

        //LaplacianFiltering Function
        private void LaplacianFiltering()
        {
            Bitmap laplacianMasking = (Bitmap)pictureBox1.Image.Clone();
            List<int> selectedColors = new List<int>();

            double sumMSE = 0;
            double difference = 0;
            for (int x = 0; x < laplacianMasking.Width - 3; x++)
            {
                for (int y = 0; y < laplacianMasking.Height - 3; y++)
                {
                    for (int i = x; i < x + 3; i++)
                    {
                        if (i == x || i == x + 2)
                        {
                            Color c = laplacianMasking.GetPixel(i, y + 1);
                            selectedColors.Add(c.R);
                            difference += c.R;
                        }
                        else
                        {
                            for (int j = y; j < y + 3; j++)
                            {
                                Color c = laplacianMasking.GetPixel(i, j);
                                if (j == y + 1)
                                {
                                    selectedColors.Add(-4 * c.R);
                                    difference += -4 * c.R;
                                }
                                else
                                {
                                    selectedColors.Add(c.R);
                                    difference += c.R;
                                }
                            }
                        }
                    }
                    int nColor = selectedColors.Sum();
                    nColor = Math.Abs(nColor) > 255 ? 255 : Math.Abs(nColor);
                    sumMSE += Math.Pow(difference, 2);

                    laplacianMasking.SetPixel(x, y, Color.FromArgb(nColor, nColor, nColor));

                    // Reset
                    selectedColors.Clear();
                    difference = 0;
                }
            }
            double mse = sumMSE / (laplacianMasking.Width * laplacianMasking.Height);
            double ratio = PSNR(255, mse);

            pictureBox3.Image = laplacianMasking;
            groupBox3.Text = "Laplacian 3: PSNR = " + ratio.ToString();
        }

        private void UnsharpMasking(Bitmap blur)
        {
            Bitmap orig = (Bitmap)pictureBox1.Image.Clone();
            Bitmap mask = (Bitmap)pictureBox1.Image.Clone();

            double sumMSE = 0;
            for (int x = 0; x < blur.Width; x++)
            {
                for (int y = 0; y < blur.Height; y++)
                {
                    Color cBlur = blur.GetPixel(x, y);
                    Color cOrig = orig.GetPixel(x, y);

                    int getMask = (cOrig.R - cBlur.R) < 0 ? 0 : cOrig.R - cBlur.R;

                    int addMask = cOrig.R + 1 * getMask > 255 ? 255 : cOrig.R + 1 * getMask;

                    mask.SetPixel(x, y, Color.FromArgb(addMask, addMask, addMask));

                    double difference = cOrig.R - addMask;
                    sumMSE += Math.Pow(difference, 2);
                }
            }
            double mse = sumMSE / (mask.Width * mask.Height);
            double ratio = PSNR(255, mse);

            pictureBox2.Image = new Bitmap(mask);
            groupBox2.Text = "Unsharp Masking: PSNR = " + ratio.ToString();
        }

        private void HighboostFiltering(Bitmap blur)
        {
            int k = trackBar1.Value;

            Bitmap orig = (Bitmap)pictureBox1.Image.Clone();
            Bitmap filter = (Bitmap)pictureBox1.Image.Clone();
            double sumMSE = 0;

            for (int x = 0; x < blur.Width; x++)
            {
                for (int y = 0; y < blur.Height; y++)
                {
                    Color cBlur = blur.GetPixel(x, y);
                    Color cOrig = orig.GetPixel(x, y);

                    int getMask = (cOrig.R - cBlur.R) < 0 ? 0 : cOrig.R - cBlur.R;
                    int addMask = cOrig.R + k * getMask > 255 ? 255 : cOrig.R + k * getMask;

                    filter.SetPixel(x, y, Color.FromArgb(addMask, addMask, addMask));

                    double difference = cOrig.R - addMask;
                    sumMSE += Math.Pow(difference, 2);
                }
            }
            double mse = sumMSE / (filter.Width * filter.Height);
            double ratio = PSNR(255, mse);

            pictureBox4.Image = new Bitmap(filter);
            groupBox4.Text = "Highboost Filtering "
                + k.ToString()
                + ": PSNR = "
                + ratio.ToString();
        }

        //Function for Sobel X 
        private void SobelX()
        {
            Bitmap sobel = (Bitmap)pictureBox1.Image.Clone();
            List<int> selectedColors = new List<int>();

            double sumMSE = 0;
            double difference = 0;
            for (int x = 0; x < sobel.Width - 3; x++)
            {
                for (int y = 0; y < sobel.Height - 3; y++)
                {
                    for (int i = x; i < x + 3; i++)
                    {
                        if (i == x || i == x + 2)
                        {
                            for (int j = y; j < y + 3; j++)
                            {
                                Color c = sobel.GetPixel(i, j);
                                if (j == y)
                                {
                                    selectedColors.Add(-1 * c.R);
                                    difference += -1 * c.R;
                                }
                                else if (j == y + 2)
                                {
                                    selectedColors.Add(c.R);
                                    difference += c.R;
                                }
                            }
                        }
                        else
                        {
                            for (int j = y; j < y + 3; j++)
                            {
                                Color c = sobel.GetPixel(i, j);
                                if (j == y)
                                {
                                    selectedColors.Add(-2 * c.R);
                                    difference += -2 * c.R;
                                }
                                else if (j == y + 2)
                                {
                                    selectedColors.Add(2 * c.R);
                                    difference += 2 * c.R;
                                }
                            }
                        }
                    }
                    int nColor = selectedColors.Sum();
                    nColor = Math.Abs(nColor) > 255 ? 255 : Math.Abs(nColor);
                    sumMSE += Math.Pow(difference, 2);

                    sobel.SetPixel(x, y, Color.FromArgb(nColor, nColor, nColor));

                    // reset
                    selectedColors.Clear();
                    difference = 0;
                }
            }
            double mse = sumMSE / (sobel.Width * sobel.Height);
            double ratio = PSNR(255, mse);

            pictureBox2.Image = sobel;
            groupBox2.Text = "Sobel X-Gradient: PSNR = " + ratio.ToString();
        }

        //Function for Sobel Y 
        private void SobelY()
        {
            Bitmap sobel = (Bitmap)pictureBox1.Image.Clone();
            List<int> selectedColors = new List<int>();

            double sumMSE = 0;
            double difference = 0;
            for (int x = 0; x < sobel.Width - 3; x++)
            {
                for (int y = 0; y < sobel.Height - 3; y++)
                {
                    for (int i = x; i < x + 3; i++)
                    {
                        if (i == x)
                        {
                            for (int j = y; j < y + 3; j++)
                            {
                                Color c = sobel.GetPixel(i, j);
                                if (j == y || j == y + 2)
                                {
                                    selectedColors.Add(-1 * c.R);
                                    difference += -1 * c.R;
                                }
                                else
                                {
                                    selectedColors.Add(-2 * c.R);
                                    difference += -2 * c.R;
                                }
                            }
                        }
                        else if (i == x + 2)
                        {
                            for (int j = y; j < y + 3; j++)
                            {
                                Color c = sobel.GetPixel(i, j);
                                if (j == y || j == y + 2)
                                {
                                    selectedColors.Add(1 * c.R);
                                    difference += 1 * c.R;
                                }
                                else
                                {
                                    selectedColors.Add(2 * c.R);
                                    difference += 2 * c.R;
                                }
                            }
                        }
                    }
                    int nColor = selectedColors.Sum();
                    nColor = Math.Abs(nColor) > 255 ? 255 : Math.Abs(nColor);
                    sumMSE += Math.Pow(difference, 2);

                    sobel.SetPixel(x, y, Color.FromArgb(nColor, nColor, nColor));

                    selectedColors.Clear();
                    difference = 0;
                }
            }
            double mse = sumMSE / (sobel.Width * sobel.Height);
            double ratio = PSNR(255, mse);

            pictureBox3.Image = sobel;
            groupBox3.Text = "Sobel Y-Gradient: PSNR = " + ratio.ToString();
        }

        //Function for Sobel Magnitude 
        private void SfMagnitiude()
        {
            Bitmap SfMagnitiude = (Bitmap)pictureBox1.Image.Clone();

            Bitmap SobelX = (Bitmap)pictureBox2.Image.Clone();
            Bitmap SobelY = (Bitmap)pictureBox3.Image.Clone();

            double sumMSE = 0;
            for (int x = 0; x < SfMagnitiude.Width; x++)
            {
                for (int y = 0; y < SfMagnitiude.Height; y++)
                {
                    Color cX = SobelX.GetPixel(x, y);
                    Color cY = SobelY.GetPixel(x, y);

                    double gradient = Math.Sqrt(Math.Pow(cX.R, 2) + Math.Pow(cY.R, 2));

                    int nColor = (int)gradient;
                    nColor = Math.Abs(nColor) > 255 ? 255 : Math.Abs(nColor);
                    sumMSE += Math.Pow(gradient, 2);

                    SfMagnitiude.SetPixel(x, y, Color.FromArgb(nColor, nColor, nColor));
                }
            }
            double mse = sumMSE / (SfMagnitiude.Width * SfMagnitiude.Height);
            double ratio = PSNR(255, mse);

            pictureBox4.Image = SfMagnitiude;
            groupBox4.Text = "Sobel Magnitude: PSNR = " + ratio.ToString();
        }

        private void SliderFormTrackBar_ValueChanged(object sender, EventArgs e)
        {
            // get blurred image from average filter
            MeanFilter();
            Bitmap blur = (Bitmap)pictureBox2.Image.Clone();

            HighboostFiltering(blur);
        }
    }
}

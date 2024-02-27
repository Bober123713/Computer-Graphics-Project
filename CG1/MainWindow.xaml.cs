using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using Microsoft.Win32;




namespace CG1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private WriteableBitmap ConvertToWriteableBitmap(BitmapSource bitmapSource)
        {
            WriteableBitmap WriteableBitmap = new WriteableBitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, bitmapSource.DpiX, bitmapSource.DpiY, bitmapSource.Format, null);
            byte[] pixels = new byte[bitmapSource.PixelWidth * bitmapSource.PixelHeight * (bitmapSource.Format.BitsPerPixel / 8)];
            bitmapSource.CopyPixels(pixels, bitmapSource.PixelWidth * (bitmapSource.Format.BitsPerPixel / 8), 0);
            WriteableBitmap.WritePixels(new Int32Rect(0, 0, bitmapSource.PixelWidth, bitmapSource.PixelHeight), pixels, bitmapSource.PixelWidth * (bitmapSource.Format.BitsPerPixel / 8), 0);
            return WriteableBitmap;
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                RightImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                LeftImage.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        private void SaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (RightImage.Source is BitmapSource source)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
                    FileName = "Image",
                    DefaultExt = ".png" 
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    BitmapEncoder? encoder = null;
                    switch (System.IO.Path.GetExtension(saveFileDialog.FileName).ToLowerInvariant())
                    {
                        case ".jpg":
                        case ".jpeg":
                            encoder = new JpegBitmapEncoder();
                            break;
                        case ".bmp":
                            encoder = new BmpBitmapEncoder();
                            break;
                        case ".png":
                        default:
                            encoder = new PngBitmapEncoder();
                            break;
                    }

                    encoder.Frames.Add(BitmapFrame.Create(source));

                    using (var stream = File.Create(saveFileDialog.FileName))
                    {
                        encoder.Save(stream);
                    }
                }
            }
            else
            {
                MessageBox.Show("There is no image to save.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void ApplyBlur(WriteableBitmap WriteableBitmap)
        {
            int width = WriteableBitmap.PixelWidth;
            int height = WriteableBitmap.PixelHeight;
            int stride = width * (WriteableBitmap.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[height * stride];
            WriteableBitmap.CopyPixels(pixels, stride, 0);

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int index = y * stride + x * 4;

                    for (int channel = 0; channel < 3; channel++) // RGB channels
                    {
                        int sum = 0;
                        for (int ny = -1; ny <= 1; ny++)
                        {
                            for (int nx = -1; nx <= 1; nx++)
                            {
                                int neighborIndex = (y + ny) * stride + (x + nx) * 4;
                                sum += pixels[neighborIndex + channel];
                            }
                        }
                        pixels[index + channel] = (byte)(sum / 9); // Average of the 3x3 kernel
                    }
                }
            }

            WriteableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        private void ApplyInversion(WriteableBitmap WriteableBitmap)
        {
            int width = WriteableBitmap.PixelWidth;
            int height = WriteableBitmap.PixelHeight;
            int stride = width * (WriteableBitmap.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[height * stride];
            WriteableBitmap.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += 4) // Assuming 32bpp
            {
                pixels[i] = (byte)(255 - pixels[i]);     // B
                pixels[i + 1] = (byte)(255 - pixels[i + 1]); // G
                pixels[i + 2] = (byte)(255 - pixels[i + 2]); // R
                                                             
            }

            WriteableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        private void ApplyBrightnessCorrection(WriteableBitmap writeableBitmap, int brightness)
        {
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int stride = width * (writeableBitmap.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[height * stride];
            writeableBitmap.CopyPixels(pixels, stride, 0);

            for (int i = 0; i < pixels.Length; i += 4) // Assuming 32bpp
            {
                for (int j = 0; j < 3; j++) // B, G, R
                {
                    int pixel = pixels[i + j] + brightness;
                    if (pixel < 0) pixel = 0;
                    if (pixel > 255) pixel = 255;
                    pixels[i + j] = (byte)pixel;
                }
            }

            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }


        private void ApplyContrastEnhancement(WriteableBitmap writeableBitmap, double contrast)
        {
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int stride = width * (writeableBitmap.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[height * stride];
            writeableBitmap.CopyPixels(pixels, stride, 0);

            contrast = (100.0 + contrast) / 100.0;
            contrast *= contrast;

            for (int i = 0; i < pixels.Length; i += 4) // Assuming 32bpp
            {
                for (int j = 0; j < 3; j++) // B, G, R
                {
                    double pixel = pixels[i + j] / 255.0;
                    pixel -= 0.5;
                    pixel *= contrast;
                    pixel += 0.5;
                    pixel *= 255;
                    if (pixel < 0) pixel = 0;
                    if (pixel > 255) pixel = 255;
                    pixels[i + j] = (byte)pixel;
                }
            }

            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }


        private void ApplyGammaCorrection(WriteableBitmap writeableBitmap, double gamma)
        {
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            int stride = width * (writeableBitmap.Format.BitsPerPixel / 8);
            byte[] pixels = new byte[height * stride];
            writeableBitmap.CopyPixels(pixels, stride, 0);

            double[] gammaCorrectionTable = new double[256];
            for (int i = 0; i < 256; i++)
            {
                gammaCorrectionTable[i] = 255 * Math.Pow(i / 255.0, 1.0 / gamma);
            }

            for (int i = 0; i < pixels.Length; i += 4) // Assuming 32bpp
            {
                pixels[i] = (byte)gammaCorrectionTable[pixels[i]];     // B
                pixels[i + 1] = (byte)gammaCorrectionTable[pixels[i + 1]]; // G
                pixels[i + 2] = (byte)gammaCorrectionTable[pixels[i + 2]]; // R
            }

            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }



        private void Blur_Click(object sender, RoutedEventArgs e)
        {
            if (RightImage.Source is BitmapSource bitmapSource)
            {
                WriteableBitmap WriteableBitmap = ConvertToWriteableBitmap(bitmapSource);
                ApplyBlur(WriteableBitmap);
                RightImage.Source = WriteableBitmap;
            }
        }


        private void Inverse_Click(object sender, RoutedEventArgs e)
        {
            if (RightImage.Source is BitmapSource bitmapSource)
            {
                WriteableBitmap WriteableBitmap = ConvertToWriteableBitmap(bitmapSource);
                ApplyInversion(WriteableBitmap);
                RightImage.Source = WriteableBitmap;
            }
        }

        private void BrightnessCorrection_Click(object sender, RoutedEventArgs e)
        {
            if (RightImage.Source is BitmapSource bitmapSource)
            {
                WriteableBitmap writeableBitmap = ConvertToWriteableBitmap(bitmapSource);
                ApplyBrightnessCorrection(writeableBitmap, 20); // Example brightness value
                RightImage.Source = writeableBitmap;
            }
        }

        private void ContrastEnhancement_Click(object sender, RoutedEventArgs e)
        {
            if (RightImage.Source is BitmapSource bitmapSource)
            {
                WriteableBitmap writeableBitmap = ConvertToWriteableBitmap(bitmapSource);
                ApplyContrastEnhancement(writeableBitmap, 20); // Example contrast value
                RightImage.Source = writeableBitmap;
            }
        }


        private void GammaCorrection_Click(object sender, RoutedEventArgs e)
        {
            if (RightImage.Source is BitmapSource bitmapSource)
            {
                WriteableBitmap writeableBitmap = ConvertToWriteableBitmap(bitmapSource);
                ApplyGammaCorrection(writeableBitmap, 2.2); // Example gamma value
                RightImage.Source = writeableBitmap;
            }
        }

    }
}
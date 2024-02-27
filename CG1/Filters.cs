using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

namespace CG1
{
    public partial class MainWindow
    {
        private void ApplyInversion(WriteableBitmap WriteableBitmap)
        {
            int width, height, stride;
            byte[] pixels;
            GetPixels(WriteableBitmap, out width, out height, out stride, out pixels);

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = (byte)(255 - pixels[i]);
            }

            WriteableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        private void ApplyBrightnessCorrection(WriteableBitmap source)
        {
            int width, height, stride;
            byte[] pixels;
            GetPixels(source, out width, out height, out stride, out pixels);

            for (int i = 0; i < pixels.Length; i++)
            {
                int pixel = pixels[i] + BRIGHTNESS;
                if (pixel < 0) pixel = 0;
                if (pixel > 255) pixel = 255;
                pixels[i] = (byte)pixel;
            }

            source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        private void ApplyContrastEnhancement(WriteableBitmap source)
        {
            int width, height, stride;
            byte[] pixels;
            GetPixels(source, out width, out height, out stride, out pixels);

            var contrast = (100.0 + CONTRAST) / 100.0;
            contrast *= contrast;

            for (int i = 0; i < pixels.Length; i++) 
            {
                double pixel = pixels[i] / 255.0;
                pixel -= 0.5;
                pixel *= contrast;
                pixel += 0.5;
                pixel *= 255;
                if (pixel < 0) pixel = 0;
                if (pixel > 255) pixel = 255;
                pixels[i] = (byte)pixel;
            }

            source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        private void ApplyGammaCorrection(WriteableBitmap source)
        {
            int width, height, stride;
            byte[] pixels;
            GetPixels(source, out width, out height, out stride, out pixels);

            double[] gammaCorrectionTable = new double[256];
            for (int i = 0; i < 256; i++)
            {
                gammaCorrectionTable[i] = 255 * Math.Pow(i / 255.0, 1.0 / GAMMA);
            }

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = (byte)gammaCorrectionTable[pixels[i]];
            }

            source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
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

                    for (int channel = 0; channel < 3; channel++)
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
    }
}

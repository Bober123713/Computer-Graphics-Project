using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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

        private void ApplyBlur(WriteableBitmap source)
        {
            int width, height, stride;
            byte[] pixels;
            GetPixels(source, out width, out height, out stride, out pixels);

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

            source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        private void ApplyGaussianBlur(WriteableBitmap source)
        {
            int width, height, stride;
            byte[] pixels;
            GetPixels(source, out width, out height, out stride, out pixels);

            // Clone the original pixels to use as a reference
            byte[] originalPixels = new byte[pixels.Length];
            Array.Copy(pixels, originalPixels, pixels.Length);

            // Simplified 3x3 Gaussian kernel with sigma = 1 (values approximated and normalized)
            double[,] kernel = {
                { 1/16d, 2/16d, 1/16d },
                { 2/16d, 4/16d, 2/16d },
                { 1/16d, 2/16d, 1/16d }
            };

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    double[] sum = new double[3]; // Sum for each channel (RGB)

                    // Apply the kernel to each neighbor
                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            int pixelIndex = ((y + ky) * stride) + ((x + kx) * 4);

                            for (int channel = 0; channel < 3; channel++) // Apply for RGB channels, skip alpha channel
                            {
                                sum[channel] += originalPixels[pixelIndex + channel] * kernel[ky + 1, kx + 1];
                            }
                        }
                    }

                    int index = y * stride + x * 4;
                    for (int channel = 0; channel < 3; channel++)
                    {
                        pixels[index + channel] = (byte)Math.Max(0, Math.Min(255, sum[channel]));
                    }
                }
            }

            // Write the modified pixels back to the source WriteableBitmap
            source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }


        private void ApplySharpening(WriteableBitmap source)
        {
            int width, height, stride;
            byte[] pixels;
            GetPixels(source, out width, out height, out stride, out pixels);

            // Create a copy of the pixels to hold the original values
            byte[] originalPixels = new byte[pixels.Length];
            Array.Copy(pixels, originalPixels, pixels.Length);

            // Define a simple sharpening kernel
            int[,] kernel = {
                { 0, -1,  0},
                {-1,  5, -1},
                { 0, -1,  0}
            };

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int pixelIndex = y * stride + x * 4;
                    for (int channel = 0; channel < 3; channel++) // Apply for RGB channels, skip alpha channel
                    {
                        int sum = 0;
                        for (int ky = -1; ky <= 1; ky++)
                        {
                            for (int kx = -1; kx <= 1; kx++)
                            {
                                int sampleIndex = (y + ky) * stride + (x + kx) * 4;
                                sum += originalPixels[sampleIndex + channel] * kernel[ky + 1, kx + 1];
                            }
                        }

                        // Clamp the values to [0, 255] and update the current pixel
                        sum = Math.Max(0, Math.Min(255, sum));
                        pixels[pixelIndex + channel] = (byte)sum;
                    }
                }
            }

            source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        private void ApplySobelEdgeDetection(WriteableBitmap source)
        {
            int width, height, stride;
            byte[] pixels;
            GetPixels(source, out width, out height, out stride, out pixels);

            // Create a clone of the pixels array to hold the edge data
            byte[] edgePixels = new byte[pixels.Length];

            // Sobel kernels for X and Y direction
            int[,] gx = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] gy = new int[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    float edgeX = 0;
                    float edgeY = 0;

                    // Apply kernels to the pixel and its neighborhood
                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            int pixelIndex = ((y + ky) * stride) + ((x + kx) * 4);
                            byte pixelIntensity = (byte)((pixels[pixelIndex] + pixels[pixelIndex + 1] + pixels[pixelIndex + 2]) / 3);

                            edgeX += gx[ky + 1, kx + 1] * pixelIntensity;
                            edgeY += gy[ky + 1, kx + 1] * pixelIntensity;
                        }
                    }

                    // Calculate the gradient magnitude
                    byte edgeMagnitude = (byte)Math.Min(255, Math.Sqrt(edgeX * edgeX + edgeY * edgeY));

                    int index = (y * stride) + (x * 4);
                    edgePixels[index] = edgeMagnitude;
                    edgePixels[index + 1] = edgeMagnitude;
                    edgePixels[index + 2] = edgeMagnitude;
                    edgePixels[index + 3] = 255; // Alpha channel
                }
            }

            // Update the writeableBitmap with the edge data
            source.WritePixels(new Int32Rect(0, 0, width, height), edgePixels, stride, 0);
        }

        private void ApplyEmboss(WriteableBitmap source)
        {
            int width, height, stride;
            byte[] pixels;
            GetPixels(source, out width, out height, out stride, out pixels);

            // Clone the original pixels to use as a reference
            byte[] originalPixels = new byte[pixels.Length];
            Array.Copy(pixels, originalPixels, pixels.Length);

            // Define the emboss kernel
            // This is a simplified 3x3 emboss kernel
            int[,] kernel = {
                { -2, -1, 0 },
                { -1, 1, 1 },
                { 0, 1, 2 }
            };

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    int pixelIndex = y * stride + x * 4;
                    for (int channel = 0; channel < 3; channel++)
                    {
                        int sum = 128; 
                        for (int ky = -1; ky <= 1; ky++)
                        {
                            for (int kx = -1; kx <= 1; kx++)
                            {
                                int sampleIndex = (y + ky) * stride + (x + kx) * 4;
                                sum += originalPixels[sampleIndex + channel] * kernel[ky + 1, kx + 1];
                            }
                        }

                        // Clamp the result to the [0, 255] range
                        sum = Math.Max(0, Math.Min(255, sum));
                        pixels[pixelIndex + channel] = (byte)sum;
                    }
                }
            }

            // Write the modified pixels back to the WriteableBitmap
            source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }
    }
}

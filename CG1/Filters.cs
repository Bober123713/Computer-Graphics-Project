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
using System.Threading.Channels;
using Media = System.Windows.Media;

namespace CG1;

public partial class MainWindow
{
    #region FUCTIONALFILTERS
    private void ApplyFunctionalFilter(WriteableBitmap source)
    {
        int width, height, stride;
        byte[] pixels;
        GetPixels(source, out width, out height, out stride, out pixels);
        byte[] bytes = GetBytesFromPolyline();

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = bytes[pixels[i]];
        }

        source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    }

    // Inversion

    //private void ApplyInversion(WriteableBitmap WriteableBitmap)
    //{
    //    int width, height, stride;
    //    byte[] pixels;
    //    GetPixels(WriteableBitmap, out width, out height, out stride, out pixels);

    //    pixels = Inversion(pixels);

    //    WriteableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    //}

    private static byte[] Inversion(byte[] pixels)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = (byte)(255 - pixels[i]);
        }
        return pixels;
    }
    
    // Brightness correction

    //private void ApplyBrightnessCorrection(WriteableBitmap source)
    //{
    //    int width, height, stride;
    //    byte[] pixels;
    //    GetPixels(source, out width, out height, out stride, out pixels);

    //    BrightnessCorrection(pixels);

    //    source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    //}

    private static byte[] BrightnessCorrection(byte[] pixels)
    {
        for (int i = 0; i < pixels.Length; i++)
        {
            int pixel = pixels[i] + BRIGHTNESS;
            if (pixel < 0) pixel = 0;
            if (pixel > 255) pixel = 255;
            pixels[i] = (byte)pixel;
        }

        return pixels;
    }

    // Contrast enhancement

    //private void ApplyContrastEnhancement(WriteableBitmap source)
    //{
    //    int width, height, stride;
    //    byte[] pixels;
    //    GetPixels(source, out width, out height, out stride, out pixels);

    //    ContrastEnhancement(pixels);

    //    source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    //}

    private static byte[] ContrastEnhancement(byte[] pixels)
    {
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

        return pixels;
    }

    //Gamma correction

    //private void ApplyGammaCorrection(WriteableBitmap source)
    //{
    //    int width, height, stride;
    //    byte[] pixels;
    //    GetPixels(source, out width, out height, out stride, out pixels);

    //    pixels = GammaCorrection(pixels);

    //    source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    //}

    private static byte[] GammaCorrection(byte[] pixels)
    {
        double[] gammaCorrectionTable = new double[256];
        for (int i = 0; i < 256; i++)
        {
            gammaCorrectionTable[i] = 255 * Math.Pow(i / 255.0, 1.0 / GAMMA);
        }

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = (byte)gammaCorrectionTable[pixels[i]];
        }

        return pixels;
    }

    //Default custom filter

    private void ApplyCustomFilter(WriteableBitmap source)
    {
        int width, height, stride;
        byte[] pixels;
        GetPixels(source, out width, out height, out stride, out pixels);

        pixels = CustomFilter(pixels);

        source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    }

    private static byte[] CustomFilter(byte[] pixels)
    {
        return pixels;
    }

    //Not default custom filter

    private void ApplyCustomFilterNotDefault(string filterName)
    {
        Queue.Add(new Filter(filterName, ApplyFunctionalFilter));
        ApplyNewest();
        MessageBox.Show($"Applying custom filter: {filterName}","Filter", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion

    #region CONVOLUTIONFILTERS
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
                    for (int ky = -1; ky <= 1; ky++)
                    {
                        for (int kx = -1; kx <= 1; kx++)
                        {
                            int sampleIndex = (y + ky) * stride + (x + kx) * 4;
                            sum += pixels[sampleIndex + channel];
                        }
                    }
                    pixels[index + channel] = (byte)(sum / 9);
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

        byte[] originalPixels = new byte[pixels.Length];
        Array.Copy(pixels, originalPixels, pixels.Length);

        double[,] kernel = {
            { 1/16.0, 2/16.0, 1/16.0 },
            { 2/16.0, 4/16.0, 2/16.0 },
            { 1/16.0, 2/16.0, 1/16.0 }
        };

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                double[] sum = new double[3];
                int index = y * stride + x * 4;

                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        int pixelIndex = ((y + ky) * stride) + ((x + kx) * 4);

                        for (int channel = 0; channel < 3; channel++)
                        {
                            sum[channel] += originalPixels[pixelIndex + channel] * kernel[ky + 1, kx + 1];
                        }
                    }
                }

                for (int channel = 0; channel < 3; channel++)
                {
                    pixels[index + channel] = (byte)Math.Clamp(sum[channel], 0, 255);
                }
            }
        }

        source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    }

    private void ApplySharpening(WriteableBitmap source)
    {
        int width, height, stride;
        byte[] pixels;
        GetPixels(source, out width, out height, out stride, out pixels);

        byte[] originalPixels = new byte[pixels.Length];
        Array.Copy(pixels, originalPixels, pixels.Length);

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
                for (int channel = 0; channel < 3; channel++)
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

                    pixels[pixelIndex + channel] = (byte)Math.Clamp(sum, 0, 255);
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

        byte[] edgePixels = new byte[pixels.Length];

        int[,] gx = new int[,] {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
        };

        int[,] gy = new int[,] {
            { -1, -2, -1 },
            { 0, 0, 0 },
            { 1, 2, 1 }
        };

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                float edgeX = 0;
                float edgeY = 0;
                int index = (y * stride) + (x * 4);

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

                byte edgeMagnitude = (byte)Math.Min(255, Math.Sqrt(edgeX * edgeX + edgeY * edgeY));

                for (int channel = 0; channel < 3; channel++)
                {
                    edgePixels[index + channel] = edgeMagnitude;
                }
            }
        }

        source.WritePixels(new Int32Rect(0, 0, width, height), edgePixels, stride, 0);
    }

    private void ApplyEmboss(WriteableBitmap source)
    {
        int width, height, stride;
        byte[] pixels;
        GetPixels(source, out width, out height, out stride, out pixels);

        byte[] originalPixels = new byte[pixels.Length];
        Array.Copy(pixels, originalPixels, pixels.Length);

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
                    pixels[pixelIndex + channel] = (byte)Math.Clamp(sum, 0, 255);
                }
            }
        }

        source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    }

    #endregion

    #region TASK2
    private void ApplyGrayscaleFilter(WriteableBitmap source)
    {
        int width, height, stride;
        byte[] pixels;
        GetPixels(source, out width, out height, out stride, out pixels);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte gray = (byte)(pixels[i + 2] * 0.299 + pixels[i + 1] * 0.587 + pixels[i] * 0.114);
            pixels[i] = gray;
            pixels[i + 1] = gray;
            pixels[i + 2] = gray;
        }

        source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    }

    private void ApplyRandomDithering(WriteableBitmap source)
    {
        int width, height, stride;
        byte[] pixels;
        GetPixels(source, out width, out height, out stride, out pixels);

        //ApplyGrayscaleFilter(source); // Could also do that but it is not necessary for it to functions

        Random rand = new Random();
        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte threshold = (byte)rand.Next(256);

            if (pixels[i] < threshold)
            {
                pixels[i] = 0; 
                pixels[i + 1] = 0; 
                pixels[i + 2] = 0; 
            }
            else
            {
                pixels[i] = 255; 
                pixels[i + 1] = 255; 
                pixels[i + 2] = 255; 
            }
        }

        source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    }

    private void ApplyOctreeQuantization(WriteableBitmap source)
    {
        int width, height, stride;
        byte[] pixels;
        GetPixels(source, out width, out height, out stride, out pixels);
        var colors = GetColors(pixels);
        var quantizer = new OctreeQuantizer();
        foreach (var color in colors)
            quantizer.AddColor(color);
        var colorCount = 0;
        try
        {
            colorCount = int.Parse(ColorCountTextBox.Text);
        }
        catch
        {
            return;
        }
        var palette = quantizer.MakePalette(colorCount);
        for(var i = 0; i < colors.Count; i++)
        {
            var index = quantizer.GetPaletteIndex(colors[i]);
            colors[i] = palette[index];
        }
        pixels = GetPixels(colors);
        source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    }

    
    #endregion
}

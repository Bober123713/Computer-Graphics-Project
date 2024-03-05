using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CG1
{
    public partial class MainWindow
    { 
        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var changedScrollViewer = sender as ScrollViewer;
            if (changedScrollViewer == ScrollViewerOriginal)
            {
                ScrollViewerEdited.ScrollToHorizontalOffset(changedScrollViewer.HorizontalOffset);
                ScrollViewerEdited.ScrollToVerticalOffset(changedScrollViewer.VerticalOffset);
            }
            else if (changedScrollViewer == ScrollViewerEdited)
            {
                ScrollViewerOriginal.ScrollToHorizontalOffset(changedScrollViewer.HorizontalOffset);
                ScrollViewerOriginal.ScrollToVerticalOffset(changedScrollViewer.VerticalOffset);
            }
        }

        private static void GetPixels(BitmapSource source, out int width, out int height, out int stride, out byte[] pixels)
        {
            width = source.PixelWidth;
            height = source.PixelHeight;
            stride = width * (source.Format.BitsPerPixel / 8);
            pixels = new byte[height * stride];
            source.CopyPixels(pixels, stride, 0);
        }

        private void DrawGridLines(Canvas canvas, int cellSize)
        {
            double width = canvas.Width;
            double height = canvas.Height;

            for (double y = 0; y <= height; y += cellSize)
            {
                Line line = new Line
                {
                    X1 = 0,
                    X2 = width,
                    Y1 = y,
                    Y2 = y,
                    Stroke = Brushes.LightSteelBlue,
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);
            }

            for (double x = 0; x <= width; x += cellSize)
            {
                Line line = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = 0,
                    Y2 = height,
                    Stroke = Brushes.LightSteelBlue,
                    StrokeThickness = 0.5
                };
                canvas.Children.Add(line);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
    }
}

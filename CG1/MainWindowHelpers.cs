using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using Media = System.Windows.Media;
using System.Windows.Shapes;

namespace CG1;

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

    private void AddNewFilterTabItem(string filterName)
    {
        TabItem newTabItem = new TabItem
        {
            Header = filterName,
            Name = filterName.Replace(" ", "_")
        };

        Button filterButton = new Button
        {
            Content = filterName,
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0.5)
        };

        filterButton.Click += (sender, e) => ApplyCustomFilterNotDefault(filterName);

        newTabItem.Content = filterButton;
        FunctionalFiltersTabControl.Items.Add(newTabItem);
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        TextBox textBox = sender as TextBox;
        string updatedText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);

        if (!e.Handled)
        {
            if (!int.TryParse(updatedText, out int result) || result == 0)
            {
                e.Handled = true; 
            }
        }
    }


    private byte[] GetPixels(List<Media.Color> colors)
    {
        var pixels = new byte[colors.Count * 4];

        for (var i = 0; i < colors.Count; i++)
        {
            pixels[4 * i + 0] = colors[i].B;
            pixels[4 * i + 1] = colors[i].G;
            pixels[4 * i + 2] = colors[i].R;
            pixels[4 * i + 3] = colors[i].A;
        }

        return pixels;
    }

    private List<Media.Color> GetColors(byte[] pixels)
    {
        var colors = new List<Media.Color>();

        for (var i = 0; i < pixels.Length; i += 4)
        {
            var b = pixels[i];
            var g = pixels[i + 1];
            var r = pixels[i + 2];
            var a = pixels[i + 3];
            colors.Add(Media.Color.FromArgb(a, r, g, b));
        }

        return colors;
    }
}

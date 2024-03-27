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
        Regex regex = new Regex("[^0-9]+");
        e.Handled = regex.IsMatch(e.Text);
    }
}

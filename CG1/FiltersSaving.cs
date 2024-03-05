using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CG1;

public partial class MainWindow
{
    private void SaveFilter_Click(object sender, RoutedEventArgs e)
    {
        byte[] values = GetBytesFromPolyline();
        string filterName = FilterNameTextBox.Text;

        if (!Dictionary.ContainsKey(filterName)) 
        {
            Dictionary.Add(filterName, values);

            AddTabItemForFilter(filterName);

            MessageBox.Show("Filter added successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("A filter with this name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddTabItemForFilter(string filterName)
    {
        var tabItem = new TabItem
        {
            Header = filterName
        };

        var applyButton = new Button
        {
            Content = "Apply " + filterName,
            Margin = new Thickness(0, 3, 0, 3),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0.5)
        };

        applyButton.Click += ApplyFilter_Click;

        var grid = new Grid();

        grid.Children.Add(applyButton);

        tabItem.Content = grid;

        FunctionalFiltersTabControl.Items.Add(tabItem);
    }

    private void ApplyFilter_Click(object sender, RoutedEventArgs e)
    {

        Queue.Add(new Filter(FilterNameTextBox.Text, ApplyFunctionalFilter));
        ApplyNewest();

    }
}

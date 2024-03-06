using System;
using System.IO;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CG1;

public partial class MainWindow
{
    private void SaveFilter_Click(object sender, RoutedEventArgs e)
    {
        byte[] values = GetBytesFromPolyline();
        string filterName = FilterNameTextBox.Text.Trim();

        if (string.IsNullOrEmpty(filterName) || CustomFilters.ContainsKey(filterName))
        {
            MessageBox.Show($"A filter named {filterName} already exists or the name is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        CustomFilters[filterName] = values;

        SaveFiltersToJson();

        AddNewFilterTabItem(filterName);
    }

    private void SaveFiltersToJson()
    {
        try
        {
            string json = JsonConvert.SerializeObject(CustomFilters);
            File.WriteAllText("CustomFilters.json", json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save filters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadFiltersFromJson()
    {
        try
        {
            if (File.Exists("CustomFilters.json"))
            {
                string json = File.ReadAllText("CustomFilters.json");
                CustomFilters = JsonConvert.DeserializeObject<Dictionary<string, byte[]>>(json) ?? new Dictionary<string, byte[]>();

                foreach (var filterName in CustomFilters.Keys)
                {
                    AddNewFilterTabItem(filterName);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load filters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddNewFilterTabItem(string filterName)
    {
        TabItem newTabItem = new TabItem
        {
            Header = filterName
        };

        Button filterButton = new Button
        {
            Content = filterName,
            //Margin = new Thickness(0, 3),
            BorderBrush = Brushes.LightGray,
            BorderThickness = new Thickness(0.5)
        };

        filterButton.Click += (sender, e) => ApplyCustomFilter(filterName);

        newTabItem.Content = filterButton;
        FunctionalFiltersTabControl.Items.Add(newTabItem);
    }

    private void ApplyCustomFilter(string filterName)
    {
        // Assuming you have a method to apply a filter by name
        // This is just a placeholder to show where you might hook in the application of the filter
        Queue.Add(new Filter(filterName, ApplyFunctionalFilter));
        ApplyNewest();
        MessageBox.Show($"Applying filter: {filterName}");
    }

    // Your existing methods to get byte[] from polyline, etc.
    //private byte[] GetBytesFromPolyline()
    //{
    //    // Placeholder for your actual implementation
    //    return new byte[0];
    //}
}

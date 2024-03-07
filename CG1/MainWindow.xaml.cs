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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace CG1;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private const int BRIGHTNESS = 20;
    private const int CONTRAST = 20;
    private const double GAMMA = 2.2;
    private Dictionary<string, byte[]> Dictionary { get; set; } = [];
    private Dictionary<string, byte[]> CustomFilters { get; set; } = [];
    public ObservableCollection<Filter> Queue { get; set; } = [];



    private WriteableBitmap original;

    public WriteableBitmap Original
    {
        get => original;
        set => SetField(ref original, value);
    }

    private WriteableBitmap edited;

    public WriteableBitmap Edited
    {
        get => edited;
        set => SetField(ref edited, value);
    }
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        var source = new Uri("..\\..\\..\\sampleimage.jpg", UriKind.Relative);
        var image = new BitmapImage(source);
        Original = new WriteableBitmap(image);
        Edited = new WriteableBitmap(image);
        LoadFiltersFromJson();
        InitializeFunctionDictionary();
    }

    private void InitializeFunctionDictionary()
    {
        var bytes = new byte[256];
        for (var i = 0; i < 256; i++)
            bytes[i] = (byte)i;

        AddFunction("TIInverse", Inversion, bytes);
        AddFunction("TIBrightnessCorrection", BrightnessCorrection, bytes);
        AddFunction("TIContrastEnhancement", ContrastEnhancement, bytes);
        AddFunction("TIGammaCorrection", GammaCorrection, bytes);
        AddFunction("TICustomFilter", CustomFilter, bytes);
    }

    private void AddFunction(string name, Func<byte[], byte[]> function, byte[] bytes)
    {
        var tempBytes = new byte[256];
        Array.Copy(bytes, tempBytes, 256);
        tempBytes = function.Invoke(tempBytes);
        Dictionary.Add(name, tempBytes);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void LoadImage_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*";
        if (openFileDialog.ShowDialog() == true)
        {
            var source = new Uri(openFileDialog.FileName);
            var image = new BitmapImage(source);
            Original = new WriteableBitmap(image);
            Edited = new WriteableBitmap(image);
            Queue.Clear();
        }
    }

    private void SaveImage_Click(object sender, RoutedEventArgs e)
    {
        if (EditedImage.Source is not BitmapSource source)
        {
            MessageBox.Show("There is no image to save.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "PNG Image|*.png|JPEG Image|*.jpg|Bitmap Image|*.bmp",
            FileName = "Image",
            DefaultExt = ".png"
        };

        if (saveFileDialog.ShowDialog() != true) return;

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

    private void ApplyNewest()
    {
        var filter = Queue.Last();
        filter.Apply(Edited);
    }

    private void FunctionalFiltersTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.AddedItems[0] as TabItem;
        var name = selected?.Name.Replace("_", " ");
        if (name is null || !(Dictionary.ContainsKey(name) || CustomFilters.ContainsKey(name)))
            return;

        var values = Dictionary.ContainsKey(name) ? Dictionary[name] : CustomFilters[name];
        var points = new PointCollection();

        for (var i = 0; i < 256; i++)
            points.Add(new Point(i, 255 - values[i]));

        PointCollection userPoints = [points[0], points[255]];

        UserPoints = userPoints;
        PolylinePoints = points;
    }


    #region BUTTONCLICKS

    private void ResetImage_Click(object sender, RoutedEventArgs e)
    {
        Queue.Clear();
        Edited = new WriteableBitmap(Original);
    }

    private void Inverse_Click(object sender, RoutedEventArgs e)
    {
        Queue.Add(new Filter("Invert", ApplyFunctionalFilter));
        ApplyNewest();
    }

    private void BrightnessCorrection_Click(object sender, RoutedEventArgs e)
    {
        Queue.Add(new Filter("Brightness Correction", ApplyFunctionalFilter));
        ApplyNewest();
    }

    private void ContrastEnhancement_Click(object sender, RoutedEventArgs e)
    {
        Queue.Add(new Filter("Contrast Enhancement", ApplyFunctionalFilter));
        ApplyNewest();
    }

    private void GammaCorrection_Click(object sender, RoutedEventArgs e)
    {
        Queue.Add(new Filter("Gamma Correction", ApplyFunctionalFilter));
        ApplyNewest();
    }

    private void CustomFilter_Click(object sender, RoutedEventArgs e)
    {
        Queue.Add(new Filter("Custom Filter", ApplyFunctionalFilter));
        ApplyNewest();
    }

    private void Blur_Click(object sender, RoutedEventArgs e)
    {
        Queue.Add(new Filter("Blur", ApplyBlur));
        ApplyNewest();
    }

    private void Sharpen_Click(object sender, RoutedEventArgs e)
    {
        Queue.Add(new Filter("Sharpen", ApplySharpening));
        ApplyNewest();
    }

    private void Edge_Click(object sender, RoutedEventArgs e)
    {
        Queue.Add(new Filter("Edge Detection", ApplySobelEdgeDetection));
        ApplyNewest();
    }

    private void GaussianBlur_Click(object sender, RoutedEventArgs e)
    {
        Queue.Add(new Filter("Gaussian Blur", ApplyGaussianBlur));
        ApplyNewest();
    }

    private void Emboss_Click(object sender, RoutedEventArgs e)
    {
        Queue.Add(new Filter("Emboss", ApplyEmboss));
        ApplyNewest();
    }

    #endregion

    #region RGBVSHTASK
    private void RgbToHsv_Click(object sender, RoutedEventArgs e)
    {
        int width = Edited.PixelWidth;
        int height = Edited.PixelHeight;
        int stride = width * (Edited.Format.BitsPerPixel / 8);
        byte[] pixels = new byte[height * stride];
        Edited.CopyPixels(pixels, stride, 0);

        byte r, g, b;
        int index;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                index = y * stride + 4 * x;
                b = pixels[index];
                g = pixels[index + 1];
                r = pixels[index + 2];

                byte[] hsv = RgbToHsv(r, g, b);

                pixels[index + 2] = hsv[0]; 
                pixels[index + 1] = hsv[1]; 
                pixels[index] = hsv[2]; 
            }
        }

        Edited.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    }

    public static byte[] RgbToHsv(byte red, byte green, byte blue)
    {
        var r = red / 255.0;
        var g = green / 255.0;
        var b = blue / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        double h, s, v;
        h = 0; // default to black

        if (delta != 0)
        {
            if (max == r)
            {
                h = (g - b) / delta;
            }
            else if (max == g)
            {
                h = 2 + (b - r) / delta;
            }
            else
            {
                h = 4 + (r - g) / delta;
            }
            h *= 60;
            if (h < 0) h += 360;
        }

        v = max;
        s = max == 0 ? 0 : delta / max;

        return new byte[] {
        (byte)((h / 360) * 255),
        (byte)(s * 255),
        (byte)(v * 255)
    };
    }


    private void HsvToRgb_Click(object sender, RoutedEventArgs e)
    {
        int width = Edited.PixelWidth;
        int height = Edited.PixelHeight;
        int stride = width * (Edited.Format.BitsPerPixel / 8);
        byte[] pixels = new byte[height * stride];
        Edited.CopyPixels(pixels, stride, 0);
        byte hue, saturation, value;
        int index;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                index = y * stride + 4 * x;
                hue = pixels[index + 2]; 
                saturation = pixels[index + 1]; 
                value = pixels[index]; 

                byte[] rgb = HsvToRgb(hue, saturation, value);

                pixels[index] = rgb[2]; 
                pixels[index + 1] = rgb[1]; 
                pixels[index + 2] = rgb[0]; 
            }
        }

        Edited.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
    }


    public static byte[] HsvToRgb(byte h, byte s, byte v)
    {
        var hue = h * 360.0 / 255.0;
        var saturation = s / 255.0;
        var value = v / 255.0;
        double f, p, q, t;

        int i;

        if (saturation == 0)
        {
            byte grey = Convert.ToByte(value * 255);
            return new byte[] { grey, grey, grey };
        }

        hue /= 60; 
        i = (int)Math.Floor(hue);
        f = hue - i; 
        p = value * (1 - saturation);
        q = value * (1 - saturation * f);
        t = value * (1 - saturation * (1 - f));

        double r, g, b;
        switch (i)
        {
            case 0:
                r = value;
                g = t;
                b = p;
                break;
            case 1:
                r = q;
                g = value;
                b = p;
                break;
            case 2:
                r = p;
                g = value;
                b = t;
                break;
            case 3:
                r = p;
                g = q;
                b = value;
                break;
            case 4:
                r = t;
                g = p;
                b = value;
                break;
            default:    
                r = value;
                g = p;
                b = q;
                break;
        }

        return new byte[] {
        Convert.ToByte(Math.Round(r * 255)),
        Convert.ToByte(Math.Round(g * 255)),
        Convert.ToByte(Math.Round(b * 255))
        };
    }
    #endregion
}


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

namespace CG1
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const int BRIGHTNESS = 20;
        private const int CONTRAST = 20;
        private const double GAMMA = 2.2;

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
            InitializeIdentityFilter();
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
           
        }


        #region BUTTONCLICKS

        private void ResetImage_Click(object sender, RoutedEventArgs e)
        {
            Queue.Clear();
            Edited = new WriteableBitmap(Original);
            var removablePoints = FilterCanvas.Children.OfType<Ellipse>()
                                .Where(ellipse => ellipse.Tag?.ToString() != "Immutable").ToList();
            foreach (var point in removablePoints)
            {
                FilterCanvas.Children.Remove(point);
            }
            UpdatePolyline();
        }

        private void Blur_Click(object sender, RoutedEventArgs e)
        {
            Queue.Add(new Filter("Blur", ApplyBlur));
            ApplyNewest();
        }

        private void Inverse_Click(object sender, RoutedEventArgs e)
        {
            Queue.Add(new Filter("Invert", ApplyInversion));
            ApplyNewest();
        }

        private void BrightnessCorrection_Click(object sender, RoutedEventArgs e)
        {
            Queue.Add(new Filter ("Brightness Correction", ApplyBrightnessCorrection));
            ApplyNewest();
        }

        private void ContrastEnhancement_Click(object sender, RoutedEventArgs e)
        {
            Queue.Add(new Filter ("Contrast Enhancement",ApplyContrastEnhancement));
            ApplyNewest();
        }

        private void GammaCorrection_Click(object sender, RoutedEventArgs e)
        {
            Queue.Add(new Filter ("Gamma Correction", ApplyGammaCorrection));
            ApplyNewest();
        }

        private void Sharpen_Click(object sender, RoutedEventArgs e)
        {
            Queue.Add(new Filter ("Sharpen", ApplySharpening));
            ApplyNewest();
        }

        private void Edge_Click(object sender, RoutedEventArgs e)
        {
            Queue.Add(new Filter ("Edge Detection", ApplySobelEdgeDetection));
            ApplyNewest();
        }

        private void GaussianBlur_Click(object sender, RoutedEventArgs e)
        {
            Queue.Add(new Filter("Gaussian Blur", ApplyGaussianBlur));
            ApplyNewest();
        }

        private void Emboss_Click(object sender, RoutedEventArgs e)
        {
            Queue.Add(new Filter("Emboss",ApplyEmboss));
            ApplyNewest();
        }

        #endregion
    }
}


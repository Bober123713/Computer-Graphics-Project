using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CG1
{
    public partial class MainWindow
    {
        public static int pointSize = 7;

        private Point? lastDragPoint = null;
        private Polyline currentPolyline = null;
        private Ellipse? selectedPoint = null;

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(FilterCanvas);

            foreach (var child in FilterCanvas.Children.OfType<Ellipse>())
            {
                var left = Canvas.GetLeft(child);
                var top = Canvas.GetTop(child);
                var rect = new Rect(left, top, child.Width, child.Height);
                if (rect.Contains(mousePos))
                {
                    selectedPoint = child;
                    lastDragPoint = mousePos;
                    return;
                }
            }

            var point = new Ellipse
            {
                Fill = Brushes.Black,
                Width = pointSize,
                Height = pointSize,
            };

            Canvas.SetLeft(point, mousePos.X - point.Width / 2);
            Canvas.SetTop(point, mousePos.Y - point.Height / 2);
            FilterCanvas.Children.Add(point);

            UpdatePolyline();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && lastDragPoint.HasValue && selectedPoint != null)
            {
                var newPos = e.GetPosition(FilterCanvas);

                if (selectedPoint.Tag != null && selectedPoint.Tag.ToString() == "Immutable")
                {
                    Canvas.SetTop(selectedPoint, newPos.Y - selectedPoint.Height / 2);
                }
                else
                {
                    Canvas.SetLeft(selectedPoint, newPos.X - selectedPoint.Width / 2);
                    Canvas.SetTop(selectedPoint, newPos.Y - selectedPoint.Height / 2);
                }

                lastDragPoint = newPos;

                UpdatePolyline();
            }
        }


        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            lastDragPoint = null;
            selectedPoint = null;
            ApplyCustomFilter(Edited);
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(FilterCanvas);

            Ellipse? pointToRemove = null;
            foreach (var child in FilterCanvas.Children.OfType<Ellipse>())
            {
                var left = Canvas.GetLeft(child);
                var top = Canvas.GetTop(child);
                var rect = new Rect(left, top, child.Width, child.Height);
                if (rect.Contains(mousePos))
                {
                    pointToRemove = child;
                    break;
                }
            }

            if (pointToRemove != null)
            {
                FilterCanvas.Children.Remove(pointToRemove);
                UpdatePolyline();
            }
            ApplyCustomFilter(Edited);
        }

        private Func<byte, byte> ConvertPolylineToFilterFunction()
        {
            var points = currentPolyline.Points.OrderBy(p => p.X).ToList();

            return (input) =>
            {
                double x = input / 255.0 * FilterCanvas.Width;
                for (int i = 0; i < points.Count - 1; i++)
                {
                    if (x >= points[i].X && x <= points[i + 1].X)
                    {
                        double t = (x - points[i].X) / (points[i + 1].X - points[i].X);
                        double y = points[i].Y + t * (points[i + 1].Y - points[i].Y);
                        return (byte)(y / FilterCanvas.Height * 255);
                    }
                }
                return input;
            };
        }

        private void ApplyCustomFilter(WriteableBitmap source)
        {
            var filterFunction = ConvertPolylineToFilterFunction();

            int width, height, stride;
            byte[] pixels;
            MainWindow.GetPixels(source, out width, out height, out stride, out pixels);

            if (currentPolyline.Points.Count == 2 &&
                currentPolyline.Points[0] == new Point(0, FilterCanvas.Height) &&
                currentPolyline.Points[1] == new Point(FilterCanvas.Width, 0))
            {
                return;
            }

            for (int i = 0; i < pixels.Length; i += 4)
            { 
                pixels[i] = filterFunction(pixels[i]);     
                pixels[i + 1] = filterFunction(pixels[i + 1]); 
                pixels[i + 2] = filterFunction(pixels[i + 2]);                                             
            }

            source.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        }

        private void UpdatePolyline()
        {
            if (currentPolyline == null)
            {
                currentPolyline = new Polyline
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                FilterCanvas.Children.Add(currentPolyline);
            }

            var points = FilterCanvas.Children.OfType<Ellipse>()
                .Select(ellipse => new Point(Canvas.GetLeft(ellipse) + ellipse.Width / 2, Canvas.GetTop(ellipse) + ellipse.Height / 2))
                .OrderBy(point => point.X).ToList();

            currentPolyline.Points = new PointCollection(points);
        }

        private void CreateImmutablePoint(double x, double y)
        {
            var point = new Ellipse
            {
                Fill = Brushes.Black,
                Width = pointSize,
                Height = pointSize,
                Tag = "Immutable"
            };

            Canvas.SetLeft(point, x - point.Width / 2);
            Canvas.SetTop(point, y - point.Height / 2);
            FilterCanvas.Children.Add(point);
        }

        private void InitializeIdentityFilter()
        {
            DrawGridLines(FilterCanvas, 10);
            CreateImmutablePoint(0, FilterCanvas.Height); 
            CreateImmutablePoint(FilterCanvas.Width, 0); 
            currentPolyline = new Polyline
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            FilterCanvas.Children.Add(currentPolyline);

            UpdatePolyline();
        }
    }
}

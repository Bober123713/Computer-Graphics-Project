using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CG1;

public partial class MainWindow
{
    private PointCollection _polylinePoints;
    private PointCollection _userPoints;

    public PointCollection PolylinePoints
    {
        get => _polylinePoints;
        set => SetField(ref _polylinePoints, value);
    }

    public PointCollection UserPoints
    {
        get => _userPoints;
        set => SetField(ref _userPoints, value);
    }

    private void RightClickCanvas(object sender, MouseButtonEventArgs e)
    {
        const int size = 9;

        var pos = e.GetPosition(sender as Canvas);
        var x = (byte)pos.X;

        var userPoints = UserPoints.Clone();
        if (userPoints.Count < 2)
            return;

        var selectedPoint = userPoints.FirstOrDefault(p => Math.Abs(p.X - x) < size);

        if (selectedPoint == default || selectedPoint.Equals(userPoints.First()) || selectedPoint.Equals(userPoints.Last()))
            return;

        userPoints.Remove(selectedPoint);
        var previousPoint = userPoints.Last(p => p.X < x);
        var nextPoint = userPoints.First(p => p.X > x);


        var points = PolylinePoints.Clone();

        MovePointsBetween(previousPoint, nextPoint, ref points);

        UserPoints = userPoints;
        PolylinePoints = points;
    }

    private void ClickCanvas(object sender, MouseButtonEventArgs e)
    {
        const int Size = 9;

        var pos = e.GetPosition(sender as Canvas);
        var x = (byte)pos.X;
        var y = (byte)pos.Y;

        if (UserPoints.Any(p => Math.Abs(p.X - x) < Size))
        {
            MovePoint();
            return;
        }

        var thisPoint = new Point(x, y);
        var userPoints = UserPoints.Clone();
        userPoints.Add(thisPoint);
        userPoints = new PointCollection(userPoints.OrderBy(p => p.X));
        var previousPoint = UserPoints.LastOrDefault(p => p.X < x);
        var nextPoint = UserPoints.FirstOrDefault(p => p.X > x);

        var points = PolylinePoints.Clone();

        MovePointsBetween(previousPoint, thisPoint, ref points);
        MovePointsBetween(thisPoint, nextPoint, ref points);

        UserPoints = userPoints;
        PolylinePoints = points;
    }


    private void MovePointsBetween(Point start, Point end, ref PointCollection points)
    {
        var deltaY = end.Y - start.Y;
        var deltaX = end.X - start.X;
        var step = deltaY / deltaX;
        for (var i = (byte)start.X; i < end.X; i++)
            points[i] = new Point(points[i].X, start.Y + (i - (byte)start.X) * step);
    }

    private void MovePoint()
    {
        return;
    }

    private byte[] GetBytesFromPolyline()
    {
        return PolylinePoints.Select(p => (byte)(255 - p.Y)).ToArray();
    }
}


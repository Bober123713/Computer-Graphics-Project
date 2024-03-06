using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CG1;

public partial class MainWindow
{
    private PointCollection _polylinePoints;
    private PointCollection _userPoints;
    private Point? _selectedPoint = null;

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
        const int Size = 7;

        var pos = e.GetPosition(sender as Canvas);
        var x = (byte)pos.X;
        var y = (byte)pos.Y;

        var closestPointIndex = UserPoints
        .Select((p, index) => new { Point = p, Index = index })
        .Where(p => Math.Abs(p.Point.X - x) < Size && Math.Abs(p.Point.Y - y) < Size)
        .OrderBy(p => Math.Sqrt(Math.Pow(p.Point.X - x, 2) + Math.Pow(p.Point.Y - y, 2)))
        .Select(p => p.Index)
        .FirstOrDefault();

        if (UserPoints.Any(p => Math.Abs(p.X - x) < Size))
        {
            _selectedPoint = UserPoints[closestPointIndex];
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

    private void LeftMouseUpCanvas(object sender, MouseButtonEventArgs e)
    {
        _selectedPoint = null;
    }

    private void MouseMoveCanvas(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && _selectedPoint.HasValue)
        {
            const int Size = 7;

            var newPos = e.GetPosition(sender as Canvas);
            int index = UserPoints.IndexOf(_selectedPoint.Value);
            var userPoints = UserPoints.Clone();
            var points = PolylinePoints.Clone();

            if (index != -1)
            {
                if(index == 0 || 
                    index == userPoints.Count - 1) { 
                    var newX = userPoints[index].X; 
                    var newY = newPos.Y;
                    newPos = new Point(newX, newY); 
                }
                else if (userPoints[index - 1].X + Size > newPos.X ||
                    newPos.X + Size > userPoints[index + 1].X)
                {
                    var newX = userPoints[index].X; 
                    var newY = newPos.Y; 
                    newPos = new Point(newX, newY);
                }

                userPoints[index] = newPos;

                if (index > 0)
                {
                    var previousPoint = userPoints[index - 1];
                    MovePointsBetween(previousPoint, newPos, ref points);
                }

                if (index < userPoints.Count - 1)
                {
                    var nextPoint = userPoints[index + 1];
                    MovePointsBetween(newPos, nextPoint, ref points);
                }

                _selectedPoint = newPos;

                UserPoints = userPoints;
                PolylinePoints = points;
            }
        }
    }

    private void MovePointsBetween(Point start, Point end, ref PointCollection points)
    {
        var deltaY = end.Y - start.Y;
        var deltaX = end.X - start.X;
        var step = deltaY / deltaX;
        for (var i = (int)start.X; i <= end.X; i++)
            points[i] = new Point(points[i].X, start.Y + (i - (byte)start.X) * step);
    }

    private byte[] GetBytesFromPolyline()
    {
        return PolylinePoints.Select(p => (byte)(255 - p.Y)).ToArray();
    }
}


using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SqlSecAuditor.Converters
{
    public class ScoreToArcConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 4)
                return Geometry.Empty;

            if (!TryToDouble(values[0], out var score) ||
                !TryToDouble(values[1], out var minScore) ||
                !TryToDouble(values[2], out var maxScore) ||
                !TryToDouble(values[3], out var size))
            {
                return Geometry.Empty;
            }

            var range = maxScore - minScore;
            if (range <= 0 || size <= 0)
                return Geometry.Empty;

            var normalized = (score - minScore) / range;
            normalized = Math.Max(0, Math.Min(1, normalized));

            if (normalized <= 0.0001)
                return Geometry.Empty;

            var startAngle = -90d;
            var sweep = 360d * normalized;
            var endAngle = startAngle + sweep;

            var center = new Point(size / 2d, size / 2d);
            var radius = (size / 2d) - 8d;

            var start = PointOnCircle(center, radius, startAngle);
            var end = PointOnCircle(center, radius, endAngle);
            var isLargeArc = sweep > 180d;

            var figure = new PathFigure { StartPoint = start, IsClosed = false };
            figure.Segments.Add(new ArcSegment
            {
                Point = end,
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = isLargeArc
            });

            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return geometry;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static bool TryToDouble(object value, out double result)
        {
            switch (value)
            {
                case double d:
                    result = d;
                    return true;
                case float f:
                    result = f;
                    return true;
                case int i:
                    result = i;
                    return true;
                case long l:
                    result = l;
                    return true;
                case decimal m:
                    result = (double)m;
                    return true;
                default:
                    result = 0;
                    return false;
            }
        }

        private static Point PointOnCircle(Point center, double radius, double angleInDegrees)
        {
            var radians = angleInDegrees * Math.PI / 180d;
            return new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
        }
    }
}

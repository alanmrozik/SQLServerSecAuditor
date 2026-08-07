using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SqlSecAuditor.Converters
{
    public class ScoreToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3)
                return Brushes.Gray;

            if (!TryToDouble(values[0], out var score) ||
                !TryToDouble(values[1], out var minScore) ||
                !TryToDouble(values[2], out var maxScore))
            {
                return Brushes.Gray;
            }

            var range = maxScore - minScore;
            if (range <= 0)
                return Brushes.Gray;

            var normalized = (score - minScore) / range;

            if (normalized >= 0.75)
                return (Brush)new BrushConverter().ConvertFrom("#2ECC71")!;
            if (normalized >= 0.5)
                return (Brush)new BrushConverter().ConvertFrom("#27AE60")!;
            if (normalized >= 0.35)
                return (Brush)new BrushConverter().ConvertFrom("#F1C40F")!;
            if (normalized >= 0.2)
                return (Brush)new BrushConverter().ConvertFrom("#F39C12")!;
            return (Brush)new BrushConverter().ConvertFrom("#E74C3C")!;
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
    }
}

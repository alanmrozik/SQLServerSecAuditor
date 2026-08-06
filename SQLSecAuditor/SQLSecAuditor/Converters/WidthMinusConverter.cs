using System;
using System.Globalization;
using System.Windows.Data;

namespace SqlSecAuditor.Converters
{
    // Subtracts a numeric parameter from the incoming double value (e.g. ActualWidth - 40)
    public class WidthMinusConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double d) return value ?? 0.0;

            double subtract = 0;
            if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var p))
            {
                subtract = p;
            }

            var result = d - subtract;
            return result > 0 ? result : 0.0;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

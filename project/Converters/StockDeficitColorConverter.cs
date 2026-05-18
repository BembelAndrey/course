using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace project.Converters
{
    public class StockDeficitColorConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int deficit)
            {
                if (parameter?.ToString() == "IsDeficit")
                    return deficit > 0;

                if (deficit > 0)
                    return new SolidColorBrush(Color.FromRgb(255, 179, 177)); // Acoustic Red
            }
            
            if (parameter?.ToString() == "IsDeficit")
                return false;

            return new SolidColorBrush(Color.FromRgb(39, 174, 96)); // Green
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 1 && values[0] is int deficit)
            {
                if (deficit > 0)
                    return new SolidColorBrush(Color.FromRgb(255, 179, 177)); // Acoustic Red
            }
            return new SolidColorBrush(Color.FromRgb(39, 174, 96)); // Green
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

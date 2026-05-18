using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace project.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter as string == "hideIfZero" && value is int intVal)
            {
                return intVal > 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            bool invert = parameter as string == "Invert";
            bool isEmpty = string.IsNullOrWhiteSpace(value as string);
            
            if (invert)
            {
                return isEmpty ? Visibility.Visible : Visibility.Collapsed;
            }
            return isEmpty ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
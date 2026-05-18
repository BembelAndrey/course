using System;
using System.Globalization;
using System.Windows.Data;
using project.Models;
using project.Services;

namespace project.Converters
{
    public class OrderStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                string key = status.ToString();
                return TranslationSource.Instance[key];
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
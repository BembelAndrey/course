using System;
using System.Globalization;
using System.Windows.Data;
using project.Services;

namespace project.Converters
{
    public class LanguageStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] != null && values[1] is string lang)
            {
                string key = values[0].ToString();
                return TranslationSource.Instance[key];
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

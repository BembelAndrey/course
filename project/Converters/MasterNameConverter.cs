using System;
using System.Globalization;
using System.Windows.Data;
using project.Models;
using project.Services;

namespace project.Converters
{
    public class MasterNameConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is User user && values[1] is string lang)
            {
                return lang == "RU" ? user.FullNameRu : user.FullNameEn;
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace project.Converters
{
    public class CalendarDayColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] = Date (DateTime)
            // values[1] = WorkDays (IEnumerable<DateTime>)
            if (values.Length >= 2 && values[0] is DateTime date && values[1] is IEnumerable<DateTime> workDays)
            {
                // По умолчанию выходные - Красные
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                {
                    return new SolidColorBrush(Color.FromRgb(231, 76, 60)); // Red
                }

                // Рабочие дни (есть заказы) - Синие
                foreach (var wd in workDays)
                {
                    if (wd.Date == date.Date)
                    {
                        return new SolidColorBrush(Color.FromRgb(52, 152, 219)); // Blue
                    }
                }
            }
            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

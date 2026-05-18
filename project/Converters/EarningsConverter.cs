using System;
using System.Globalization;
using System.Windows.Data;
using project.Models;
using project.Services;

namespace project.Converters
{
    public class EarningsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Order order)
            {
                if (order.Area <= 0) return "0 $";

                decimal rate = order.SurfaceType switch
                {
                    "Floor" => 30m,
                    "Wall" => 50m,
                    "Ceiling" => 70m,
                    _ => 0m
                };

                return $"{rate * (decimal)order.Area} $";
            }
            return "0 $";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace project.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            if (string.IsNullOrWhiteSpace(path)) return null;

            try
            {
                // Если путь уже абсолютный
                if (Path.IsPathRooted(path) && File.Exists(path))
                {
                    return new BitmapImage(new Uri(path));
                }

                // Пробуем найти относительно базовой директории приложения
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                if (File.Exists(fullPath))
                {
                    return new BitmapImage(new Uri(fullPath));
                }
            }
            catch
            {
                // Игнорируем ошибки загрузки картинки, чтобы не крашить приложение
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

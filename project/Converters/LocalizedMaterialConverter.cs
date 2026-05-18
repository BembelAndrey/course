using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using project.Models;
using project.Services;

namespace project.Converters
{
    public class LocalizedMaterialConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return string.Empty;

            string lang = values[1] as string ?? "RU";

            if (values[0] is Material mat)
            {
                string prop = parameter as string;
                if (prop == "Title") return lang == "RU" ? mat.TitleRu : mat.TitleEn;
                if (prop == "Description") return lang == "RU" ? mat.DescriptionRu : mat.DescriptionEn;
                if (prop == "Category") return lang == "RU" ? mat.CategoryRu : TranslationSource.Instance[mat.CategoryEn];
            }
            
            if (values[0] is Order order)
            {
                if (parameter?.ToString() == "OrderName")
                {
                    if (order.OrderName == "CatalogOrder")
                        return TranslationSource.Instance[lang, "CatalogOrder"];
                    
                    if (order.OrderName.StartsWith("CalcOrder|"))
                    {
                        var parts = order.OrderName.Split('|');
                        if (parts.Length >= 3)
                        {
                            string surfKey = parts[1];
                            string lvlKey = parts[2];
                            string surf = TranslationSource.Instance[lang, surfKey];
                            string lvl = TranslationSource.Instance[lang, lvlKey];
                            
                            if (lang == "RU")
                                return $"Монтаж звукоизоляции: {surf} ({lvl}), {order.Area} кв.м.";
                            else
                                return $"Soundproofing installation: {surf} ({lvl}), {order.Area} sq.m.";
                        }
                    }

                    // Для совместимости со старыми заказами
                    if (order.OrderName == "Заказ из каталога" || order.OrderName == "Catalog Order")
                        return TranslationSource.Instance[lang, "CatalogOrder"];

                    return order.OrderName;
                }
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

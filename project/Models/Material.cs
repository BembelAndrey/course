using System.Collections.Generic;

namespace project.Models
{
    public class Material
    {
        public int Id { get; set; }
        public string TitleRu { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string DescriptionRu { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string CategoryRu { get; set; } = string.Empty;
        public string CategoryEn { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        public int StockQuantity { get; set; } = 100; // По умолчанию на складе 100 шт.

        public double AverageRating { get; set; }
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<User> ViewedByUsers { get; set; } = new List<User>();
    }
}
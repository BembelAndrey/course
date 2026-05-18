using project.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using project.Models;

namespace project.Services
{
    public class CalculatorService
    {
        public const string SurfWall = "Wall";
        public const string SurfFloor = "Floor";
        public const string SurfCeiling = "Ceiling";

        public const string LvlEco = "Economy";
        public const string LvlComf = "Comfort";
        public const string LvlStd = "Standard";
        public const string LvlPrem = "Premium";

        private readonly Dictionary<string, decimal> _prices = new()
        {
            { $"{SurfWall}_{LvlEco}", 60m },
            { $"{SurfWall}_{LvlComf}", 90m },
            { $"{SurfWall}_{LvlStd}", 120m },
            { $"{SurfWall}_{LvlPrem}", 180m },

            { $"{SurfFloor}_{LvlStd}", 100m },
            { $"{SurfFloor}_{LvlPrem}", 150m },

            { $"{SurfCeiling}_{LvlEco}", 70m },
            { $"{SurfCeiling}_{LvlStd}", 130m },
            { $"{SurfCeiling}_{LvlPrem}", 200m }
        };

        public decimal CalculateTotalCost(decimal basePrice, int quantity, bool isUrgent)
        {
            decimal total = basePrice * quantity;
            if (isUrgent) total *= 1.5m;
            if (quantity >= 5) total *= 0.9m;
            return Math.Round(total, 2);
        }

        public decimal CalculateWorkCost(string surfaceType, string workLevel, double area)
        {
            string key = $"{surfaceType}_{workLevel}";
            if (_prices.TryGetValue(key, out decimal pricePerSqm))
            {
                return pricePerSqm * (decimal)area;
            }
            return 0m;
        }

        public List<string> GetLevelsForSurface(string surfaceType)
        {
            return surfaceType switch
            {
                SurfWall => new List<string> { LvlEco, LvlComf, LvlStd, LvlPrem },
                SurfFloor => new List<string> { LvlStd, LvlPrem },
                SurfCeiling => new List<string> { LvlEco, LvlStd, LvlPrem },
                _ => new List<string>()
            };
        }

        public List<OrderItem> DeductMaterialsForService(string surfaceType, string workLevel, double area, AppDbContext context)
        {
            var materials = context.Materials.ToList();
            var bomItems = new List<OrderItem>();

            void DeductExact(string titleRu, int qty)
            {
                var mat = materials.FirstOrDefault(m => m.TitleRu.Equals(titleRu, StringComparison.OrdinalIgnoreCase));
                if (mat != null)
                {
                    int taken = Math.Min(qty, mat.StockQuantity);
                    int deficit = qty - taken;
                    
                    bomItems.Add(new OrderItem { MaterialId = mat.Id, Material = mat, Quantity = qty, Deficit = deficit, Price = mat.Price });
                    
                    mat.StockQuantity -= taken;
                }
            }

            void Deduct(string titleRuSubstring, int qty)
            {
                var mat = materials.FirstOrDefault(m => m.TitleRu.ToLower().Contains(titleRuSubstring.ToLower()));
                if (mat != null)
                {
                    int taken = Math.Min(qty, mat.StockQuantity);
                    int deficit = qty - taken;

                    bomItems.Add(new OrderItem { MaterialId = mat.Id, Material = mat, Quantity = qty, Deficit = deficit, Price = mat.Price });
                    
                    mat.StockQuantity -= taken;
                }
            }

            int areaCeil = (int)Math.Ceiling(area);
            int underlayRolls = (int)Math.Ceiling(area / 10.0);
            int sealantCans = (int)Math.Ceiling(area / 5.0);
            int tapeRolls = (int)Math.Ceiling(area / 10.0);
            int eco30Packs = (int)Math.Ceiling(area / 5.0);
            int eco80Packs = (int)Math.Ceiling(area / 5.0);

            // Для каждого вида и качества добавляется лента-скотч soundproject tape (1 моток на 10 кв.м.)
            Deduct("soundproject tape", tapeRolls);

            if (surfaceType == SurfFloor)
            {
                if (workLevel == LvlStd || workLevel == LvlPrem)
                {
                    Deduct("шумощит 18 для пола", areaCeil);
                    Deduct("Лист ГВЛ", areaCeil);
                    Deduct("Подложка демпферная", underlayRolls);
                    Deduct("seal 310", sealantCans);
                    Deduct("Саморезы 30x35", 6 * areaCeil);

                    if (workLevel == LvlPrem)
                    {
                        Deduct("ЭкоАкустик 80", eco80Packs);
                    }
                }
            }
            else if (surfaceType == SurfWall)
            {
                if (workLevel == LvlEco)
                {
                    Deduct("Подложка демпферная", underlayRolls);
                    DeductExact("Панель звукоизоляционная", areaCeil);
                    Deduct("seal 310", sealantCans);
                    Deduct("Лист гипса knauf", areaCeil);
                }
                else
                {
                    Deduct("Профиль CD", areaCeil);
                    Deduct("Профиль ud", areaCeil);
                    Deduct("Дюбеля 3x25", 6 * areaCeil);
                    Deduct("экоакустик 30", eco30Packs);
                    DeductExact("Панель звукоизоляционная", areaCeil);
                    Deduct("Лист гипса knauf", areaCeil);
                    Deduct("Саморезы 30x25", 14 * areaCeil);
                    Deduct("seal 310", sealantCans);

                    if (workLevel == LvlComf || workLevel == LvlPrem)
                    {
                        Deduct("vibro p", 2 * areaCeil);
                    }
                }
            }
            else if (surfaceType == SurfCeiling)
            {
                Deduct("экоакустик 30", eco30Packs);
                Deduct("seal 310", sealantCans);

                if (workLevel == LvlEco)
                {
                    DeductExact("Панель звукоизоляционная", areaCeil);
                    Deduct("protecktor", 2 * areaCeil);
                }
                else if (workLevel == LvlStd)
                {
                    DeductExact("Панель звукоизоляционная", areaCeil);
                    Deduct("vibro pl", 2 * areaCeil);
                }
                else if (workLevel == LvlPrem)
                {
                    Deduct("за 88.50", areaCeil);
                    Deduct("vibro pl", 2 * areaCeil);
                    Deduct("Подложка демпферная", underlayRolls);
                }
            }

            return bomItems;
        }
    }
}

using project.Models;
using System.Collections.Generic;
using System.Linq;

namespace project.Services
{
    public class CartItem
    {
        public Material Material { get; set; } = null!;
        public int Quantity { get; set; }
    }

    public class CartService
    {
        private static CartService? _instance;
        public static CartService Instance => _instance ??= new CartService();

        public List<CartItem> Items { get; private set; } = new List<CartItem>();

        public void Add(Material material, int qty)
        {
            var existing = Items.FirstOrDefault(i => i.Material.Id == material.Id);
            if (existing != null)
            {
                existing.Quantity += qty;
            }
            else
            {
                Items.Add(new CartItem { Material = material, Quantity = qty });
            }
        }

        public void Remove(int materialId)
        {
            var existing = Items.FirstOrDefault(i => i.Material.Id == materialId);
            if (existing != null)
            {
                Items.Remove(existing);
            }
        }

        public void Clear()
        {
            Items.Clear();
        }

        public decimal TotalCost => Items.Sum(i => i.Material.Price * i.Quantity);
    }
}

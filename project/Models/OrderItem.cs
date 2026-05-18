using System;

namespace project.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        public int MaterialId { get; set; }
        public virtual Material Material { get; set; } = null!;
        public int Quantity { get; set; }
        public int Deficit { get; set; } // Сколько не хватило при заказе
        public decimal Price { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace project.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime? ScheduledStartDate { get; set; }
        public DateTime? ScheduledEndDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public decimal TotalCost { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Имя заказа (например, "Заказ из каталога" или "Услуга монтажа (Стена)")
        public string OrderName { get; set; } = string.Empty;

        // Адрес доставки
        public string Address { get; set; } = string.Empty;

        // Новые поля для Исполнителя
        public double Area { get; set; }
        public string SurfaceType { get; set; } = string.Empty; // Wall, Floor, Ceiling

        // Foreign keys
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int? ExecutorId { get; set; }
        public virtual User? Executor { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}

using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace project.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullNameRu { get; set; } = string.Empty;
        public string FullNameEn { get; set; } = string.Empty;
        public decimal TotalEarnings { get; set; }
        public Role Role { get; set; } = Role.Client;

        // Relation to Reviews and Orders
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Order> ExecutedOrders { get; set; } = new List<Order>();

        // Navigation property for "history of viewed materials"
        public virtual ICollection<Material> ViewedMaterials { get; set; } = new List<Material>();

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}

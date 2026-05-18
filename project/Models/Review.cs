using System;

namespace project.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Rating { get; set; } // e.g. 1 to 5
        public bool IsApproved { get; set; } = false; // Requires admin moderation
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign keys
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int MaterialId { get; set; }
        public virtual Material Material { get; set; } = null!;
    }
}

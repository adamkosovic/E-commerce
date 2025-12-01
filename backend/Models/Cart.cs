using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Cart
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }   // 🔴 Viktigt: GUID, inte int

        public List<CartItem> Items { get; set; } = new();

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

using System;
using System.Collections.Generic;

namespace backend.Dtos
{
    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Title { get; set; } = "";
        public decimal Price { get; set; }
        public int Qty { get; set; }
    }

    public class CartDto
    {
        public Guid Id { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public int TotalMinor { get; set; }
    }

    public class AddToCartRequest
    {
        public Guid ProductId { get; set; }
        public int Qty { get; set; }
    }

    public class UpdateCartItemRequest
    {
        public int Qty { get; set; }
    }
}

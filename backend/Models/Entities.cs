// Models/Product.cs (eller Models/Entities.cs om du föredrar)
namespace backend.Models
{
    public class Product
    {
        public Guid Id { get; set; } 
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
    }
}

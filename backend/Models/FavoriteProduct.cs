namespace backend.Models;

public class FavoriteProduct
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string ProductId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
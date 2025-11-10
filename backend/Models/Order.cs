using System; 
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace backend.Models;

public class Order 
{
  public Guid Id { get ; set; }
  public Guid UserId { get ; set; } //koppling till Users 
  public User? User { get ; set; }

  public int SubtotalMinor { get ; set; } //pris i öre (säkrast)
  public int TaxMinor { get ; set; }
  public int ShippingMinor { get ; set; }                               
  public int TotalMinor { get ; set; }
  public string Currency { get ; set; } = "SEK";

  public DateTime CreatedAt { get ; set; } = DateTime.UtcNow;
  public List <OrderItem> Items { get ; set; } = new();
}

public class OrderItem 
{
  public Guid Id { get ; set; }
  public Guid OrderId { get ; set; } //koppling till Orders

  [JsonIgnore]
  public Order Order { get ; set; } = default!;
  
  public Guid ProductId { get ; set; } //koppling till Products                     
  public string Title { get ; set; } = string.Empty;
  public int Qty { get ; set; }
  public int UnitPriceMinor { get ; set; } //pris i öre (säkrast)
}
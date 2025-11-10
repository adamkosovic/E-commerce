using System;
using System.Collections.Generic;

namespace backend.Dtos;

public record OrderItemDto
{ 
  public Guid Id { get; init; }
  public Guid ProductId { get; init; }
  public string Title { get; init; } = string.Empty;
  public int Qty { get; init; }
  public int UnitPriceMinor { get; init; }
}

public record OrderDto 
{
  public Guid Id { get; init; }
  public Guid UserId { get; init; }
  public int SubtotalMinor { get; init; }
  public int TaxMinor { get; init; }
  public int ShippingMinor { get; init; }
  public int TotalMinor { get; init; }
  public string Currency { get; init; } = "SEK";
  public DateTime CreatedAt { get; init; }
  public List<OrderItemDto> Items { get; init; } = new();
}


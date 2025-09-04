namespace Backend.Models;

public enum PropertyType { House, Apartment, TownHouse, Farm, Plot }
public enum ListingStatus { Active, Pending, Sold, Withdrawn }

public class Listing 
{
  public int Id { get; set; }
  public string Title { get; set; } = "";
  public string Description { get; set; } = "";

  public decimal Price { get; set; }           // SEK
  public int LivingAreaSqm { get; set; }       // boyta
  public int? PlotAreaSqm { get; set; }        // tomt
  public float Rooms { get; set; }             // t.ex. 4.5
  public int? Bedrooms { get; set; }
  public int? Bathrooms { get; set; } 
  public int? YearBuilt { get; set; }

  // Plats
  public string StreetAddress { get; set; } = "";
  public string PostalCode { get; set; } = "";
  public string City { get; set; } = "";
  public string? Municipality { get; set; }       // kommun
  public double? Latitude { get; set; }
  public double? Longitude { get; set; }

  public PropertyType Type { get; set; } = PropertyType.House;
  public ListingStatus Status { get; set; } = ListingStatus.Active;

  public DateTime ListedAtUtc { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAtUtc { get; set; }
}
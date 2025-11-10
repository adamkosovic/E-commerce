using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Dtos;

namespace backend.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
  private readonly AppDbContext _db;
  public OrdersController(AppDbContext db) => _db = db;

  private Guid? Getuid()
  {
    var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
    return Guid.TryParse(id, out var g) ? g : (Guid?)null; 
  }

  //POST /orders (Skapa order för inloggade användare)
  [HttpPost]
  [Authorize] // Customer eller Admin
  public async Task <IActionResult>Create([FromBody] CreateOrderRequest req)
  {
    if(req?.Items is null || req.Items.Count == 0)
      return BadRequest(new { message = "Order must contain at least one item." });

    var uid = Getuid();
    if(uid is null) return Unauthorized();

    //Hämta produkter och räkna totals 
    var productIds = req.Items.Select(i => i.ProductId).Distinct().ToList();
    var products = await _db.Products
      .Where(p => productIds.Contains(p.Id))  
      .ToDictionaryAsync(p => p.Id, p => p);

      // Validera alla rader
    foreach(var item in req.Items)
    {
      if(!products.ContainsKey(item.ProductId))
        return BadRequest(new { error = $"Product with ID {item.ProductId} not found." });

      if(item.Qty <= 0)
        return BadRequest(new { error = "Quantity must be greater than zero." });
    }

    //Summera i öre 
    var subtotal = req.Items.Sum (i =>{
      var p = products[i.ProductId];
      var unitMinor = (int)decimal.Round(p.Price * 100m, 0, MidpointRounding.AwayFromZero);
        return unitMinor * i.Qty;
    });

    var tax = (int)Math.Round(subtotal * 0.25); //25% moms 
    var shipping = 0;
    var total = subtotal + tax + shipping;

    var order = new Order 
    {
      Id = Guid.NewGuid(),
      UserId = uid.Value,
      SubtotalMinor = subtotal,
      TaxMinor = tax,
      ShippingMinor = shipping,
      TotalMinor = total,
      Items = req.Items.Select(i =>
      {
        var p = products[i.ProductId];
        var unitMinor = (int)decimal.Round(p.Price * 100m, 0, MidpointRounding.AwayFromZero);
        return new OrderItem 
        {
          Id = Guid.NewGuid(),
          ProductId = p.Id,
          Title = p.Title,
          Qty = i.Qty,
          UnitPriceMinor = unitMinor
        };
      }).ToList()
    };

    _db.Orders.Add(order);
    await _db.SaveChangesAsync();

    //PROJEKTERA -> DTO (ingen loop)
    var dto = await _db.Orders
      .Where(o => o.Id == order.Id)
      .Select(o => new OrderDto 
      {
        Id = o.Id,
        UserId = o.UserId,
        SubtotalMinor = o.SubtotalMinor,
        TaxMinor = o.TaxMinor,
        ShippingMinor = o.ShippingMinor,
        TotalMinor = o.TotalMinor,
        Currency = o.Currency,
        CreatedAt = o.CreatedAt,
        Items = o.Items.Select(oi => new OrderItemDto 
        {
          Id = oi.Id,
          ProductId = oi.ProductId,
          Title = oi.Title,
          Qty = oi.Qty,
          UnitPriceMinor = oi.UnitPriceMinor
        }).ToList()
      })
      .FirstAsync();

    return CreatedAtAction (nameof (GetById), new { id = dto.Id }, dto);
  }


  //GET /orders  (egna ordrar, admin ser alla)
  [HttpGet]
  [Authorize]
  public async Task<IActionResult> ListMine()
  {
    var uid = Getuid();
    if(uid is null) return Unauthorized();

    var isAdmin = User.IsInRole("Admin");

    IQueryable<Order> q = _db.Orders
        .Include(o => o.Items)
        .AsQueryable();

    if(!isAdmin)
      q = q.Where(o => o.UserId == uid.Value);

    var orders = await q
        .OrderByDescending(o => o.CreatedAt)
        .Select(o => new OrderDto 
        {
          Id = o.Id,
          UserId = o.UserId,
          SubtotalMinor = o.SubtotalMinor,
          TaxMinor = o.TaxMinor,
          ShippingMinor = o.ShippingMinor,
          TotalMinor = o.TotalMinor,
          Currency = o.Currency,
          CreatedAt = o.CreatedAt,
          Items = o.Items.Select(oi => new OrderItemDto 
          {
            Id = oi.Id,
            ProductId = oi.ProductId,
            Title = oi.Title,
            Qty = oi.Qty,
            UnitPriceMinor = oi.UnitPriceMinor
          }).ToList()
        })
        .ToListAsync();


    return Ok(orders);
  }

  //GET /orders/{id} (ägare eller admin)
  [HttpGet("{id:guid}")]
  [Authorize]
  public async Task<IActionResult> GetById(Guid id)
  {
    Console.WriteLine($">>> HIT OrdersController.GetById id={id}");

    var uid = Getuid();
    if(uid is null) return Unauthorized();

    var order = await _db.Orders
      .Include(o => o.Items)
      .FirstOrDefaultAsync(o => o.Id == id);

    if(order is null)
      return NotFound(new { message = "Order not found." });

    var isAdmin = User.IsInRole("Admin");
    if(order.UserId != uid.Value && !isAdmin)
      return Forbid();

    var dto = new OrderDto
    {
      Id = order.Id,
      UserId = order.UserId,
      SubtotalMinor = order.SubtotalMinor,
      TaxMinor = order.TaxMinor,
      ShippingMinor = order.ShippingMinor,
      TotalMinor = order.TotalMinor,
      Currency = order.Currency,
      CreatedAt = order.CreatedAt,
      Items = order.Items.Select(oi => new OrderItemDto 
      {
        Id = oi.Id,
        ProductId = oi.ProductId,
        Title = oi.Title,
        Qty = oi.Qty,
        UnitPriceMinor = oi.UnitPriceMinor
      }).ToList()
    };

    return Ok(dto);
  }


}
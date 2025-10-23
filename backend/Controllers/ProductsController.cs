using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using backend.Data;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
  private readonly AppDbContext _db;
  public ProductsController(AppDbContext db) => _db = db;

  //GET /products
  [HttpGet]
  [AllowAnonymous]
  public async Task<IActionResult> GetAll()
  {
    var products = await _db.Products.ToListAsync();
    return Ok(products);
  }

  //GET /products/{id}
  [HttpGet("{id:guid}")]
  [AllowAnonymous]
  public async Task<IActionResult> GetById (Guid id)
  {
    var product = await _db.Products.FindAsync(id);
    if (product == null)
    {
      return NotFound();
    }
    return Ok(product);
  }

  //POST /products
  [HttpPost]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Create([FromBody] Product product)
  {
    product.id = Guid.NewGuid();
    _db.Products.Add(product);
    await _db.SaveChangesAsync();
    return CreatedAtAction(nameof(GetById), new { id = product.id }, product);
  }
  
}
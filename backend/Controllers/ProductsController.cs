using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using backend.Models;
using backend.Data;

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
    try
    {
      var products = await _db.Products.ToListAsync();
      return Ok(products);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error getting products: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");
      return StatusCode(500, new { error = "Failed to retrieve products", message = ex.Message });
    }
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
    product.Id = Guid.NewGuid();
    _db.Products.Add(product);
    await _db.SaveChangesAsync();
    return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
  }

  //PUT /products/{id}
  [HttpPut("{id:guid}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto updatedProduct)
  {
    var product = await _db.Products.FindAsync(id);
    if (product == null)
    {
      return NotFound();
    }

    product.Title = updatedProduct.Title ?? product.Title;
    product.Description = updatedProduct.Description ?? product.Description;
    product.Price = updatedProduct.Price ?? product.Price;
    product.ImageUrl = updatedProduct.ImageUrl ?? product.ImageUrl;

    await _db.SaveChangesAsync();
    return Ok(product);
  }

  //DELETE /products/{id}
  [HttpDelete("{id:guid}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Delete(Guid id)
  {
    var product = await _db.Products.FindAsync(id);
    if (product == null)
    {
      return NotFound();
    }

    _db.Products.Remove(product);
    await _db.SaveChangesAsync();
    return NoContent();
  }
  
}
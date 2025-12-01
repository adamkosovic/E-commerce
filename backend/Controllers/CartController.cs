using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("cart")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CartController(AppDbContext db)
        {
            _db = db;
        }

        private Guid? Getuid()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var g) ? g : (Guid?)null;
        }

        // GET /cart
        [HttpGet]
        public async Task<ActionResult<CartDto>> GetMyCart()
        {
            var uid = Getuid();
            if (uid is null) return Unauthorized();

            var cart = await _db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == uid.Value);   // Guid == Guid

            if (cart is null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = uid.Value
                };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

            var dto = new CartDto
            {
                Id = cart.Id,
                Items = cart.Items.Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,                        // Guid
                    Title = i.Product?.Title ?? string.Empty,
                    Price = i.Product?.Price ?? 0m,
                    Qty = i.Qty
                }).ToList()
            };

            dto.TotalMinor = dto.Items.Sum(i =>
            {
                var unitMinor = (int)decimal.Round(i.Price * 100m, 0, MidpointRounding.AwayFromZero);
                return unitMinor * i.Qty;
            });

            return Ok(dto);
        }

        // POST /cart/items
        [HttpPost("items")]
        public async Task<IActionResult> AddItem([FromBody] AddToCartRequest req)
        {
            if (req.Qty <= 0)
                return BadRequest(new { message = "Quantity must be greater than zero." });

            var uid = Getuid();
            if (uid is null) return Unauthorized();

            // 1) Kolla att produkten finns
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == req.ProductId);
            if (product is null)
                return BadRequest(new { message = "Product not found." });

            // 2) Hämta eller skapa cart för användaren
            var cart = await _db.Carts
                .FirstOrDefaultAsync(c => c.UserId == uid.Value);

            if (cart is null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = uid.Value,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Carts.Add(cart);
                // spara direkt så vi är säkra på att cart finns i DB
                await _db.SaveChangesAsync();
            }

            // 3) Kolla om det redan finns en rad för den produkten i den här carten
            var existing = await _db.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == req.ProductId);

            if (existing is null)
            {
                var newItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,          // sätt CartId explicit
                    ProductId = req.ProductId,
                    Qty = req.Qty
                };

                _db.CartItems.Add(newItem);
            }
            else
            {
                existing.Qty += req.Qty;
                _db.CartItems.Update(existing); // tydligt att vi uppdaterar just den raden
            }

            // 4) Uppdatera timestamp på carten
            cart.UpdatedAt = DateTime.UtcNow;
            _db.Carts.Update(cart);

            // 5) Spara ändringarna (cart + items) i ett svep
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // PUT /cart/items/{productId}
        [HttpPut("items/{productId:guid}")]
        public async Task<IActionResult> UpdateItem(Guid productId, [FromBody] UpdateCartItemRequest req)
        {
            var uid = Getuid();
            if (uid is null) return Unauthorized();

            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == uid.Value);   // Guid == Guid

            if (cart is null)
                return NotFound(new { message = "Cart not found." });

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId); // Guid == Guid
            if (item is null)
                return NotFound(new { message = "Item not found in cart." });

            if (req.Qty <= 0)
            {
                cart.Items.Remove(item);
            }
            else
            {
                item.Qty = req.Qty;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /cart/items/{productId}
        [HttpDelete("items/{productId:guid}")]
        public async Task<IActionResult> RemoveItem(Guid productId)
        {
            var uid = Getuid();
            if (uid is null) return Unauthorized();

            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == uid.Value);   // Guid == Guid

            if (cart is null)
                return NotFound(new { message = "Cart not found." });

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId); // Guid == Guid
            if (item is null)
                return NotFound(new { message = "Item not found in cart." });

            cart.Items.Remove(item);
            cart.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // DELETE /cart
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var uid = Getuid();
            if (uid is null) return Unauthorized();

            var cart = await _db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == uid.Value);   // Guid == Guid

            if (cart is null) return NoContent();

            _db.CartItems.RemoveRange(cart.Items);
            cart.Items.Clear();
            cart.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}




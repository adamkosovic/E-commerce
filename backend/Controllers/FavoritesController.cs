using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("favorites")]
    [Authorize] // bara för inloggade
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public FavoritesController(AppDbContext db)
        {
            _db = db;
        }

        // Samma mönster som i CartController
        private Guid? Getuid()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(id, out var g) ? g : (Guid?)null;
        }

        // GET /favorites  → returnerar lista med productId:n
        [HttpGet]
        public async Task<ActionResult<List<string>>> GetFavorites()
        {
            var uid = Getuid();
            if (uid is null)
            {
                return Unauthorized();
            }

            var productIds = await _db.FavoriteProducts
                .Where(f => f.UserId == uid.Value)
                .Select(f => f.ProductId)
                .ToListAsync();

            return Ok(productIds);
        }

        // POST /favorites/{productId} → gilla produkt
        [HttpPost("{productId}")]
        public async Task<IActionResult> AddFavorite(string productId)
        {
            var uid = Getuid();
            if (uid is null)
            {
                return Unauthorized();
            }

            var exists = await _db.FavoriteProducts
                .AnyAsync(f => f.UserId == uid.Value && f.ProductId == productId);

            if (!exists)
            {
                _db.FavoriteProducts.Add(new FavoriteProduct
                {
                    UserId = uid.Value,
                    ProductId = productId
                });

                await _db.SaveChangesAsync();
            }

            return Ok();
        }

        // DELETE /favorites/{productId} → ta bort gillning
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFavorite(string productId)
        {
            var uid = Getuid();
            if (uid is null)
            {
                return Unauthorized();
            }

            var fav = await _db.FavoriteProducts
                .FirstOrDefaultAsync(f => f.UserId == uid.Value && f.ProductId == productId);

            if (fav != null)
            {
                _db.FavoriteProducts.Remove(fav);
                await _db.SaveChangesAsync();
            }

            return Ok();
        }

        // POST /favorites/merge  → valfritt: merge:a guest-favoriter upp i kontot vid login
        [HttpPost("merge")]
        public async Task<IActionResult> MergeFavorites([FromBody] List<string> productIds)
        {
            var uid = Getuid();
            if (uid is null)
            {
                return Unauthorized();
            }

            var existingIds = await _db.FavoriteProducts
                .Where(f => f.UserId == uid.Value)
                .Select(f => f.ProductId)
                .ToListAsync();

            var newFavorites = productIds
                .Except(existingIds)
                .Select(pid => new FavoriteProduct
                {
                    UserId = uid.Value,
                    ProductId = pid
                });

            _db.FavoriteProducts.AddRange(newFavorites);
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}

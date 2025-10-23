using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using backend.Data;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly ITokenService _tokens;

    public AuthController(
        AppDbContext db,
        IPasswordHasher<User> hasher,
        ITokenService tokens)
    {
        _db = db;
        _hasher = hasher;
        _tokens = tokens;
    }


    //POST /auth/register (öppen)
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {

        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "Email and password are required" });

        var normalized = req.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users.AnyAsync(u => u.NormalizedEmail == normalized);
        if (exists) return Conflict(new { error = "Email already registered" });

        // Skapa ny användare
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = req.Email.Trim(),
            NormalizedEmail = normalized,
            Role = "Customer"
        };
        user.PasswordHash = _hasher.HashPassword(user, req.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _tokens.CreateToken(user.Id.ToString(), user.Email, new[] { user.Role });
        return Ok(new { token, role = user.Role });
    }


    // POST /auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "Email and password are required" });

        // Hårdkodad admin (valfritt att ha kvar)
        if (req.Email.Equals("admin@shop.se", StringComparison.OrdinalIgnoreCase) && req.Password == "Admin!234")
        {
            var token = _tokens.CreateToken(Guid.NewGuid().ToString(), req.Email, new[] { "Admin" });
            return Ok(new { token, role = "Admin" });
        }

        var normalized = req.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalized);
        if (user is null) return Unauthorized(new { error = "Invalid email or password" });

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (verify == PasswordVerificationResult.Success || verify == PasswordVerificationResult.SuccessRehashNeeded)
        {
            if (verify == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _hasher.HashPassword(user, req.Password);
                await _db.SaveChangesAsync();
            }

            var token = _tokens.CreateToken(user.Id.ToString(), user.Email, new[] { user.Role });
            return Ok(new { token, role = user.Role });
        }

        return Unauthorized(new { error = "Fel e-post eller lösenord" });
    }
}


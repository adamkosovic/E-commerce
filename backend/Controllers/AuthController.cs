using Microsoft.AspNetCore.Mvc;
using backend.Services;

namespace backend.Controllers;

public record LoginRequest(string Email, string Password);

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokens;
    public AuthController(ITokenService tokens) => _tokens = tokens;

    // DEMO: hårdkodad inlogg (byt mot riktig user-hantering senare)
    // POST /auth/login
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        // Hårdkodad användare: admin@shop.se / Admin!234
        if (req.Email.Equals("admin@shop.se", StringComparison.OrdinalIgnoreCase)
            && req.Password == "Admin!234")
        {
            var jwt = _tokens.CreateToken(
                userId: Guid.NewGuid().ToString(),
                email: req.Email,
                roles: new[] { "Admin" }   // <- viktig del: rollen "Admin"
            );
            return Ok(new { token = jwt });
        }

        return Unauthorized(new { error = "Fel e-post eller lösenord" });
    }
}


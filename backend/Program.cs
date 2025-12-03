using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using backend.Models;
using System.Text;
using backend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;



var builder = WebApplication.CreateBuilder(args);

// Railway sets PORT environment variable - configure to listen on all interfaces
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var httpPorts = Environment.GetEnvironmentVariable("HTTP_PORTS");
Console.WriteLine($"PORT: {port}, HTTP_PORTS: {httpPorts}");

// Use * instead of 0.0.0.0 for better compatibility with Railway
builder.WebHost.UseUrls($"http://*:{port}");
Console.WriteLine($"Configured to listen on http://*:{port}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Backend API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header. Skriv: Bearer {token}",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// CORS - Tillåt Angular dev-servern och Netlify
// Using explicit origins for maximum compatibility
var allowedOrigins = new List<string>
{
    "http://localhost:4200",
    "https://localhost:4200",
    "https://mellow-griffin-feb028.netlify.app"
};

// Add additional origins from configuration (semicolon-separated)
var additionalOrigins = builder.Configuration["CORS:AllowedOrigins"];
if (!string.IsNullOrEmpty(additionalOrigins))
{
    allowedOrigins.AddRange(additionalOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}

builder.Services.AddCors(o =>
{
    // TEMPORARY: Allow all origins for testing - will restrict after confirming CORS works
    o.AddPolicy("NgDev", p => p
        .AllowAnyOrigin()  // Temporarily allow all origins
        .AllowAnyHeader()
        .AllowAnyMethod());
    // No AllowCredentials() - JWT tokens are sent in Authorization header, not cookies
    // Note: AllowAnyOrigin() cannot be used with AllowCredentials()
});

var jwt = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Allow OPTIONS requests to bypass authorization (needed for CORS preflight)
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

var app = builder.Build();

// CORS must be the VERY FIRST middleware - before anything else
app.UseCors("NgDev");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

// Only use HTTPS redirection in development (Railway handles HTTPS in production)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Skip authentication/authorization for health checks
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/health"), appBuilder =>
{
    appBuilder.UseAuthentication();
    appBuilder.UseAuthorization();
});


// 🚧 Tillfälligt bortkommenterat under utveckling.
// Angular körs separat via ng serve (port 4200) och proxy till API.
// Avkommentera dessa rader när appen ska byggas för produktion
// och Angular-dist-filerna ska servas från wwwroot:

// app.UseDefaultFiles();     
// app.UseStaticFiles();
// app.MapFallbackToFile("index.html");

// Map health endpoint - must be accessible without any dependencies
app.MapGet("/health", () =>
{
    Console.WriteLine("Health check called");
    return Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow });
})
.WithName("health")
.AllowAnonymous();

// Also add root endpoint for testing
app.MapGet("/", () => Results.Ok(new { message = "API is running", timestamp = DateTime.UtcNow }))
    .AllowAnonymous();

app.MapControllers();

Console.WriteLine("Application starting...");
Console.WriteLine("Health endpoint available at: /health");
app.Run();

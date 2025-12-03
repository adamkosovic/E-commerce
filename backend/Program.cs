using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using backend.Models;
using System.Text;
using backend.Services;
using System.Security.Claims;



var builder = WebApplication.CreateBuilder(args);

// Railway sets PORT and HTTP_PORTS environment variables
// Configure Kestrel to listen on the PORT that Railway provides
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var httpPorts = Environment.GetEnvironmentVariable("HTTP_PORTS");
Console.WriteLine($"PORT: {port}, HTTP_PORTS: {httpPorts}");

// Configure Kestrel to listen on the correct port and interface
// Railway uses IPv4, so bind to IPv4 explicitly (0.0.0.0)
builder.WebHost.ConfigureKestrel(options =>
{
    // Listen on IPv4 (0.0.0.0) - Railway uses IPv4
    options.Listen(System.Net.IPAddress.Any, int.Parse(port));
    Console.WriteLine($"Kestrel configured to listen on 0.0.0.0:{port} (IPv4)");
});

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

// Configure DbContext with retry logic and lazy connection
// Railway provides DATABASE_URL, but we can also use ConnectionStrings:Default from appsettings.json
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("Default");

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("WARNING: No database connection string found. Database operations will fail.");
}
else
{
    Console.WriteLine($"Database connection string configured (length: {connectionString.Length})");
}

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connectionString ?? "Host=localhost;Port=5432;Database=temp",
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)));

// CORS Configuration - Simple and reliable
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p
        .AllowAnyOrigin()  // Allow all origins (no credentials needed for JWT in headers)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// Configure JWT authentication
var jwt = builder.Configuration.GetSection("Jwt");
var jwtKey = jwt["Key"] ?? "default-key-for-development-only-change-in-production";
var jwtIssuer = jwt["Issuer"] ?? "backend";
var jwtAudience = jwt["Audience"] ?? "backend";

Console.WriteLine($"JWT Configuration - Issuer: {jwtIssuer}, Audience: {jwtAudience}");

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

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
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
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

// Enable routing first
app.UseRouting();

// CORS middleware - MUST be before UseAuthentication/UseAuthorization
app.UseCors();

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

// Skip authentication/authorization for OPTIONS (CORS preflight) and health checks
app.UseWhen(context =>
    !context.Request.Path.StartsWithSegments("/health") &&
    context.Request.Method != "OPTIONS",
    appBuilder =>
{
    appBuilder.UseAuthentication();
    appBuilder.UseAuthorization();
});

// Map endpoints (UseEndpoints is implicit in .NET 6+ with MapControllers/MapGet)


// 🚧 Tillfälligt bortkommenterat under utveckling.
// Angular körs separat via ng serve (port 4200) och proxy till API.
// Avkommentera dessa rader när appen ska byggas för produktion
// och Angular-dist-filerna ska servas från wwwroot:

// app.UseDefaultFiles();     
// app.UseStaticFiles();
// app.MapFallbackToFile("index.html");

// OPTIONS requests are handled by CORS middleware automatically

// Map health endpoint - must be accessible without any dependencies
// This endpoint should NEVER fail, even if database is down
// Railway uses this for health checks
app.MapGet("/health", () =>
{
    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Health check called - returning OK");
    return Results.Ok(new
    {
        status = "ok",
        timestamp = DateTime.UtcNow,
        uptime = "healthy"
    });
})
.WithName("health")
.AllowAnonymous();

// Also map /healthz (common health check path)
app.MapGet("/healthz", () =>
{
    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Healthz check called - returning OK");
    return Results.Ok(new { status = "ok" });
})
.AllowAnonymous();

// Also add root endpoint for testing
app.MapGet("/", (HttpContext context) =>
{
    var corsHeader = context.Response.Headers["Access-Control-Allow-Origin"].ToString();
    return Results.Ok(new
    {
        message = "API is running",
        timestamp = DateTime.UtcNow,
        corsHeader = corsHeader,
        hasCorsHeader = !string.IsNullOrEmpty(corsHeader)
    });
})
.AllowAnonymous();

app.MapControllers();

// Run database migrations on startup (non-blocking)
_ = Task.Run(async () =>
{
    try
    {
        await Task.Delay(1000); // Wait 1 second for app to fully start
        Console.WriteLine("Checking database connection and running migrations...");
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("Database migrations completed successfully.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"WARNING: Database migration failed: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        Console.WriteLine("Application will continue, but database operations may fail.");
    }
});

Console.WriteLine("========================================");
Console.WriteLine("Application starting...");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Health endpoint: /health");
Console.WriteLine($"Root endpoint: /");
Console.WriteLine($"Listening on: http://0.0.0.0:{port}");
Console.WriteLine("========================================");

app.Run();

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

// Railway sets PORT and HTTP_PORTS environment variables
// .NET 9.0 automatically uses HTTP_PORTS if set, so we don't need to configure it manually
// This avoids the "Overriding HTTP_PORTS" warning and lets Railway handle port binding
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var httpPorts = Environment.GetEnvironmentVariable("HTTP_PORTS");
Console.WriteLine($"PORT: {port}, HTTP_PORTS: {httpPorts}");

// Only configure URL if HTTP_PORTS is not set (for local development)
if (string.IsNullOrEmpty(httpPorts))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    Console.WriteLine($"Configured to listen on http://0.0.0.0:{port} (HTTP_PORTS not set)");
}
else
{
    Console.WriteLine($"Using Railway's HTTP_PORTS configuration: {httpPorts}");
    // Let .NET 9.0 automatically use HTTP_PORTS - don't override it
}

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

// Enable routing first (required for endpoint routing)
app.UseRouting();

// CORS must be called after UseRouting but before UseAuthentication/UseAuthorization
// This is critical for CORS preflight (OPTIONS) requests
app.UseCors("NgDev");

// Ensure CORS headers are added even on errors
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Exception: {ex.Message}");
        // Ensure CORS headers are still added even on error
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = 500;
        }
        throw;
    }
});

// Add CORS headers manually BEFORE the request is processed (ensures they're always present)
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();
    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {context.Request.Method} {context.Request.Path} from Origin: {origin}");

    // ALWAYS add CORS headers manually as a fallback (before any processing)
    if (!string.IsNullOrEmpty(origin))
    {
        context.Response.Headers["Access-Control-Allow-Origin"] = "*";
        context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS, PATCH";
        context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With";
        context.Response.Headers["Access-Control-Allow-Credentials"] = "false";
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Added CORS headers manually for origin: {origin}");
    }

    if (context.Request.Method == "OPTIONS")
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] OPTIONS preflight - Access-Control-Request-Method: {context.Request.Headers["Access-Control-Request-Method"]}, Access-Control-Request-Headers: {context.Request.Headers["Access-Control-Request-Headers"]}");
        // Return immediately for OPTIONS requests
        context.Response.StatusCode = 200;
        return;
    }

    await next();

    // Log response headers after processing
    var corsOrigin = context.Response.Headers["Access-Control-Allow-Origin"].ToString();
    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Response Status: {context.Response.StatusCode}, CORS Origin: {corsOrigin}");
});

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

// Handle OPTIONS requests explicitly for CORS preflight - must be before other routes
app.MapMethods("/{*path}", new[] { "OPTIONS" }, (HttpContext context) =>
{
    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] OPTIONS preflight request for {context.Request.Path}");
    // CORS middleware should handle headers, but ensure we return 200
    return Results.Ok();
})
.AllowAnonymous();

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
app.MapGet("/", () => Results.Ok(new { message = "API is running", timestamp = DateTime.UtcNow }))
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

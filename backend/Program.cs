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
using backend.Filters;



var builder = WebApplication.CreateBuilder(args);

// Railway sets HTTP_PORTS automatically - let .NET 9.0 use it
// NEVER override HTTP_PORTS to avoid the "Overriding HTTP_PORTS" warning
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var httpPorts = Environment.GetEnvironmentVariable("HTTP_PORTS");
Console.WriteLine($"PORT: {port}, HTTP_PORTS: {httpPorts}");

// .NET 9.0 automatically uses HTTP_PORTS if set
// DO NOT call UseUrls when HTTP_PORTS is set (Railway sets it)
// This prevents the "Overriding HTTP_PORTS" warning and container restarts
if (!string.IsNullOrEmpty(httpPorts))
{
    Console.WriteLine($"Railway HTTP_PORTS detected: {httpPorts} - letting .NET handle it automatically");
    // Don't call UseUrls - let .NET use HTTP_PORTS
}
else
{
    // Only for local development when HTTP_PORTS is not set
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    Console.WriteLine($"Configured to listen on http://0.0.0.0:{port} (local development, HTTP_PORTS not set)");
}

builder.Services.AddControllers(options =>
{
    // Add global filter to ensure CORS headers are always present
    options.Filters.Add(new CorsHeaderFilter());
});
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
    // Use explicit origins for better compatibility
    o.AddPolicy("NgDev", p => p
        .WithOrigins(
            "http://localhost:4200",
            "https://localhost:4200",
            "https://mellow-griffin-feb028.netlify.app",
            "https://*.netlify.app"  // Allow all Netlify preview deployments
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowedToAllowWildcardSubdomains());
    // No AllowCredentials() - JWT tokens are sent in Authorization header, not cookies
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

// CORS middleware
app.UseCors("NgDev");

// Add CORS headers IMMEDIATELY - before any processing
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();
    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {context.Request.Method} {context.Request.Path} from Origin: {origin}");

    // Determine which origin to allow (use actual origin if it matches, otherwise use *)
    var allowedOrigin = "*";
    if (!string.IsNullOrEmpty(origin) &&
        (origin.Contains("netlify.app") || origin.Contains("localhost:4200")))
    {
        allowedOrigin = origin;
    }

    // Add CORS headers IMMEDIATELY (not just in OnStarting)
    context.Response.Headers["Access-Control-Allow-Origin"] = allowedOrigin;
    context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS, PATCH";
    context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With";
    context.Response.Headers["Access-Control-Allow-Credentials"] = "false";
    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] CORS headers added immediately - Origin: {allowedOrigin}");

    // Also use OnStarting as backup
    context.Response.OnStarting(() =>
    {
        if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = allowedOrigin;
            context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS, PATCH";
            context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With";
            context.Response.Headers["Access-Control-Allow-Credentials"] = "false";
            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] CORS headers added via OnStarting (backup) - Origin: {allowedOrigin}");
        }
        return Task.CompletedTask;
    });

    // Handle OPTIONS preflight immediately
    if (context.Request.Method == "OPTIONS")
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] OPTIONS preflight - returning 200");
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync("");
        return;
    }

    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Exception: {ex.Message}");
        // CORS headers already added, so error response will have them
        throw;
    }

    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Response Status: {context.Response.StatusCode}");
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

// FINAL middleware - add CORS headers at the very end, after all routes
app.Use(async (context, next) =>
{
    await next();

    // Ensure CORS headers are ALWAYS present, even if they weren't added earlier
    if (!context.Response.Headers.ContainsKey("Access-Control-Allow-Origin"))
    {
        context.Response.Headers["Access-Control-Allow-Origin"] = "*";
        context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS, PATCH";
        context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With";
        context.Response.Headers["Access-Control-Allow-Credentials"] = "false";
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] FINAL: Added CORS headers after all processing");
    }
});

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

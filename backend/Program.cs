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

// Railway sets PORT environment variable (e.g., "8080")
// We need to explicitly set HTTP_PORTS so .NET uses the correct port
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var httpPorts = Environment.GetEnvironmentVariable("HTTP_PORTS");

// Clear ASPNETCORE_URLS if it's set incorrectly
var aspnetcoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
if (!string.IsNullOrEmpty(aspnetcoreUrls))
{
    Console.WriteLine($"ASPNETCORE_URLS was set to: {aspnetcoreUrls}");
    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", null);
    Console.WriteLine("ASPNETCORE_URLS cleared");
}

// Explicitly set HTTP_PORTS to ensure .NET uses the correct port
if (string.IsNullOrEmpty(httpPorts))
{
    Environment.SetEnvironmentVariable("HTTP_PORTS", port);
    Console.WriteLine($"HTTP_PORTS set to: {port}");
}
else
{
    Console.WriteLine($"HTTP_PORTS already set to: {httpPorts}");
}

Console.WriteLine($"PORT: {port}, HTTP_PORTS: {Environment.GetEnvironmentVariable("HTTP_PORTS")}");

// Explicitly configure Kestrel to listen on IPv4 and the correct port
builder.WebHost.ConfigureKestrel(options =>
{
    if (int.TryParse(port, out int portNumber))
    {
        options.Listen(System.Net.IPAddress.Any, portNumber);
        Console.WriteLine($"Kestrel explicitly configured to listen on 0.0.0.0:{portNumber} (IPv4)");
    }
    else
    {
        Console.WriteLine($"ERROR: Invalid port value '{port}'. Using default configuration.");
    }
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Ensure API controllers work correctly
        options.SuppressModelStateInvalidFilter = true;
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

// CORS Configuration - Allow Netlify and localhost explicitly
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p
        .WithOrigins(
            "https://mellow-griffin-feb028.netlify.app",
            "http://localhost:4200",
            "https://localhost:4200"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowedToAllowWildcardSubdomains());
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
// This adds CORS headers to all responses
// Use default policy with explicit origins for security
app.UseCors(); // Uses the default policy with explicit Netlify origin

// Log all incoming requests for debugging
app.Use(async (context, next) =>
{
    try
    {
        var origin = context.Request.Headers["Origin"].ToString();
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {context.Request.Method} {context.Request.Path} from Origin: {origin}");
        await next();
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Response: {context.Response.StatusCode} for {context.Request.Method} {context.Request.Path}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] EXCEPTION in middleware: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw; // Re-throw to let error handling middleware handle it
    }
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

// Authentication and Authorization middleware
// Note: Controllers with [AllowAnonymous] will bypass authorization
// Authentication and Authorization middleware
// Note: Controllers with [AllowAnonymous] will bypass authorization
// OPTIONS requests (CORS preflight) are handled by CORS middleware and don't need auth
app.UseAuthentication();
app.UseAuthorization();

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

// Favicon endpoint - browsers automatically request this
app.MapGet("/favicon.ico", () =>
{
    return Results.NoContent(); // Return 204 No Content
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

// Simple test endpoint that doesn't require database
app.MapGet("/api-test", () =>
{
    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] /api-test called");
    return Results.Ok(new { message = "API is responding", timestamp = DateTime.UtcNow });
})
.AllowAnonymous();

// Database connectivity test endpoint
app.MapGet("/db-test", async (AppDbContext db) =>
{
    try
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] /db-test called - testing database connection");

        // Test connection
        var canConnect = await db.Database.CanConnectAsync();
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Database can connect: {canConnect}");

        if (!canConnect)
        {
            return Results.Ok(new
            {
                connected = false,
                error = "Cannot connect to database",
                timestamp = DateTime.UtcNow
            });
        }

        // Check if tables exist
        var tablesExist = new Dictionary<string, bool>();
        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_type = 'BASE TABLE'
                ORDER BY table_name;
            ";

            using var reader = await command.ExecuteReaderAsync();
            var existingTables = new List<string>();
            while (await reader.ReadAsync())
            {
                existingTables.Add(reader.GetString(0));
            }

            // Check which tables we expect exist
            tablesExist["Products"] = existingTables.Contains("Products");
            tablesExist["Users"] = existingTables.Contains("Users");
            tablesExist["Orders"] = existingTables.Contains("Orders");
            tablesExist["OrderItems"] = existingTables.Contains("OrderItems");
            tablesExist["Carts"] = existingTables.Contains("Carts");
            tablesExist["CartItems"] = existingTables.Contains("CartItems");
            tablesExist["FavoriteProducts"] = existingTables.Contains("FavoriteProducts");

            Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Existing tables: {string.Join(", ", existingTables)}");

            // Try to query products
            int productCount = -1;
            string? productError = null;
            try
            {
                productCount = await db.Products.CountAsync();
            }
            catch (Exception ex)
            {
                productError = ex.Message;
                Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Error querying Products: {ex.Message}");
            }

            return Results.Ok(new
            {
                connected = true,
                tables = tablesExist,
                existingTables = existingTables,
                productCount = productCount,
                productError = productError ?? "none",
                timestamp = DateTime.UtcNow
            });
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Database connection test failed: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.AllowAnonymous();

app.MapControllers();

// Test endpoint to verify routing works (with database)
app.MapGet("/test-products", async (AppDbContext db) =>
{
    try
    {
        var products = await db.Products.ToListAsync();
        return Results.Ok(products);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in test endpoint: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.AllowAnonymous();

// Run database migrations on startup (non-blocking)
_ = Task.Run(async () =>
{
    try
    {
        await Task.Delay(2000); // Wait 2 seconds for app to fully start
        Console.WriteLine("Checking database connection and running migrations...");
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Check if database can connect first
            var canConnect = await dbContext.Database.CanConnectAsync();
            Console.WriteLine($"Database can connect: {canConnect}");

            if (canConnect)
            {
                // Get pending migrations
                var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
                var pendingList = pendingMigrations.ToList();
                Console.WriteLine($"Pending migrations: {pendingList.Count}");
                if (pendingList.Any())
                {
                    Console.WriteLine($"Migrations to apply: {string.Join(", ", pendingList)}");
                }

                // Apply migrations
                await dbContext.Database.MigrateAsync();
                Console.WriteLine("Database migrations completed successfully.");
            }
            else
            {
                Console.WriteLine("WARNING: Cannot connect to database. Migrations skipped.");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"WARNING: Database migration failed: {ex.Message}");
        Console.WriteLine($"Exception type: {ex.GetType().Name}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
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
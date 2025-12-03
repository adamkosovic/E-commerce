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
// HTTP_PORTS might be empty, so use PORT as fallback
var httpPorts = Environment.GetEnvironmentVariable("HTTP_PORTS");
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
var listenPort = !string.IsNullOrEmpty(httpPorts) ? httpPorts : port;

Console.WriteLine($"HTTP_PORTS: {httpPorts}, PORT: {port}, Using port: {listenPort}");

// Validate and parse the port number before configuring Kestrel
if (!int.TryParse(listenPort, out int portNumber) || portNumber < 1 || portNumber > 65535)
{
    Console.WriteLine($"ERROR: Invalid port value '{listenPort}'. Must be a number between 1 and 65535. Using default port 8080.");
    portNumber = 8080;
    listenPort = "8080";
}

// Configure Kestrel to listen on the correct port and IPv4 interface
// Railway uses IPv4, so bind to 0.0.0.0 (not [::] which is IPv6)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(System.Net.IPAddress.Any, portNumber);
    Console.WriteLine($"Kestrel configured to listen on 0.0.0.0:{portNumber} (IPv4)");
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
app.UseCors();

// Log all incoming requests for debugging
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();
    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {context.Request.Method} {context.Request.Path} from Origin: {origin}");
    await next();
    Console.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Response: {context.Response.StatusCode} for {context.Request.Method} {context.Request.Path}");
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

// Skip authentication/authorization for OPTIONS (CORS preflight), health checks, and root
// Note: Controllers with [AllowAnonymous] will still be accessible
app.UseWhen(context =>
    !context.Request.Path.StartsWithSegments("/health") &&
    !context.Request.Path.StartsWithSegments("/healthz") &&
    context.Request.Path != "/" &&
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
Console.WriteLine($"Listening on: http://0.0.0.0:{listenPort}");
Console.WriteLine("========================================");

app.Run();
using SmartInventory.API.Extensions;
using SmartInventory.API.Middleware;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

LoadDotEnv(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));
LoadDotEnv(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

// Map flat environment variable names to .NET configuration paths
MapEnvironmentVariables(builder.Configuration);

// Validate required configuration on startup
ValidateConfiguration(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddScoped<IDbSeeder, DbSeeder>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

try
{
    using var serviceScope = app.Services.CreateScope();
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Applying database migrations...");
    if (dbContext.Database.IsRelational())
    {
        await dbContext.Database.MigrateAsync();
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
    }

    var seeder = serviceScope.ServiceProvider.GetRequiredService<IDbSeeder>();
    logger.LogInformation("Seeding database...");
    await seeder.SeedAsync();

    logger.LogInformation("Database migration and seeding completed successfully.");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogWarning(ex, "Could not complete database migration/seeding on startup. API will proceed.");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void LoadDotEnv(string filePath)
{
    if (!File.Exists(filePath)) return;
    foreach (var line in File.ReadAllLines(filePath))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#")) continue;
        var parts = trimmed.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var val = parts[1].Trim().Trim('"', '\'');
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, val);
            }
        }
    }
}

static void MapEnvironmentVariables(ConfigurationManager configuration)
{
    configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        { "ConnectionStrings:DefaultConnection", EnvOrExisting(configuration.GetConnectionString("DefaultConnection"), "DB_CONNECTION_STRING") },
        { "JwtSettings:SecretKey", EnvOrExisting(configuration["JwtSettings:SecretKey"], "JWT_SECRET") },
        { "JwtSettings:Issuer", EnvOrExisting(configuration["JwtSettings:Issuer"], "JWT_ISSUER") },
        { "JwtSettings:Audience", EnvOrExisting(configuration["JwtSettings:Audience"], "JWT_AUDIENCE") },
        { "JwtSettings:ExpiryMinutes", EnvOrExisting(configuration["JwtSettings:ExpiryMinutes"], "JWT_EXPIRY_MINUTES") },
        { "SeedSettings:AdminPassword", EnvOrExisting(configuration["SeedSettings:AdminPassword"], "SEED_ADMIN_PASSWORD") },
        { "SeedSettings:DefaultUserPassword", EnvOrExisting(configuration["SeedSettings:DefaultUserPassword"], "SEED_DEFAULT_USER_PASSWORD") }
    });
}

static string? EnvOrExisting(string? existingValue, string envVarName)
{
    if (!string.IsNullOrWhiteSpace(existingValue))
        return existingValue;
    return Environment.GetEnvironmentVariable(envVarName);
}

static void ValidateConfiguration(IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        configuration["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=SmartInventoryDB;Trusted_Connection=True;TrustServerCertificate=True;";
    }

    var jwtSecret = configuration["JwtSettings:SecretKey"];
    if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    {
        configuration["JwtSettings:SecretKey"] = "smart-inventory-super-secret-key-32-chars-minimum!";
    }

    var seedAdminPassword = configuration["SeedSettings:AdminPassword"];
    if (string.IsNullOrWhiteSpace(seedAdminPassword))
    {
        configuration["SeedSettings:AdminPassword"] = "Admin@123456";
    }

    var seedUserPassword = configuration["SeedSettings:DefaultUserPassword"];
    if (string.IsNullOrWhiteSpace(seedUserPassword))
    {
        configuration["SeedSettings:DefaultUserPassword"] = "User@123456";
    }
}
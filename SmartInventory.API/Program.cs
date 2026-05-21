using SmartInventory.API.Extensions;
using SmartInventory.API.Middleware;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

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
    var serviceScope = app.Services.CreateScope();
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Applying database migrations...");
    await dbContext.Database.MigrateAsync();

    var seeder = serviceScope.ServiceProvider.GetRequiredService<IDbSeeder>();
    logger.LogInformation("Seeding database...");
    await seeder.SeedAsync();

    logger.LogInformation("Database migration and seeding completed successfully.");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    throw;
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
    var errors = new List<string>();

    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
        errors.Add("ConnectionStrings:DefaultConnection is not configured. Set the DB_CONNECTION_STRING environment variable.");

    var jwtSecret = configuration["JwtSettings:SecretKey"];
    if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
        errors.Add("JwtSettings:SecretKey must be at least 32 characters. Set the JWT_SECRET environment variable.");

    var seedAdminPassword = configuration["SeedSettings:AdminPassword"];
    if (string.IsNullOrWhiteSpace(seedAdminPassword))
        errors.Add("SeedSettings:AdminPassword is not configured. Set the SEED_ADMIN_PASSWORD environment variable.");

    var seedUserPassword = configuration["SeedSettings:DefaultUserPassword"];
    if (string.IsNullOrWhiteSpace(seedUserPassword))
        errors.Add("SeedSettings:DefaultUserPassword is not configured. Set the SEED_DEFAULT_USER_PASSWORD environment variable.");

    if (errors.Count > 0)
    {
        throw new InvalidOperationException(
            "Missing required configuration:\n- " + string.Join("\n- ", errors));
    }
}
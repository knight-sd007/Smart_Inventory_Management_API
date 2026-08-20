namespace SmartInventory.API.Extensions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartInventory.Application.Mappings;
using SmartInventory.Application.Services;
using SmartInventory.Application.Interfaces;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.Repositories;
using SmartInventory.Infrastructure.Services;
using System.Text;

public static class ServiceExtensions
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IRepository<Domain.Entities.BaseEntity>, Repository<Domain.Entities.BaseEntity>>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }

    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        services.AddScoped<IProductService, Application.Services.ProductService>();
        services.AddScoped<ICategoryService, Application.Services.CategoryService>();
        services.AddScoped<ISupplierService, Application.Services.SupplierService>();
        services.AddScoped<IOrderService, Application.Services.OrderService>();
        services.AddScoped<IUserService, Application.Services.UserService>();
    }

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["JwtSettings:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
        {
            throw new InvalidOperationException("JwtSettings:SecretKey configuration is missing or shorter than 32 characters. Application cannot start.");
        }

        var issuer = configuration["JwtSettings:Issuer"] ?? "SmartInventoryAPI";
        var audience = configuration["JwtSettings:Audience"] ?? "SmartInventoryAPIUsers";

        services.AddSingleton(new JwtService(secretKey, issuer, audience));
        services.AddScoped<IAuthService, AuthService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                    NameClaimType = System.Security.Claims.ClaimTypes.Name
                };
            });
    }

    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Smart Inventory Management API",
                Version = "v1.0.0",
                Description = "Production-grade API for managing inventory, categories, suppliers, and orders",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Your Company",
                    Email = "support@company.com"
                }
            });

            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme"
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}
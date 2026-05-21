using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using SmartInventory.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SmartInventory.Infrastructure.Services;

public interface IDbSeeder
{
    Task SeedAsync();
}

public class DbSeeder : IDbSeeder
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(AppDbContext context, IJwtService jwtService, IConfiguration configuration, ILogger<DbSeeder> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SeedAsync()
    {
        try
        {
            if (_context.Users.Any() || _context.Categories.Any())
            {
                _logger.LogInformation("Database already seeded. Skipping seed operation.");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            await SeedUsersAsync();
            await SeedCategoriesAsync();
            await SeedSuppliersAsync();
            await SeedProductsAsync();
            await SeedOrdersAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database seeding.");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        var adminPassword = _configuration["SeedSettings:AdminPassword"]!;
        var userPassword = _configuration["SeedSettings:DefaultUserPassword"]!;

        var adminPasswordHash = _jwtService.HashPassword(adminPassword);
        var userPasswordHash = _jwtService.HashPassword(userPassword);

        var users = new List<User>
        {
            new User
            {
                Username = "admin",
                Email = "admin@smartinventory.com",
                PasswordHash = adminPasswordHash,
                FullName = "System Administrator",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Username = "manager",
                Email = "manager@smartinventory.com",
                PasswordHash = userPasswordHash,
                FullName = "Inventory Manager",
                Role = "Manager",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Username = "employee",
                Email = "employee@smartinventory.com",
                PasswordHash = userPasswordHash,
                FullName = "Store Employee",
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Users.AddRangeAsync(users);
        _logger.LogInformation("Seeded {Count} users", users.Count);
    }

    private async Task SeedCategoriesAsync()
    {
        var categories = new List<Category>
        {
            new Category
            {
                Name = "Electronics",
                Description = "Electronic devices and accessories",
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Name = "Clothing",
                Description = "Apparel and fashion items",
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Name = "Home & Garden",
                Description = "Home and garden supplies",
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Name = "Sports & Outdoors",
                Description = "Sports equipment and outdoor gear",
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Name = "Books",
                Description = "Books and educational materials",
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Categories.AddRangeAsync(categories);
        _logger.LogInformation("Seeded {Count} categories", categories.Count);
    }

    private async Task SeedSuppliersAsync()
    {
        var suppliers = new List<Supplier>
        {
            new Supplier
            {
                Name = "TechSupply Co.",
                Email = "contact@techsupply.com",
                PhoneNumber = "+1-800-123-4567",
                Address = "123 Tech Street",
                City = "San Francisco",
                State = "CA",
                ZipCode = "94105",
                Country = "USA",
                ContactPerson = "John Tech",
                CreatedAt = DateTime.UtcNow
            },
            new Supplier
            {
                Name = "Global Textiles",
                Email = "sales@globaltextiles.com",
                PhoneNumber = "+1-800-234-5678",
                Address = "456 Fashion Ave",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Country = "USA",
                ContactPerson = "Sarah Fashion",
                CreatedAt = DateTime.UtcNow
            },
            new Supplier
            {
                Name = "Home Goods Plus",
                Email = "info@homegoods.com",
                PhoneNumber = "+1-800-345-6789",
                Address = "789 Home Road",
                City = "Chicago",
                State = "IL",
                ZipCode = "60601",
                Country = "USA",
                ContactPerson = "Mike Home",
                CreatedAt = DateTime.UtcNow
            },
            new Supplier
            {
                Name = "Sports Elite",
                Email = "orders@sportselite.com",
                PhoneNumber = "+1-800-456-7890",
                Address = "321 Sports Lane",
                City = "Miami",
                State = "FL",
                ZipCode = "33101",
                Country = "USA",
                ContactPerson = "Alex Sports",
                CreatedAt = DateTime.UtcNow
            },
            new Supplier
            {
                Name = "Knowledge Publishers",
                Email = "publishers@knowledge.com",
                PhoneNumber = "+1-800-567-8901",
                Address = "654 Book Street",
                City = "Boston",
                State = "MA",
                ZipCode = "02101",
                Country = "USA",
                ContactPerson = "Dr. Books",
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Suppliers.AddRangeAsync(suppliers);
        _logger.LogInformation("Seeded {Count} suppliers", suppliers.Count);
    }

    private async Task SeedProductsAsync()
    {
        var categories = await _context.Categories.ToListAsync();
        var suppliers = await _context.Suppliers.ToListAsync();

        var products = new List<Product>
        {
            new Product
            {
                Code = "ELEC-001",
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with USB receiver",
                Price = 29.99m,
                QuantityInStock = 150,
                ReorderLevel = 50,
                CategoryId = categories.First(c => c.Name == "Electronics").Id,
                SupplierId = suppliers.First(s => s.Name == "TechSupply Co.").Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Code = "ELEC-002",
                Name = "USB-C Cable",
                Description = "High-speed USB-C charging and data cable",
                Price = 12.99m,
                QuantityInStock = 300,
                ReorderLevel = 100,
                CategoryId = categories.First(c => c.Name == "Electronics").Id,
                SupplierId = suppliers.First(s => s.Name == "TechSupply Co.").Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Code = "ELEC-003",
                Name = "LED Monitor Stand",
                Description = "Adjustable LED monitor stand with USB hub",
                Price = 45.99m,
                QuantityInStock = 80,
                ReorderLevel = 30,
                CategoryId = categories.First(c => c.Name == "Electronics").Id,
                SupplierId = suppliers.First(s => s.Name == "TechSupply Co.").Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Code = "CLOTH-001",
                Name = "Cotton T-Shirt",
                Description = "100% organic cotton comfortable t-shirt",
                Price = 19.99m,
                QuantityInStock = 200,
                ReorderLevel = 75,
                CategoryId = categories.First(c => c.Name == "Clothing").Id,
                SupplierId = suppliers.First(s => s.Name == "Global Textiles").Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Code = "CLOTH-002",
                Name = "Jeans",
                Description = "Classic denim jeans",
                Price = 59.99m,
                QuantityInStock = 120,
                ReorderLevel = 40,
                CategoryId = categories.First(c => c.Name == "Clothing").Id,
                SupplierId = suppliers.First(s => s.Name == "Global Textiles").Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Code = "HOME-001",
                Name = "Bed Sheets Set",
                Description = "Luxury 1000 thread count bed sheets",
                Price = 89.99m,
                QuantityInStock = 60,
                ReorderLevel = 20,
                CategoryId = categories.First(c => c.Name == "Home & Garden").Id,
                SupplierId = suppliers.First(s => s.Name == "Home Goods Plus").Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Code = "HOME-002",
                Name = "Pillow",
                Description = "Memory foam pillow",
                Price = 34.99m,
                QuantityInStock = 100,
                ReorderLevel = 40,
                CategoryId = categories.First(c => c.Name == "Home & Garden").Id,
                SupplierId = suppliers.First(s => s.Name == "Home Goods Plus").Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Code = "SPORT-001",
                Name = "Yoga Mat",
                Description = "Non-slip yoga mat with carrying strap",
                Price = 24.99m,
                QuantityInStock = 85,
                ReorderLevel = 30,
                CategoryId = categories.First(c => c.Name == "Sports & Outdoors").Id,
                SupplierId = suppliers.First(s => s.Name == "Sports Elite").Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Code = "SPORT-002",
                Name = "Water Bottle",
                Description = "Insulated stainless steel water bottle",
                Price = 34.99m,
                QuantityInStock = 150,
                ReorderLevel = 60,
                CategoryId = categories.First(c => c.Name == "Sports & Outdoors").Id,
                SupplierId = suppliers.First(s => s.Name == "Sports Elite").Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Code = "BOOK-001",
                Name = "ASP.NET Core Guide",
                Description = "Complete guide to ASP.NET Core development",
                Price = 49.99m,
                QuantityInStock = 30,
                ReorderLevel = 15,
                CategoryId = categories.First(c => c.Name == "Books").Id,
                SupplierId = suppliers.First(s => s.Name == "Knowledge Publishers").Id,
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Code = "BOOK-002",
                Name = "Clean Code",
                Description = "A handbook of agile software craftsmanship",
                Price = 44.99m,
                QuantityInStock = 45,
                ReorderLevel = 20,
                CategoryId = categories.First(c => c.Name == "Books").Id,
                SupplierId = suppliers.First(s => s.Name == "Knowledge Publishers").Id,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Products.AddRangeAsync(products);
        _logger.LogInformation("Seeded {Count} products", products.Count);
    }

    private async Task SeedOrdersAsync()
    {
        var products = await _context.Products.ToListAsync();

        var orders = new List<Order>
        {
            new Order
            {
                OrderNumber = $"ORD-{DateTime.UtcNow.AddDays(-5).Ticks}",
                OrderDate = DateTime.UtcNow.AddDays(-5),
                Status = OrderStatus.Delivered,
                TotalAmount = 0,
                DeliveryAddress = "123 Main Street, Anytown, USA",
                Notes = "First sample order",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                OrderItems = new List<OrderItem>()
            },
            new Order
            {
                OrderNumber = $"ORD-{DateTime.UtcNow.AddDays(-2).Ticks}",
                OrderDate = DateTime.UtcNow.AddDays(-2),
                Status = OrderStatus.Confirmed,
                TotalAmount = 0,
                DeliveryAddress = "456 Oak Avenue, Somewhere, USA",
                Notes = "Second sample order",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                OrderItems = new List<OrderItem>()
            }
        };

        var mouse = products.First(p => p.Code == "ELEC-001");
        orders[0].OrderItems.Add(new OrderItem
        {
            ProductId = mouse.Id,
            Quantity = 2,
            UnitPrice = mouse.Price,
            TotalPrice = mouse.Price * 2,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        });
        orders[0].TotalAmount = orders[0].OrderItems.Sum(oi => oi.TotalPrice);

        var tshirt = products.First(p => p.Code == "CLOTH-001");
        var jeans = products.First(p => p.Code == "CLOTH-002");
        orders[1].OrderItems.Add(new OrderItem
        {
            ProductId = tshirt.Id,
            Quantity = 3,
            UnitPrice = tshirt.Price,
            TotalPrice = tshirt.Price * 3,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        });
        orders[1].OrderItems.Add(new OrderItem
        {
            ProductId = jeans.Id,
            Quantity = 1,
            UnitPrice = jeans.Price,
            TotalPrice = jeans.Price,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        });
        orders[1].TotalAmount = orders[1].OrderItems.Sum(oi => oi.TotalPrice);

        await _context.Orders.AddRangeAsync(orders);
        _logger.LogInformation("Seeded {Count} orders", orders.Count);
    }
}
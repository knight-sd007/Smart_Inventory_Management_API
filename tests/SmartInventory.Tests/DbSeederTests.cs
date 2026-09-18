namespace SmartInventory.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.Services;
using Xunit;

public class DbSeederTests
{
    private const string DummySecretKey = "super-secret-key-that-is-at-least-32-characters-long!";

    private (AppDbContext context, DbSeeder seeder) CreateTestContextAndSeeder()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "SeedSettings:AdminPassword", "Admin@123456" },
            { "SeedSettings:DefaultUserPassword", "User@123456" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();
        var jwtService = new JwtService(DummySecretKey);
        var mockLogger = new Mock<ILogger<DbSeeder>>();

        var seeder = new DbSeeder(context, jwtService, configuration, mockLogger.Object);
        return (context, seeder);
    }

    [Fact]
    public async Task SeedAsync_FreshDatabase_SeedsAllEntitiesSuccessfullyWithValidRelationships()
    {
        // Arrange
        var (context, seeder) = CreateTestContextAndSeeder();

        // Act
        await seeder.SeedAsync();

        // Assert - Users
        var users = await context.Users.ToListAsync();
        Assert.Equal(3, users.Count);
        Assert.Contains(users, u => u.Username == "admin" && u.Role == "Admin");
        Assert.Contains(users, u => u.Username == "manager" && u.Role == "Manager");
        Assert.Contains(users, u => u.Username == "employee" && u.Role == "User");
        Assert.All(users, u => Assert.True(u.Id > 0));

        // Assert - Categories
        var categories = await context.Categories.ToListAsync();
        Assert.Equal(5, categories.Count);
        Assert.All(categories, c => Assert.True(c.Id > 0));

        // Assert - Suppliers
        var suppliers = await context.Suppliers.ToListAsync();
        Assert.Equal(5, suppliers.Count);
        Assert.All(suppliers, s => Assert.True(s.Id > 0));

        // Assert - Products
        var products = await context.Products.ToListAsync();
        Assert.Equal(11, products.Count);
        Assert.All(products, p =>
        {
            Assert.True(p.Id > 0);
            Assert.True(p.CategoryId > 0);
            Assert.True(p.SupplierId > 0);
            Assert.True(p.Price > 0);
        });

        // Assert - Orders & OrderItems
        var orders = await context.Orders.Include(o => o.OrderItems).ToListAsync();
        Assert.Equal(2, orders.Count);
        Assert.All(orders, o =>
        {
            Assert.True(o.Id > 0);
            Assert.NotEmpty(o.OrderItems);
            Assert.True(o.TotalAmount > 0);
        });

        var orderItems = await context.OrderItems.ToListAsync();
        Assert.Equal(3, orderItems.Count);
        Assert.All(orderItems, oi =>
        {
            Assert.True(oi.Id > 0);
            Assert.True(oi.ProductId > 0);
            Assert.True(oi.OrderId > 0);
            Assert.True(oi.TotalPrice > 0);
        });
    }

    [Fact]
    public async Task SeedAsync_AlreadySeededDatabase_SkipsSeedingAndDoesNotDuplicate()
    {
        // Arrange
        var (context, seeder) = CreateTestContextAndSeeder();

        // Act - Initial seed
        await seeder.SeedAsync();

        var initialUserCount = await context.Users.CountAsync();
        var initialCategoryCount = await context.Categories.CountAsync();
        var initialSupplierCount = await context.Suppliers.CountAsync();
        var initialProductCount = await context.Products.CountAsync();
        var initialOrderCount = await context.Orders.CountAsync();

        // Act - Repeat seed
        await seeder.SeedAsync();

        // Assert - Counts must not change
        Assert.Equal(initialUserCount, await context.Users.CountAsync());
        Assert.Equal(initialCategoryCount, await context.Categories.CountAsync());
        Assert.Equal(initialSupplierCount, await context.Suppliers.CountAsync());
        Assert.Equal(initialProductCount, await context.Products.CountAsync());
        Assert.Equal(initialOrderCount, await context.Orders.CountAsync());
    }

    [Fact]
    public async Task SeedAsync_WhenUsersAlreadyExist_SkipsEntireSeeding()
    {
        // Arrange
        var (context, seeder) = CreateTestContextAndSeeder();
        context.Users.Add(new User
        {
            Username = "existinguser",
            Email = "existing@example.com",
            PasswordHash = "hash123",
            FullName = "Existing User",
            Role = "User"
        });
        await context.SaveChangesAsync();

        // Act
        await seeder.SeedAsync();

        // Assert
        Assert.Equal(1, await context.Users.CountAsync());
        Assert.Equal(0, await context.Categories.CountAsync());
        Assert.Equal(0, await context.Suppliers.CountAsync());
        Assert.Equal(0, await context.Products.CountAsync());
        Assert.Equal(0, await context.Orders.CountAsync());
    }
}

namespace SmartInventory.Tests;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.Repositories;
using Xunit;

public class RepositoryIntegrationTests
{
    private AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ProductRepository_AddAndGetByCode_WorksInMemory()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var catRepo = new CategoryRepository(context);
        var suppRepo = new SupplierRepository(context);
        var prodRepo = new ProductRepository(context);

        var category = await catRepo.AddAsync(new Category { Name = "Hardware", Description = "Tech Hardware" });
        var supplier = await suppRepo.AddAsync(new Supplier { Name = "TechSupplier", Email = "tech@supp.com" });

        var product = new Product
        {
            Code = "INTEG-PROD-01",
            Name = "Integration Test Product",
            Price = 129.99m,
            QuantityInStock = 25,
            ReorderLevel = 5,
            CategoryId = category.Id,
            SupplierId = supplier.Id,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await prodRepo.AddAsync(product);
        var retrieved = await prodRepo.GetByCodeAsync("INTEG-PROD-01");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Integration Test Product", retrieved.Name);
        Assert.Equal(129.99m, retrieved.Price);
    }

    [Fact]
    public async Task CategoryRepository_GetAllAsync_ReturnsPagedResults()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repo = new CategoryRepository(context);

        await repo.AddAsync(new Category { Name = "Cat 1", Description = "Desc 1" });
        await repo.AddAsync(new Category { Name = "Cat 2", Description = "Desc 2" });
        await repo.AddAsync(new Category { Name = "Cat 3", Description = "Desc 3" });

        // Act
        var paged = await repo.GetAllAsync(pageNumber: 1, pageSize: 2);

        // Assert
        Assert.Equal(2, paged.Count());
    }

    [Fact]
    public async Task OrderRepository_GetByStatusAsync_FiltersCorrectly()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repo = new OrderRepository(context);

        await repo.AddAsync(new Order { OrderNumber = "ORD-001", Status = OrderStatus.Pending, DeliveryAddress = "Addr 1" });
        await repo.AddAsync(new Order { OrderNumber = "ORD-002", Status = OrderStatus.Delivered, DeliveryAddress = "Addr 2" });
        await repo.AddAsync(new Order { OrderNumber = "ORD-003", Status = OrderStatus.Pending, DeliveryAddress = "Addr 3" });

        // Act
        var pendingOrders = await repo.GetByStatusAsync(OrderStatus.Pending, 1, 10);

        // Assert
        Assert.Equal(2, pendingOrders.Count());
        Assert.All(pendingOrders, o => Assert.Equal(OrderStatus.Pending, o.Status));
    }

    [Fact]
    public async Task UserRepository_GetByUsernameAsync_FindsUser()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var repo = new UserRepository(context);

        var user = new User
        {
            Username = "johndoe",
            Email = "john@example.com",
            PasswordHash = "HashedSecret123",
            Role = "Manager",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await repo.AddAsync(user);
        var retrieved = await repo.GetByUsernameAsync("johndoe");

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("john@example.com", retrieved.Email);
        Assert.Equal("Manager", retrieved.Role);
    }
}

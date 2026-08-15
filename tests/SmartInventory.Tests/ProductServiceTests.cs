namespace SmartInventory.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SmartInventory.Application.Interfaces;
using SmartInventory.Application.Services;
using SmartInventory.Domain.Entities;
using Xunit;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _service = new ProductService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetProductByIdAsync_ReturnsProduct_WhenIdExists()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Laptop", Code = "PROD-001", Price = 999.99m, QuantityInStock = 50 };
        _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        var result = await _service.GetProductByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Laptop", result.Name);
        Assert.Equal("PROD-001", result.Code);
    }

    [Fact]
    public async Task CreateProductAsync_ThrowsArgumentNullException_WhenProductIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateProductAsync(null!));
    }

    [Fact]
    public async Task CreateProductAsync_ThrowsArgumentException_WhenCodeIsEmpty()
    {
        // Arrange
        var product = new Product { Name = "Laptop", Code = "" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateProductAsync(product));
    }

    [Fact]
    public async Task CreateProductAsync_ThrowsArgumentException_WhenNameIsEmpty()
    {
        // Arrange
        var product = new Product { Name = "", Code = "PROD-002" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateProductAsync(product));
    }

    [Fact]
    public async Task CreateProductAsync_CallsRepositoryAddAsync_WhenProductIsValid()
    {
        // Arrange
        var product = new Product { Name = "Keyboard", Code = "PROD-003", Price = 49.99m, QuantityInStock = 100 };
        _mockRepo.Setup(r => r.AddAsync(product)).ReturnsAsync(product);

        // Act
        var result = await _service.CreateProductAsync(product);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Keyboard", result.Name);
        _mockRepo.Verify(r => r.AddAsync(product), Times.Once);
    }

    [Fact]
    public async Task GetLowStockProductsAsync_ReturnsLowStockList()
    {
        // Arrange
        var lowStockList = new List<Product>
        {
            new Product { Id = 2, Name = "Mouse", Code = "PROD-004", QuantityInStock = 2, ReorderLevel = 5 }
        };
        _mockRepo.Setup(r => r.GetLowStockProductsAsync(1, 10)).ReturnsAsync(lowStockList);

        // Act
        var result = await _service.GetLowStockProductsAsync(1, 10);

        // Assert
        Assert.Single(result);
        Assert.Equal("Mouse", result.First().Name);
    }
}

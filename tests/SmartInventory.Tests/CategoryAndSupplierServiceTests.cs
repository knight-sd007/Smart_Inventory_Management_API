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

public class CategoryAndSupplierServiceTests
{
    [Fact]
    public async Task CategoryService_CreateCategoryAsync_ValidatesAndAdds()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var category = new Category { Name = "Electronics", Description = "Electronic components and gadgets" };
        mockRepo.Setup(r => r.AddAsync(category)).ReturnsAsync(category);

        var service = new CategoryService(mockRepo.Object);

        // Act
        var result = await service.CreateCategoryAsync(category);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Electronics", result.Name);
        mockRepo.Verify(r => r.AddAsync(category), Times.Once);
    }

    [Fact]
    public async Task CategoryService_CreateCategoryAsync_Throws_WhenNameIsEmpty()
    {
        // Arrange
        var mockRepo = new Mock<ICategoryRepository>();
        var category = new Category { Name = "" };
        var service = new CategoryService(mockRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateCategoryAsync(category));
    }

    [Fact]
    public async Task SupplierService_CreateSupplierAsync_ValidatesAndAdds()
    {
        // Arrange
        var mockRepo = new Mock<ISupplierRepository>();
        var supplier = new Supplier { Name = "Acme Corp", Email = "contact@acme.com", PhoneNumber = "555-0199" };
        mockRepo.Setup(r => r.AddAsync(supplier)).ReturnsAsync(supplier);

        var service = new SupplierService(mockRepo.Object);

        // Act
        var result = await service.CreateSupplierAsync(supplier);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Acme Corp", result.Name);
        mockRepo.Verify(r => r.AddAsync(supplier), Times.Once);
    }

    [Fact]
    public async Task SupplierService_GetSupplierByNameAsync_Throws_WhenNameIsEmpty()
    {
        // Arrange
        var mockRepo = new Mock<ISupplierRepository>();
        var service = new SupplierService(mockRepo.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetSupplierByNameAsync(""));
    }
}

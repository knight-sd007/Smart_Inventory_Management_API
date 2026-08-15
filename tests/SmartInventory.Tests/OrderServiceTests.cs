namespace SmartInventory.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SmartInventory.Application.Interfaces;
using SmartInventory.Application.Services;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using Xunit;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockRepo;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _mockRepo = new Mock<IOrderRepository>();
        _service = new OrderService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ReturnsOrder_WhenIdExists()
    {
        // Arrange
        var order = new Order { Id = 10, OrderNumber = "ORD-2026-001", Status = OrderStatus.Pending, TotalAmount = 250.00m };
        _mockRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(order);

        // Act
        var result = await _service.GetOrderByIdAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ORD-2026-001", result.OrderNumber);
        Assert.Equal(OrderStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateOrderAsync_ThrowsArgumentNullException_WhenOrderIsNull()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateOrderAsync(null!));
    }

    [Fact]
    public async Task CreateOrderAsync_ThrowsArgumentException_WhenOrderNumberIsEmpty()
    {
        // Arrange
        var order = new Order { OrderNumber = "", DeliveryAddress = "123 Business St" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateOrderAsync(order));
    }

    [Fact]
    public async Task CreateOrderAsync_ThrowsArgumentException_WhenDeliveryAddressIsEmpty()
    {
        // Arrange
        var order = new Order { OrderNumber = "ORD-100", DeliveryAddress = "" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateOrderAsync(order));
    }

    [Fact]
    public async Task CreateOrderAsync_CallsRepositoryAddAsync_WhenOrderIsValid()
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "ORD-2026-002",
            DeliveryAddress = "456 Tech Park",
            Status = OrderStatus.Confirmed,
            TotalAmount = 1500.00m
        };
        _mockRepo.Setup(r => r.AddAsync(order)).ReturnsAsync(order);

        // Act
        var result = await _service.CreateOrderAsync(order);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ORD-2026-002", result.OrderNumber);
        _mockRepo.Verify(r => r.AddAsync(order), Times.Once);
    }

    [Fact]
    public async Task GetOrdersByStatusAsync_ReturnsMatchingOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order { Id = 1, OrderNumber = "ORD-001", Status = OrderStatus.Shipped }
        };
        _mockRepo.Setup(r => r.GetByStatusAsync(OrderStatus.Shipped, 1, 10)).ReturnsAsync(orders);

        // Act
        var result = await _service.GetOrdersByStatusAsync(OrderStatus.Shipped, 1, 10);

        // Assert
        Assert.Single(result);
        Assert.Equal(OrderStatus.Shipped, result.First().Status);
    }
}

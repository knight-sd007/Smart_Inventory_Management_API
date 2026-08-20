namespace SmartInventory.Tests;

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SmartInventory.API.Controllers;
using SmartInventory.API.Extensions;
using SmartInventory.Application.DTOs.Order;
using SmartInventory.Application.DTOs.Product;
using SmartInventory.Application.Services;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using SmartInventory.Infrastructure.Services;
using Xunit;

public class SecurityAndAuthorizationTests
{
    private const string ValidSecretKey = "super-secret-key-that-is-at-least-32-characters-long!";

    [Fact]
    public void AddJwtAuthentication_MissingSecretKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => services.AddJwtAuthentication(configuration));
        Assert.Contains("missing or shorter than 32 characters", ex.Message);
    }

    [Fact]
    public void AddJwtAuthentication_ShortSecretKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", "too-short-secret" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => services.AddJwtAuthentication(configuration));
        Assert.Contains("missing or shorter than 32 characters", ex.Message);
    }

    [Fact]
    public void AddJwtAuthentication_ValidSecretKey_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "JwtSettings:SecretKey", ValidSecretKey },
            { "JwtSettings:Issuer", "SmartInventoryAPI" },
            { "JwtSettings:Audience", "SmartInventoryAPIUsers" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        // Act
        services.AddJwtAuthentication(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var jwtService = provider.GetService<JwtService>();
        Assert.NotNull(jwtService);
    }

    [Fact]
    public void PasswordHashing_BCrypt_ProducesBCryptFormattedHashAndVerifies()
    {
        // Arrange
        var jwtService = new JwtService(ValidSecretKey);
        var plainPassword = "MySecretPassword123!";

        // Act
        var hash = jwtService.HashPassword(plainPassword);

        // Assert
        Assert.NotNull(hash);
        Assert.StartsWith("$2", hash);
        Assert.True(jwtService.VerifyPassword(plainPassword, hash));
        Assert.False(jwtService.VerifyPassword("WrongPassword123!", hash));
    }

    [Fact]
    public async Task OrdersController_Update_InvalidStatus_ReturnsBadRequest()
    {
        // Arrange
        var mockOrderService = new Mock<IOrderService>();
        var mockProductService = new Mock<IProductService>();
        var mockMapper = new Mock<IMapper>();
        var mockLogger = new Mock<ILogger<OrdersController>>();

        var controller = new OrdersController(mockOrderService.Object, mockProductService.Object, mockMapper.Object, mockLogger.Object);
        var updateDto = new UpdateOrderDto
        {
            Status = "NonExistentStatus",
            DeliveryAddress = "123 Main St",
            Notes = "Test"
        };

        // Act
        var result = await controller.Update(1, updateDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task OrdersController_Update_ValidStatus_SucceedsForAdminOrManager()
    {
        // Arrange
        var mockOrderService = new Mock<IOrderService>();
        var mockProductService = new Mock<IProductService>();
        var mockMapper = new Mock<IMapper>();
        var mockLogger = new Mock<ILogger<OrdersController>>();

        var existingOrder = new Order { Id = 1, OrderNumber = "ORD-001", Status = OrderStatus.Pending };
        mockOrderService.Setup(s => s.GetOrderByIdAsync(1)).ReturnsAsync(existingOrder);
        mockOrderService.Setup(s => s.UpdateOrderAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

        var controller = new OrdersController(mockOrderService.Object, mockProductService.Object, mockMapper.Object, mockLogger.Object);
        SetupControllerUserContext(controller, "Manager");

        var updateDto = new UpdateOrderDto
        {
            Status = "Shipped",
            DeliveryAddress = "456 Oak St",
            Notes = "Updated status"
        };

        // Act
        var result = await controller.Update(1, updateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal(OrderStatus.Shipped, existingOrder.Status);
    }

    [Fact]
    public void EndpointAuthorizationAttributes_EnforceExpectedRoleRequirements()
    {
        // ProductsController Create/Update/Delete require Admin,Manager
        var productsCreateAttr = GetAuthorizeAttribute<ProductsController>(nameof(ProductsController.Create));
        Assert.NotNull(productsCreateAttr);
        Assert.Equal("Admin,Manager", productsCreateAttr.Roles);

        var productsDeleteAttr = GetAuthorizeAttribute<ProductsController>(nameof(ProductsController.Delete));
        Assert.NotNull(productsDeleteAttr);
        Assert.Equal("Admin,Manager", productsDeleteAttr.Roles);

        // OrdersController Update/Delete require Admin,Manager
        var ordersUpdateAttr = GetAuthorizeAttribute<OrdersController>(nameof(OrdersController.Update));
        Assert.NotNull(ordersUpdateAttr);
        Assert.Equal("Admin,Manager", ordersUpdateAttr.Roles);

        var ordersDeleteAttr = GetAuthorizeAttribute<OrdersController>(nameof(OrdersController.Delete));
        Assert.NotNull(ordersDeleteAttr);
        Assert.Equal("Admin,Manager", ordersDeleteAttr.Roles);

        // CategoriesController Delete requires Admin
        var categoriesDeleteAttr = GetAuthorizeAttribute<CategoriesController>(nameof(CategoriesController.Delete));
        Assert.NotNull(categoriesDeleteAttr);
        Assert.Equal("Admin", categoriesDeleteAttr.Roles);

        // SuppliersController Delete requires Admin
        var suppliersDeleteAttr = GetAuthorizeAttribute<SuppliersController>(nameof(SuppliersController.Delete));
        Assert.NotNull(suppliersDeleteAttr);
        Assert.Equal("Admin", suppliersDeleteAttr.Roles);
    }

    private static Microsoft.AspNetCore.Authorization.AuthorizeAttribute? GetAuthorizeAttribute<TController>(string methodName)
    {
        var method = typeof(TController).GetMethod(methodName);
        if (method == null) return null;

        var attrs = method.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);
        return attrs.Length > 0 ? (Microsoft.AspNetCore.Authorization.AuthorizeAttribute)attrs[0] : null;
    }

    private static void SetupControllerUserContext(ControllerBase controller, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }
}

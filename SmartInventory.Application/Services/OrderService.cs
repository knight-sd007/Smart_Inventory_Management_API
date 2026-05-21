namespace SmartInventory.Application.Services;

using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using SmartInventory.Application.Interfaces;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _repository.GetAllAsync(pageNumber, pageSize);
    }

    public async Task<Order?> GetOrderByNumberAsync(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            throw new ArgumentException("Order number cannot be empty", nameof(orderNumber));
        return await _repository.GetByOrderNumberAsync(orderNumber);
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, int pageNumber = 1, int pageSize = 10)
    {
        return await _repository.GetByStatusAsync(status, pageNumber, pageSize);
    }

    public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int days = 30, int pageNumber = 1, int pageSize = 10)
    {
        if (days <= 0)
            throw new ArgumentException("Days must be greater than zero", nameof(days));
        return await _repository.GetRecentOrdersAsync(days, pageNumber, pageSize);
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        if (string.IsNullOrWhiteSpace(order.OrderNumber))
            throw new ArgumentException("Order number is required", nameof(order));
        if (string.IsNullOrWhiteSpace(order.DeliveryAddress))
            throw new ArgumentException("Delivery address is required", nameof(order));

        return await _repository.AddAsync(order);
    }

    public async Task UpdateOrderAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        await _repository.UpdateAsync(order);
    }

    public async Task DeleteOrderAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<int> GetOrderCountAsync()
    {
        return await _repository.CountAsync();
    }
}

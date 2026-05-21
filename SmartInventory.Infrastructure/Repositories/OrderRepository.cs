namespace SmartInventory.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Entities;
using SmartInventory.Domain.Enums;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Application.Interfaces;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status, int pageNumber = 1, int pageSize = 10)
    {
        return await _context.Orders
            .Where(o => o.Status == status && !o.IsDeleted)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _context.Orders
            .Where(o => o.OrderNumber == orderNumber && !o.IsDeleted)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int days = 30, int pageNumber = 1, int pageSize = 10)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        return await _context.Orders
            .Where(o => o.OrderDate >= startDate && !o.IsDeleted)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.OrderDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}

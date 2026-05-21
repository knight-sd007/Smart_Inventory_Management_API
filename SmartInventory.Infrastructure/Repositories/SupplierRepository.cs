namespace SmartInventory.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Application.Interfaces;

public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    private readonly AppDbContext _context;

    public SupplierRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Supplier?> GetByNameAsync(string name)
    {
        return await _context.Suppliers
            .Where(s => s.Name == name && !s.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<Supplier?> GetByEmailAsync(string email)
    {
        return await _context.Suppliers
            .Where(s => s.Email == email && !s.IsDeleted)
            .FirstOrDefaultAsync();
    }
}


namespace SmartInventory.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Application.Interfaces;

public class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, int pageNumber = 1, int pageSize = 10)
    {
        return await _context.Products
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetBySupplierAsync(int supplierId, int pageNumber = 1, int pageSize = 10)
    {
        return await _context.Products
            .Where(p => p.SupplierId == supplierId && !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Product?> GetByCodeAsync(string code)
    {
        return await _context.Products
            .Where(p => p.Code == code && !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _context.Products
            .Where(p => p.QuantityInStock <= p.ReorderLevel && !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .OrderBy(p => p.QuantityInStock)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}

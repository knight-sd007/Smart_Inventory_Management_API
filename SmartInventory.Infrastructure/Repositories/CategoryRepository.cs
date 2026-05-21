namespace SmartInventory.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using SmartInventory.Domain.Entities;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Application.Interfaces;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _context.Categories
            .Where(c => c.Name == name && !c.IsDeleted)
            .FirstOrDefaultAsync();
    }
}

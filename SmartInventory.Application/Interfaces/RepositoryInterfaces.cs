namespace SmartInventory.Application.Interfaces;

using SmartInventory.Domain.Entities;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<int> CountAsync();
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
}

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Supplier?> GetByNameAsync(string name);
    Task<Supplier?> GetByEmailAsync(string email);
}

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByCodeAsync(string code);
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<Product>> GetBySupplierAsync(int supplierId, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int pageNumber = 1, int pageSize = 10);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<IEnumerable<Order>> GetByStatusAsync(Domain.Enums.OrderStatus status, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<Order>> GetRecentOrdersAsync(int days = 30, int pageNumber = 1, int pageSize = 10);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
}

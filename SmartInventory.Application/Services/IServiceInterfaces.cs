namespace SmartInventory.Application.Services;

using SmartInventory.Domain.Entities;

public interface IProductService
{
    Task<Product?> GetProductByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<Product>> GetProductsBySupplierAsync(int supplierId, int pageNumber = 1, int pageSize = 10);
    Task<Product?> GetProductByCodeAsync(string code);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int pageNumber = 1, int pageSize = 10);
    Task<Product> CreateProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(int id);
    Task<int> GetProductCountAsync();
}

public interface ICategoryService
{
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<IEnumerable<Category>> GetAllCategoriesAsync(int pageNumber = 1, int pageSize = 10);
    Task<Category?> GetCategoryByNameAsync(string name);
    Task<Category> CreateCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(int id);
    Task<int> GetCategoryCountAsync();
}

public interface ISupplierService
{
    Task<Supplier?> GetSupplierByIdAsync(int id);
    Task<IEnumerable<Supplier>> GetAllSuppliersAsync(int pageNumber = 1, int pageSize = 10);
    Task<Supplier?> GetSupplierByNameAsync(string name);
    Task<Supplier> CreateSupplierAsync(Supplier supplier);
    Task UpdateSupplierAsync(Supplier supplier);
    Task DeleteSupplierAsync(int id);
    Task<int> GetSupplierCountAsync();
}

public interface IOrderService
{
    Task<Order?> GetOrderByIdAsync(int id);
    Task<IEnumerable<Order>> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 10);
    Task<Order?> GetOrderByNumberAsync(string orderNumber);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(Domain.Enums.OrderStatus status, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<Order>> GetRecentOrdersAsync(int days = 30, int pageNumber = 1, int pageSize = 10);
    Task<Order> CreateOrderAsync(Order order);
    Task UpdateOrderAsync(Order order);
    Task DeleteOrderAsync(int id);
    Task<int> GetOrderCountAsync();
}

public interface IUserService
{
    Task<User?> GetUserByIdAsync(int id);
    Task<IEnumerable<User>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int id);
    Task<int> GetUserCountAsync();
}

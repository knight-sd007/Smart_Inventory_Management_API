namespace SmartInventory.Application.Services;

using SmartInventory.Domain.Entities;
using SmartInventory.Application.Interfaces;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _repository.GetAllAsync(pageNumber, pageSize);
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int pageNumber = 1, int pageSize = 10)
    {
        return await _repository.GetByCategoryAsync(categoryId, pageNumber, pageSize);
    }

    public async Task<IEnumerable<Product>> GetProductsBySupplierAsync(int supplierId, int pageNumber = 1, int pageSize = 10)
    {
        return await _repository.GetBySupplierAsync(supplierId, pageNumber, pageSize);
    }

    public async Task<Product?> GetProductByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Product code cannot be empty", nameof(code));
        return await _repository.GetByCodeAsync(code);
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _repository.GetLowStockProductsAsync(pageNumber, pageSize);
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));
        if (string.IsNullOrWhiteSpace(product.Code))
            throw new ArgumentException("Product code is required", nameof(product));
        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Product name is required", nameof(product));

        return await _repository.AddAsync(product);
    }

    public async Task UpdateProductAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));
        await _repository.UpdateAsync(product);
    }

    public async Task DeleteProductAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<int> GetProductCountAsync()
    {
        return await _repository.CountAsync();
    }
}

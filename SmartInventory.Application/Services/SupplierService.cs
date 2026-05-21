namespace SmartInventory.Application.Services;

using SmartInventory.Domain.Entities;
using SmartInventory.Application.Interfaces;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repository;

    public SupplierService(ISupplierRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Supplier?> GetSupplierByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _repository.GetAllAsync(pageNumber, pageSize);
    }

    public async Task<Supplier?> GetSupplierByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Supplier name cannot be empty", nameof(name));
        return await _repository.GetByNameAsync(name);
    }

    public async Task<Supplier> CreateSupplierAsync(Supplier supplier)
    {
        if (supplier == null)
            throw new ArgumentNullException(nameof(supplier));
        if (string.IsNullOrWhiteSpace(supplier.Name))
            throw new ArgumentException("Supplier name is required", nameof(supplier));

        return await _repository.AddAsync(supplier);
    }

    public async Task UpdateSupplierAsync(Supplier supplier)
    {
        if (supplier == null)
            throw new ArgumentNullException(nameof(supplier));
        await _repository.UpdateAsync(supplier);
    }

    public async Task DeleteSupplierAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<int> GetSupplierCountAsync()
    {
        return await _repository.CountAsync();
    }
}

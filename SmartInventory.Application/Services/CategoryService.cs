namespace SmartInventory.Application.Services;

using SmartInventory.Domain.Entities;
using SmartInventory.Application.Interfaces;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _repository.GetAllAsync(pageNumber, pageSize);
    }

    public async Task<Category?> GetCategoryByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty", nameof(name));
        return await _repository.GetByNameAsync(name);
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new ArgumentException("Category name is required", nameof(category));

        return await _repository.AddAsync(category);
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));
        await _repository.UpdateAsync(category);
    }

    public async Task DeleteCategoryAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<int> GetCategoryCountAsync()
    {
        return await _repository.CountAsync();
    }
}

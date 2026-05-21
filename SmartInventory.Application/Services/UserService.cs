namespace SmartInventory.Application.Services;

using SmartInventory.Domain.Entities;
using SmartInventory.Application.Interfaces;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _repository.GetAllAsync(pageNumber, pageSize);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));
        return await _repository.GetByUsernameAsync(username);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        return await _repository.GetByEmailAsync(email);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrWhiteSpace(user.Username))
            throw new ArgumentException("Username is required", nameof(user));
        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ArgumentException("Email is required", nameof(user));

        return await _repository.AddAsync(user);
    }

    public async Task UpdateUserAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        await _repository.UpdateAsync(user);
    }

    public async Task DeleteUserAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _repository.CountAsync();
    }
}

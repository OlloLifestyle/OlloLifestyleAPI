using OlloLifestyleAPI.Core.Entities.FactoryFlowTracker;

namespace OlloLifestyleAPI.Application.Interfaces.Persistence;

public interface ICompanyRepository
{
    // FactoryFlowTracker User operations
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(Guid id);
    Task<IEnumerable<User>> GetUsersByDepartmentAsync(string department);
    Task<bool> UserEmailExistsAsync(string email);
}
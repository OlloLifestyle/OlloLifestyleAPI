using OlloLifestyleAPI.Application.DTOs.Tenant;

namespace OlloLifestyleAPI.Application.Interfaces.Services;

public interface IUserService
{
    // User Query Operations
    Task<IEnumerable<FactoryFlowTrackerUserDto>> GetAllUsersAsync();
    Task<FactoryFlowTrackerUserDto?> GetUserByIdAsync(Guid id);
    Task<UserListResponse> GetUsersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? department = null, int? status = null);
    Task<IEnumerable<FactoryFlowTrackerUserDto>> GetUsersByDepartmentAsync(string department);
    Task<IEnumerable<FactoryFlowTrackerUserDto>> GetActiveUsersAsync();
    Task<FactoryFlowTrackerUserDto?> GetUserByEmailAsync(string email);

    // User Management Operations
    Task<FactoryFlowTrackerUserDto> CreateUserAsync(CreateUserRequest request);
    Task<FactoryFlowTrackerUserDto> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid id);
    Task<bool> ActivateUserAsync(Guid id);
    Task<bool> DeactivateUserAsync(Guid id);
    Task<bool> SuspendUserAsync(Guid id);

    // Department Operations
    Task<IEnumerable<string>> GetDepartmentsAsync();
    Task<IEnumerable<string>> GetPositionsAsync();
    Task<IEnumerable<string>> GetPositionsByDepartmentAsync(string department);

    // Statistics
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetActiveUsersCountAsync();
    Task<Dictionary<string, int>> GetUsersByDepartmentCountAsync();
    Task<Dictionary<string, int>> GetUsersByStatusCountAsync();
}
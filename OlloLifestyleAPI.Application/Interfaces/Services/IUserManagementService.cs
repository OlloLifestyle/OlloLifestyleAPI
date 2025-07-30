using OlloLifestyleAPI.Application.DTOs.Master;

namespace OlloLifestyleAPI.Application.Interfaces.Services;

public interface IUserManagementService
{
    // User CRUD Operations
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> ActivateUserAsync(int id);
    Task<bool> DeactivateUserAsync(int id);

    // Password Management
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<bool> ResetPasswordAsync(int userId, string newPassword);

    // User-Role Management
    Task<bool> AssignRolesToUserAsync(int userId, List<int> roleIds);
    Task<bool> RemoveRoleFromUserAsync(int userId, int roleId);
    Task<IEnumerable<RoleInfo>> GetUserRolesAsync(int userId);

    // User-Company Management
    Task<bool> AssignCompaniesToUserAsync(int userId, List<int> companyIds);
    Task<bool> RemoveCompanyFromUserAsync(int userId, int companyId);
    Task<IEnumerable<CompanyInfo>> GetUserCompaniesAsync(int userId);

    // User Permissions (through roles)
    Task<IEnumerable<PermissionDto>> GetUserPermissionsAsync(int userId);
    Task<bool> UserHasPermissionAsync(int userId, string permissionName);
}
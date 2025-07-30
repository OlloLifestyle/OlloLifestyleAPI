using Microsoft.Extensions.Logging;
using OlloLifestyleAPI.Application.DTOs.Master;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Application.Interfaces.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace OlloLifestyleAPI.Infrastructure.Services.Master;

public class UserManagementService : IUserManagementService
{
    private readonly IMasterUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        IMasterUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UserManagementService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region User CRUD Operations

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return null;

            // Load related data
            var userWithRoles = await _unitOfWork.Users.GetQueryable()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserCompanies)
                    .ThenInclude(uc => uc.Company)
                .FirstOrDefaultAsync(u => u.Id == id);

            return _mapper.Map<UserDto>(userWithRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", id);
            throw;
        }
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        try
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.UserName == username);
            var user = users.FirstOrDefault();
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by username {Username}", username);
            throw;
        }
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        try
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            throw;
        }
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            // Check if username already exists
            var existingUsers = await _unitOfWork.Users.FindAsync(u => u.UserName == request.UserName);
            if (existingUsers.Any())
            {
                throw new ArgumentException($"User with username {request.UserName} already exists");
            }

            var user = new Core.Entities.Master.User
            {
                UserName = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = AuthService.HashPassword(request.Password),
                IsActive = true
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Assign roles if provided
            if (request.RoleIds.Any())
            {
                await AssignRolesToUserAsync(user.Id, request.RoleIds);
            }

            // Assign companies if provided
            if (request.CompanyIds.Any())
            {
                await AssignCompaniesToUserAsync(user.Id, request.CompanyIds);
            }

            _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);
            return _mapper.Map<UserDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            throw;
        }
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.IsActive = request.IsActive;

            _unitOfWork.Users.Update(user);

            // Update roles
            await UpdateUserRolesAsync(id, request.RoleIds);

            // Update companies
            await UpdateUserCompaniesAsync(id, request.CompanyIds);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User updated successfully with ID: {UserId}", id);
            return _mapper.Map<UserDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            // Remove user roles
            var userRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == id);
            foreach (var userRole in userRoles)
            {
                _unitOfWork.UserRoles.Remove(userRole);
            }

            // Remove user companies
            var userCompanies = await _unitOfWork.UserCompanies.FindAsync(uc => uc.UserId == id);
            foreach (var userCompany in userCompanies)
            {
                _unitOfWork.UserCompanies.Remove(userCompany);
            }

            _unitOfWork.Users.Remove(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User deleted successfully with ID: {UserId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            throw;
        }
    }

    public async Task<bool> ActivateUserAsync(int id)
    {
        return await UpdateUserStatusAsync(id, true);
    }

    public async Task<bool> DeactivateUserAsync(int id)
    {
        return await UpdateUserStatusAsync(id, false);
    }

    private async Task<bool> UpdateUserStatusAsync(int id, bool isActive)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            user.IsActive = isActive;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} status updated to {Status}", id, isActive ? "Active" : "Inactive");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user status for {UserId}", id);
            throw;
        }
    }

    #endregion

    #region Password Management

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return false;
            }

            // Update password
            user.PasswordHash = AuthService.HashPassword(request.NewPassword);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Password changed successfully for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.PasswordHash = AuthService.HashPassword(newPassword);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Password reset successfully for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
            throw;
        }
    }

    #endregion

    #region User Roles Management

    public async Task<bool> AssignRolesToUserAsync(int userId, List<int> roleIds)
    {
        try
        {
            // Remove existing roles
            var existingRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == userId);
            foreach (var existingRole in existingRoles)
            {
                _unitOfWork.UserRoles.Remove(existingRole);
            }

            // Add new roles
            foreach (var roleId in roleIds)
            {
                var userRole = new Core.Entities.Master.UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                };
                await _unitOfWork.UserRoles.AddAsync(userRole);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning roles to user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
    {
        try
        {
            var userRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            var userRole = userRoles.FirstOrDefault();
            
            if (userRole == null)
            {
                return false;
            }

            _unitOfWork.UserRoles.Remove(userRole);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleId} from user {UserId}", roleId, userId);
            throw;
        }
    }

    public async Task<IEnumerable<RoleInfo>> GetUserRolesAsync(int userId)
    {
        try
        {
            var userRoles = await _unitOfWork.UserRoles.GetQueryable()
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role)
                .ToListAsync();

            return _mapper.Map<IEnumerable<RoleInfo>>(userRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles for user {UserId}", userId);
            throw;
        }
    }

    private async Task UpdateUserRolesAsync(int userId, List<int> roleIds)
    {
        // Remove existing roles
        var existingRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == userId);
        foreach (var existingRole in existingRoles)
        {
            _unitOfWork.UserRoles.Remove(existingRole);
        }

        // Add new roles
        foreach (var roleId in roleIds)
        {
            var userRole = new Core.Entities.Master.UserRole
            {
                UserId = userId,
                RoleId = roleId
            };
            await _unitOfWork.UserRoles.AddAsync(userRole);
        }
    }

    #endregion

    #region User Companies Management

    public async Task<bool> AssignCompaniesToUserAsync(int userId, List<int> companyIds)
    {
        try
        {
            // Remove existing companies
            var existingCompanies = await _unitOfWork.UserCompanies.FindAsync(uc => uc.UserId == userId);
            foreach (var existingCompany in existingCompanies)
            {
                _unitOfWork.UserCompanies.Remove(existingCompany);
            }

            // Add new companies
            foreach (var companyId in companyIds)
            {
                var userCompany = new Core.Entities.Master.UserCompany
                {
                    UserId = userId,
                    CompanyId = companyId
                };
                await _unitOfWork.UserCompanies.AddAsync(userCompany);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning companies to user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<CompanyInfo>> GetUserCompaniesAsync(int userId)
    {
        try
        {
            var userCompanies = await _unitOfWork.UserCompanies.GetQueryable()
                .Include(uc => uc.Company)
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.Company)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CompanyInfo>>(userCompanies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving companies for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> RemoveCompanyFromUserAsync(int userId, int companyId)
    {
        try
        {
            var userCompanies = await _unitOfWork.UserCompanies.FindAsync(uc => uc.UserId == userId && uc.CompanyId == companyId);
            var userCompany = userCompanies.FirstOrDefault();
            
            if (userCompany == null)
            {
                return false;
            }

            _unitOfWork.UserCompanies.Remove(userCompany);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing company {CompanyId} from user {UserId}", companyId, userId);
            throw;
        }
    }

    private async Task UpdateUserCompaniesAsync(int userId, List<int> companyIds)
    {
        // Remove existing companies
        var existingCompanies = await _unitOfWork.UserCompanies.FindAsync(uc => uc.UserId == userId);
        foreach (var existingCompany in existingCompanies)
        {
            _unitOfWork.UserCompanies.Remove(existingCompany);
        }

        // Add new companies
        foreach (var companyId in companyIds)
        {
            var userCompany = new Core.Entities.Master.UserCompany
            {
                UserId = userId,
                CompanyId = companyId
            };
            await _unitOfWork.UserCompanies.AddAsync(userCompany);
        }
    }

    #endregion

    #region User Permissions

    public async Task<IEnumerable<PermissionDto>> GetUserPermissionsAsync(int userId)
    {
        try
        {
            var permissions = await _unitOfWork.UserRoles.GetQueryable()
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission)
                .Where(p => p.IsActive)
                .Distinct()
                .ToListAsync();

            return _mapper.Map<IEnumerable<PermissionDto>>(permissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UserHasPermissionAsync(int userId, string permissionName)
    {
        try
        {
            var hasPermission = await _unitOfWork.UserRoles.GetQueryable()
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .AnyAsync(rp => rp.Permission.Name == permissionName && rp.Permission.IsActive);

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permissionName, userId);
            throw;
        }
    }

    #endregion
}
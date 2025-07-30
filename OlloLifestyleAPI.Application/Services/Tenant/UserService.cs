using Microsoft.Extensions.Logging;
using OlloLifestyleAPI.Application.DTOs.Tenant;
using OlloLifestyleAPI.Application.Interfaces.Persistence;
using OlloLifestyleAPI.Application.Interfaces.Services;
using OlloLifestyleAPI.Core.Entities.FactoryFlowTracker;
using AutoMapper;

namespace OlloLifestyleAPI.Application.Services.Tenant;

public class UserService : IUserService
{
    private readonly ITenantUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(ITenantUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region Query Operations

    public async Task<IEnumerable<FactoryFlowTrackerUserDto>> GetAllUsersAsync()
    {
        try
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return _mapper.Map<IEnumerable<FactoryFlowTrackerUserDto>>(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all users");
            throw;
        }
    }

    public async Task<FactoryFlowTrackerUserDto?> GetUserByIdAsync(Guid id)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            return user != null ? _mapper.Map<FactoryFlowTrackerUserDto>(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user with ID: {Id}", id);
            throw;
        }
    }

    public Task<UserListResponse> GetUsersAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string? department = null, int? status = null)
    {
        try
        {
            var query = _unitOfWork.Users.GetQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u => u.FirstName.Contains(searchTerm) || 
                                        u.LastName.Contains(searchTerm) || 
                                        u.Email.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                query = query.Where(u => u.Department == department);
            }

            if (status.HasValue)
            {
                query = query.Where(u => (int)u.Status == status.Value);
            }

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var users = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(new UserListResponse
            {
                Users = _mapper.Map<IEnumerable<FactoryFlowTrackerUserDto>>(users),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting paginated users");
            throw;
        }
    }

    public async Task<IEnumerable<FactoryFlowTrackerUserDto>> GetUsersByDepartmentAsync(string department)
    {
        try
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Department == department);
            return _mapper.Map<IEnumerable<FactoryFlowTrackerUserDto>>(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting users by department: {Department}", department);
            throw;
        }
    }

    public async Task<IEnumerable<FactoryFlowTrackerUserDto>> GetActiveUsersAsync()
    {
        try
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Status == UserStatus.Active);
            return _mapper.Map<IEnumerable<FactoryFlowTrackerUserDto>>(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting active users");
            throw;
        }
    }

    public async Task<FactoryFlowTrackerUserDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == email);
            var user = users.FirstOrDefault();
            return user != null ? _mapper.Map<FactoryFlowTrackerUserDto>(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting user by email: {Email}", email);
            throw;
        }
    }

    #endregion

    #region Management Operations

    public async Task<FactoryFlowTrackerUserDto> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            // Check if email already exists
            var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Email == request.Email);
            if (existingUsers.Any())
            {
                throw new ArgumentException($"User with email {request.Email} already exists");
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Department = request.Department,
                Position = request.Position,
                HireDate = request.HireDate,
                Status = UserStatus.Active
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User created successfully with ID: {Id}", user.Id);
            return _mapper.Map<FactoryFlowTrackerUserDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user");
            throw;
        }
    }

    public async Task<FactoryFlowTrackerUserDto> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found");
            }

            // Check if email is being changed and if it already exists
            if (user.Email != request.Email)
            {
                var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Email == request.Email && u.Id != id);
                if (existingUsers.Any())
                {
                    throw new ArgumentException($"User with email {request.Email} already exists");
                }
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.Department = request.Department;
            user.Position = request.Position;
            user.HireDate = request.HireDate;
            user.Status = (UserStatus)request.Status;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User updated successfully with ID: {Id}", user.Id);
            return _mapper.Map<FactoryFlowTrackerUserDto>(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user with ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            _unitOfWork.Users.Remove(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User deleted successfully with ID: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user with ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> ActivateUserAsync(Guid id)
    {
        return await UpdateUserStatusAsync(id, UserStatus.Active);
    }

    public async Task<bool> DeactivateUserAsync(Guid id)
    {
        return await UpdateUserStatusAsync(id, UserStatus.Inactive);
    }

    public async Task<bool> SuspendUserAsync(Guid id)
    {
        return await UpdateUserStatusAsync(id, UserStatus.Suspended);
    }

    private async Task<bool> UpdateUserStatusAsync(Guid id, UserStatus status)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            user.Status = status;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User status updated to {Status} for ID: {Id}", status, id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user status for ID: {Id}", id);
            throw;
        }
    }

    #endregion

    #region Lookup Operations

    public async Task<IEnumerable<string>> GetDepartmentsAsync()
    {
        try
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return users.Select(u => u.Department).Distinct().Where(d => !string.IsNullOrWhiteSpace(d));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting departments");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetPositionsAsync()
    {
        try
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return users.Select(u => u.Position).Distinct().Where(p => !string.IsNullOrWhiteSpace(p));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting positions");
            throw;
        }
    }

    public async Task<IEnumerable<string>> GetPositionsByDepartmentAsync(string department)
    {
        try
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Department == department);
            return users.Select(u => u.Position).Distinct().Where(p => !string.IsNullOrWhiteSpace(p));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting positions for department: {Department}", department);
            throw;
        }
    }

    #endregion

    #region Statistics

    public async Task<int> GetTotalUsersCountAsync()
    {
        try
        {
            return await _unitOfWork.Users.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting total users count");
            throw;
        }
    }

    public async Task<int> GetActiveUsersCountAsync()
    {
        try
        {
            return await _unitOfWork.Users.CountAsync(u => u.Status == UserStatus.Active);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting active users count");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetUsersByDepartmentCountAsync()
    {
        try
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return users
                .Where(u => !string.IsNullOrWhiteSpace(u.Department))
                .GroupBy(u => u.Department)
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting users count by department");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetUsersByStatusCountAsync()
    {
        try
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return users
                .GroupBy(u => u.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting users count by status");
            throw;
        }
    }

    #endregion
}
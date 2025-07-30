using OlloLifestyleAPI.Application.DTOs.Master;

namespace OlloLifestyleAPI.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<bool> LogoutAsync(int userId);
    Task<LoginResponse> RefreshTokenAsync(int userId);
    Task<bool> ValidateUserAccessToCompanyAsync(int userId, int companyId);
    string GenerateJwtToken(int userId, string userName, string firstName, string lastName);
    Task<string> GenerateJwtTokenAsync(
        Core.Entities.Master.User user, 
        IList<Core.Entities.Master.Role> roles, 
        IList<Core.Entities.Master.Permission> permissions,
        IList<Core.Entities.Master.Company> companies);
}
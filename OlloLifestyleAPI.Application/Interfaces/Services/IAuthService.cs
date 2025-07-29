using OlloLifestyleAPI.Application.DTOs.Master;

namespace OlloLifestyleAPI.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<bool> ValidateUserAccessToCompanyAsync(int userId, int companyId);
    string GenerateJwtToken(int userId, string userName, string firstName, string lastName);
}
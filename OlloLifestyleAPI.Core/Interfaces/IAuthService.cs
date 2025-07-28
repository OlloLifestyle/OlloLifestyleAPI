using OlloLifestyleAPI.Core.DTOs;

namespace OlloLifestyleAPI.Core.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
}
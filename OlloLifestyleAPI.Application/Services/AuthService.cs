using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OlloLifestyleAPI.Core.DTOs;
using OlloLifestyleAPI.Core.Entities;
using OlloLifestyleAPI.Core.Interfaces;
using System.Security.Claims;

namespace OlloLifestyleAPI.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AuthService(
        UserManager<User> userManager,
        ITokenService tokenService,
        IMapper mapper)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var jwtToken = _tokenService.GenerateJwtToken(user, roles, claims.ToList());
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);

        var userDto = _mapper.Map<UserDto>(user);
        userDto = userDto with 
        { 
            Roles = roles.ToList(),
            Companies = await GetUserCompaniesAsync(user.Id)
        };

        return new LoginResponseDto
        {
            AccessToken = jwtToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User = userDto
        };
    }

    public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid token");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var newJwtToken = _tokenService.GenerateJwtToken(user, roles, claims.ToList());
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _userManager.UpdateAsync(user);

        return new TokenResponseDto
        {
            AccessToken = newJwtToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    public async Task<bool> LogoutAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<UserDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email is already registered");
        }

        var user = _mapper.Map<User>(request);
        user.SecurityStamp = Guid.NewGuid().ToString();

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"User creation failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "User");

        var userDto = _mapper.Map<UserDto>(user);
        return userDto with 
        { 
            Roles = new List<string> { "User" },
            Companies = new List<CompanyDto>()
        };
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        return result.Succeeded;
    }

    private async Task<List<CompanyDto>> GetUserCompaniesAsync(int userId)
    {
        return new List<CompanyDto>();
    }

    private int GetCurrentUserId()
    {
        return 1;
    }
}
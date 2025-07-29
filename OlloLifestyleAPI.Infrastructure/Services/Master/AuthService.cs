using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using OlloLifestyleAPI.Application.DTOs.Master;
using OlloLifestyleAPI.Application.Interfaces.Services;
using OlloLifestyleAPI.Infrastructure.Persistence;

namespace OlloLifestyleAPI.Infrastructure.Services.Master;

public class AuthService : IAuthService
{
    private readonly AppDbContext _appDbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext appDbContext,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _appDbContext = appDbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Find user with company information through UserCompanies
        var user = await _appDbContext.Users
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Company)
            .FirstOrDefaultAsync(u => u.UserName == request.UserName && u.IsActive);

        if (user == null)
        {
            _logger.LogWarning("Login attempt failed: User not found for username {UserName}", request.UserName);
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Check if user has access to any active companies
        var activeCompanies = user.UserCompanies
            .Where(uc => uc.Company.IsActive)
            .Select(uc => uc.Company)
            .ToList();

        if (!activeCompanies.Any())
        {
            _logger.LogWarning("Login attempt failed: User {UserId} has no access to active companies", user.Id);
            throw new UnauthorizedAccessException("No active company access found");
        }

        // Verify password
        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login attempt failed: Invalid password for user {UserId}", user.Id);
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _appDbContext.SaveChangesAsync();

        // Generate JWT token
        var token = GenerateJwtToken(user.Id, user.UserName, user.FirstName, user.LastName);

        var expireMinutes = _configuration.GetValue<int>("Jwt:ExpireMinutes", 1440);
        var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

        _logger.LogInformation("User {UserId} logged in successfully with access to {CompanyCount} companies", 
            user.Id, activeCompanies.Count);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserInfo
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive
            },
            Companies = activeCompanies.Select(c => new CompanyInfo
            {
                Id = c.Id,
                Name = c.Name,
                IsActive = c.IsActive
            }).ToList()
        };
    }

    public async Task<bool> ValidateUserAccessToCompanyAsync(int userId, int companyId)
    {
        var userCompany = await _appDbContext.UserCompanies
            .Include(uc => uc.User)
            .Include(uc => uc.Company)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CompanyId == companyId 
                                     && uc.User.IsActive && uc.Company.IsActive);

        return userCompany != null;
    }

    public string GenerateJwtToken(int userId, string userName, string firstName, string lastName)
    {
        var secretKey = _configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT Secret Key not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? "OlloLifestyleAPI";
        var audience = _configuration["Jwt:Audience"] ?? "OlloLifestyleAPI";
        var expireMinutes = _configuration.GetValue<int>("Jwt:ExpireMinutes", 1440);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.GivenName, firstName),
            new Claim(ClaimTypes.Surname, lastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        // Simple hash verification for demo purposes
        // In production, use BCrypt.Net or similar
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public static string HashPassword(string password)
    {
        // Simple password hashing for demo purposes
        // In production, use BCrypt.Net or similar
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
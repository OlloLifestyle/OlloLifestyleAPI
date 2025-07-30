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
        // Find user with company information, roles, and permissions
        var user = await _appDbContext.Users
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Company)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
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

        // Get user roles and permissions
        var userRoles = user.UserRoles.Select(ur => ur.Role).ToList();
        var userPermissions = userRoles
            .SelectMany(r => r.RolePermissions)
            .Select(rp => rp.Permission)
            .Where(p => p.IsActive)
            .Distinct()
            .ToList();

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _appDbContext.SaveChangesAsync();

        // Generate JWT token with roles and permissions
        var token = await GenerateJwtTokenAsync(user, userRoles, userPermissions, activeCompanies);

        var expireMinutes = _configuration.GetValue<int>("Jwt:ExpireMinutes", 1440);
        var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

        _logger.LogInformation("User {UserId} logged in successfully with access to {CompanyCount} companies, {RoleCount} roles, {PermissionCount} permissions", 
            user.Id, activeCompanies.Count, userRoles.Count, userPermissions.Count);

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

    public Task<string> GenerateJwtTokenAsync(
        Core.Entities.Master.User user, 
        IList<Core.Entities.Master.Role> roles, 
        IList<Core.Entities.Master.Permission> permissions,
        IList<Core.Entities.Master.Company> companies)
    {
        var secretKey = _configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT Secret Key not configured");
        var issuer = _configuration["Jwt:Issuer"] ?? "OlloLifestyleAPI";
        var audience = _configuration["Jwt:Audience"] ?? "OlloLifestyleAPI";
        var expireMinutes = _configuration.GetValue<int>("Jwt:ExpireMinutes", 1440);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.GivenName, user.FirstName),
            new(ClaimTypes.Surname, user.LastName),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
            claims.Add(new Claim("role_id", role.Id.ToString()));
        }

        // Add permission claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission.Name));
            claims.Add(new Claim($"permission_{permission.Module.ToLowerInvariant()}", permission.Action));
        }

        // Add company access claims
        foreach (var company in companies)
        {
            claims.Add(new Claim("company_id", company.Id.ToString()));
            claims.Add(new Claim("company_name", company.Name));
        }

        // Add custom claims for easier access control
        claims.Add(new Claim("user_type", roles.Any(r => r.IsSystemRole) ? "system" : "tenant"));
        claims.Add(new Claim("is_admin", roles.Any(r => r.Name == "Administrator").ToString().ToLower()));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    private static bool VerifyPassword(string password, string hash)
    {
        // Simple hash verification for demo purposes
        // In production, use BCrypt.Net or similar
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public async Task<bool> LogoutAsync(int userId)
    {
        try
        {
            var user = await _appDbContext.Users.FindAsync(userId);
            if (user != null)
            {
                // In a production system, you might want to:
                // 1. Invalidate the token by adding to a blacklist
                // 2. Update last logout time
                // 3. Clear any session data
                _logger.LogInformation("User {UserId} logged out", userId);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            throw;
        }
    }

    public async Task<LoginResponse> RefreshTokenAsync(int userId)
    {
        try
        {
            // Find user with full context for token generation
            var user = await _appDbContext.Users
                .Include(u => u.UserCompanies)
                    .ThenInclude(uc => uc.Company)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found or inactive");
            }

            // Get active companies
            var activeCompanies = user.UserCompanies
                .Where(uc => uc.Company.IsActive)
                .Select(uc => uc.Company)
                .ToList();

            if (!activeCompanies.Any())
            {
                throw new UnauthorizedAccessException("No active company access found");
            }

            // Get user roles and permissions
            var userRoles = user.UserRoles.Select(ur => ur.Role).ToList();
            var userPermissions = userRoles
                .SelectMany(r => r.RolePermissions)
                .Select(rp => rp.Permission)
                .Where(p => p.IsActive)
                .Distinct()
                .ToList();

            // Generate new token
            var token = await GenerateJwtTokenAsync(user, userRoles, userPermissions, activeCompanies);

            var expireMinutes = _configuration.GetValue<int>("Jwt:ExpireMinutes", 1440);
            var expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token for user {UserId}", userId);
            throw;
        }
    }

    public static string HashPassword(string password)
    {
        // Simple password hashing for demo purposes
        // In production, use BCrypt.Net or similar
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OlloLifestyleAPI.Application.Interfaces.Services;

namespace OlloLifestyleAPI.Infrastructure.Persistence.Factories;

public class CompanyDbFactory
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<CompanyDbFactory> _logger;

    public CompanyDbFactory(ITenantService tenantService, ILogger<CompanyDbFactory> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    public CompanyDbContext CreateDbContext()
    {
        var tenantInfo = _tenantService.GetCurrentTenant();
        if (tenantInfo == null)
        {
            throw new InvalidOperationException("No tenant context found. Ensure TenantMiddleware is properly configured.");
        }

        return CreateDbContextInternal(tenantInfo.ConnectionString);
    }

    public CompanyDbContext CreateDbContext(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        return CreateDbContextInternal(connectionString);
    }

    public async Task<CompanyDbContext> CreateDbContextAsync()
    {
        var tenantInfo = _tenantService.GetCurrentTenant();
        if (tenantInfo == null)
        {
            throw new InvalidOperationException("No tenant context found. Ensure TenantMiddleware is properly configured.");
        }

        var context = CreateDbContextInternal(tenantInfo.ConnectionString);
        
        try
        {
            await context.Database.CanConnectAsync();
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to tenant database for company {CompanyId}", tenantInfo.CompanyId);
            context.Dispose();
            throw new InvalidOperationException($"Unable to connect to tenant database: {ex.Message}", ex);
        }
    }

    public async Task<CompanyDbContext> CreateDbContextAsync(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
        }

        var context = CreateDbContextInternal(connectionString);
        
        try
        {
            await context.Database.CanConnectAsync();
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to database with provided connection string");
            context.Dispose();
            throw new InvalidOperationException($"Unable to connect to database: {ex.Message}", ex);
        }
    }

    private CompanyDbContext CreateDbContextInternal(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CompanyDbContext>();
        
        optionsBuilder.UseSqlServer(connectionString, options =>
        {
            options.CommandTimeout(30);
            options.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });

        optionsBuilder.EnableSensitiveDataLogging(false);
        optionsBuilder.EnableServiceProviderCaching(false);

        return new CompanyDbContext(optionsBuilder.Options);
    }
}
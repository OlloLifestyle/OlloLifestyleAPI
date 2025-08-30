using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using OlloLifestyleAPI.Infrastructure.Extensions;
using OlloLifestyleAPI.Middleware;
using OlloLifestyleAPI.Configuration;
using Serilog;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting up Ollo Lifestyle API");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();

// Add Health Checks with database connectivity
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running"))
    .AddDbContextCheck<OlloLifestyleAPI.Infrastructure.Persistence.AppDbContext>("master-database")
    .AddDbContextCheck<OlloLifestyleAPI.Infrastructure.Persistence.CompanyDbContext>("tenant-database");

// Add Memory Cache
builder.Services.AddMemoryCache();

// Configure Swagger/OpenAPI - Only in Development
if (builder.Environment.IsDevelopment())
{
    Log.Information("Configuring Swagger services for Development environment...");
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "Ollo Lifestyle API", 
            Version = "v1",
            Description = "A production-grade .NET 9.0 ASP.NET Core Web API using Clean Architecture with multi-tenancy support"
        });
        
        // Add JWT Authentication to Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        
        c.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            }
        });
    });
    Log.Information("Swagger services configured successfully for Development");
}
else
{
    Log.Information("Swagger services disabled in Production environment");
}

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Infrastructure services (DbContext, Multi-tenancy, etc.)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Application services
builder.Services.AddApplicationServices();

// Add Authentication services
builder.Services.AddAuthenticationServices();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key not configured")))
        };
    });

// Add Authorization with Policies
builder.Services.AddAuthorizationPolicies();

// Add Rate Limiting
builder.Services.AddRateLimiting(builder.Configuration);

// Add CORS - Environment specific
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Permissive CORS for Development
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
        Log.Information("CORS configured for Development: AllowAnyOrigin");
    }
    else
    {
        // Restrictive CORS for Production
        options.AddPolicy("CorsPolicy", policy =>
        {
            policy.WithOrigins("https://portal.ollolife.com")
                  .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                  .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With")
                  .AllowCredentials();
        });
        Log.Information("CORS configured for Production: portal.ollolife.com only");
    }
});

    var app = builder.Build();

    // Seed data in development
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<OlloLifestyleAPI.Infrastructure.Persistence.AppDbContext>();
        await OlloLifestyleAPI.Infrastructure.Data.SeedData.SeedMasterDataAsync(appDbContext);
        
        // Seed tenant data
        await OlloLifestyleAPI.Infrastructure.Data.SeedData.SeedTenantDataAsync(
            "Server=LAPTOP-418M7MUO\\SQLEXPRESS;Database=OlloLifestyleAPI_Tenant;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;");
    }

    // Configure the HTTP request pipeline.
    // Enable Swagger only in Development
    if (app.Environment.IsDevelopment())
    {
        Log.Information("Configuring Swagger middleware for Development environment...");
        
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "swagger/{documentName}/swagger.json";
        });
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ollo Lifestyle API v1");
            c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            c.DefaultModelsExpandDepth(-1);
        });
        
        Log.Information("Swagger middleware configured successfully for Development");
    }
    else
    {
        Log.Information("Swagger middleware disabled in Production environment");
    }

    // Add global exception handling middleware
    app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseCors("CorsPolicy");

    // Rate limiting - should be after routing but before authentication
    app.UseRateLimiter();

    app.UseAuthentication();

    // Add Tenant Middleware - MUST be after UseAuthentication and before UseAuthorization
    app.UseMiddleware<TenantMiddleware>();

    app.UseAuthorization();

    app.MapControllers();

    // Add a simple test endpoint
    app.MapGet("/test", () => new { 
        message = "API is working", 
        swagger = app.Environment.IsDevelopment() ? "available at /swagger" : "disabled in production", 
        environment = app.Environment.EnvironmentName,
        time = DateTime.UtcNow 
    });

    // Map Health Check endpoint with detailed response
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(x => new
                {
                    name = x.Key,
                    status = x.Value.Status.ToString(),
                    description = x.Value.Description,
                    duration = x.Value.Duration.TotalMilliseconds
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    });

    Log.Information("Ollo Lifestyle API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using OlloLifestyleAPI.Infrastructure.Extensions;
using OlloLifestyleAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
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

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Infrastructure services (DbContext, Multi-tenancy, etc.)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add Application services
builder.Services.AddApplicationServices();

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

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed data in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var appDbContext = scope.ServiceProvider.GetRequiredService<OlloLifestyleAPI.Infrastructure.Persistence.AppDbContext>();
    await OlloLifestyleAPI.Infrastructure.Data.SeedData.SeedMasterDataAsync(appDbContext);
    
    // Seed tenant data for both companies
    await OlloLifestyleAPI.Infrastructure.Data.SeedData.SeedTenantDataAsync(
        "Server=LAPTOP-418M7MUO\\SQLEXPRESS;Database=OlloLifestyle_Acme;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;");
    await OlloLifestyleAPI.Infrastructure.Data.SeedData.SeedTenantDataAsync(
        "Server=LAPTOP-418M7MUO\\SQLEXPRESS;Database=OlloLifestyle_GlobalTech;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ollo Lifestyle API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

// Add global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();

// Add Tenant Middleware - MUST be after UseAuthentication and before UseAuthorization
app.UseMiddleware<TenantMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OlloLifestyleAPI.Core.Entities;
using OlloLifestyleAPI.Extensions;
using OlloLifestyleAPI.Infrastructure.DbContexts;
using OlloLifestyleAPI.Infrastructure.Seeds;
using OlloLifestyleAPI.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ollo Lifestyle API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.MapControllers();

await SeedDataAsync(app);

app.Run();

static async Task SeedDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var appContext = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<Role>>();

        await appContext.Database.MigrateAsync();

        await RoleSeed.SeedAsync(roleManager);
        await UserSeed.SeedAsync(userManager);
        await CompanySeed.SeedAsync(appContext);
        await PermissionSeed.SeedAsync(appContext);

        Log.Information("Data seeding completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while seeding data");
        throw;
    }
}

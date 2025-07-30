using Microsoft.Extensions.DependencyInjection;
using OlloLifestyleAPI.Application.Interfaces.Services;

namespace OlloLifestyleAPI.Infrastructure.Extensions;

    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            // Add Authentication services
            services.AddScoped<IAuthService, Infrastructure.Services.Master.AuthService>();

            return services;
        }
    }
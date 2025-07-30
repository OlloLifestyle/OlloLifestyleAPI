using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace OlloLifestyleAPI.Infrastructure.Extensions.Application;

public static class ApplicationValidationExtensions
{
    public static IServiceCollection AddApplicationValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(OlloLifestyleAPI.Application.Validators.Master.LoginRequestValidator).Assembly);

        return services;
    }
}
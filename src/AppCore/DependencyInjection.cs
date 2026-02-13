using OrionSoft.AppCore.Application.Behaviours;
using OrionSoft.AppCore.Application.Interfaces;

using OrionSoft.AppCore.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace OrionSoft.AppCore;

/// <summary>
/// Provides extension methods for configuring AppCore services in the dependency injection container.
/// AOT-compatible implementation avoiding reflection-based registrations.
/// </summary>
public static class DependencyInjection {

    /// <summary>
    /// Registers core application services including MediatR pipeline behaviors and application services.
    /// Uses explicit registration instead of Assembly scanning for AOT compatibility.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddCoreApplication(this IServiceCollection services) {
        // Register MediatR pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        // Note: For AOT compatibility, validators should be registered explicitly instead of using
        // Assembly scanning. Add validators manually when implementing specific validation rules:
        // services.AddTransient<IValidator<YourRequest>, YourRequestValidator>();

        // Register interceptors


        #region Application Services
        services.AddTransient<IDateTimeService, DateTimeService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        #endregion

        return services;
    }
}

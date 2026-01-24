using System.Reflection;
using AppCore.Application.Behaviours;
using AppCore.Application.Interfaces;
using AppCore.Infrastructure.Data.Interceptors;
using AppCore.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AppCore;

public static class DependencyInjection {

    public static IServiceCollection AddCoreApplication(this IServiceCollection services) {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<SaveChangesInterceptor>();
        #region Servicios
        services.AddTransient<IDateTimeService, DateTimeService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        #endregion
        return services;
    }
}

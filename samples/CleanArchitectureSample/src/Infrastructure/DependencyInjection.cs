using System.Reflection;
using App.Application.Common;
using App.Application.Interfaces;
using App.Application.Interfaces.Services;
using App.Infrastructure.Data;
using App.Infrastructure.Repositories;
using App.Infrastructure.Services;
using App.Infrastructure.Mappings;
using App.Application.Domain.Entities;
using App.Infrastructure.Data.DAOs;
using AppCore.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace App.Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services) {
        services.AddDbContext<AppDbContext>(options =>
            options
                .UseModel(App.Infrastructure.Data.CompiledModels.AppDbContextModel.AppDbContextModel.Instance)
                .UseNpgsql(AppConstants.DefaultConnection, builder =>
                    builder.MigrationsHistoryTable("__EFMigrationsHistory", AppConstants.SchemaDB)));


        services.AddSingleton<EmployeMappingService>();
        services.AddSingleton<IMappingService<EmployeEntity, EmployeDao>>(sp => sp.GetRequiredService<EmployeMappingService>());
        services.AddSingleton<IMappingService<EmployeDao, EmployeEntity>>(sp => sp.GetRequiredService<EmployeMappingService>());

        services.AddScoped<IEmployeRepository, EmployeRepository>();

        services.AddHttpClient<PokemonService>();
        services.AddScoped<IPokeService, PokemonService>();

        return services;
    }
}

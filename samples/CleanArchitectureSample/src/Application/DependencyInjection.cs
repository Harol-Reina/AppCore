using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using AppCore;
using App.Application.DTOs.Request;
using App.Application.DTOs.Response;
using App.Application.Features.Employes.Command;
using App.Application.Features.Employes.Query;
using App.Application.Features.Pokemons.Query;
using MediatR;
using AppCore.Application.Wrappers;
using App.Application.Domain.Entities;

namespace App.Application;

public static class DependencyInjection {
    public static void AddApplication(this IServiceCollection services) {
        services.AddCoreApplication();
        
        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        
        // Validators
        services.AddTransient<IValidator<EmployeRequestDto>, EmployeRequestDtoValidator>();
        services.AddTransient<IValidator<EditEmployeCommand>, EditEmployeCommandValidator>();
        services.AddTransient<IValidator<AddEmployeCommand>, AddEmployeServiceValidator>();
    }
}

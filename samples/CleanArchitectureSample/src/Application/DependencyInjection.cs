using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrionSoft.AppCore;
using App.Application.DTOs.Request;
using App.Application.DTOs.Response;
using App.Application.Features.Employes.Command;
using OrionSoft.AppCore.Domain.Common;
using App.Application.Features.Employes.Query;
using App.Application.Features.Pokemons.Query;
using MediatR;
using OrionSoft.AppCore.Application.Wrappers;
using App.Application.Domain.Entities;

namespace App.Application;

public static class DependencyInjection {
    public static void AddApplication(this IServiceCollection services) {
        services.AddCoreApplication();
        
        // MediatR
        // MediatR Handlers registration
        services.AddTransient<IRequestHandler<GetAllEmployeQuery, Response<PaginationResponse<EmployeResponseDto>>>, GetAllEmployeQueryHandler>();
        services.AddTransient<IRequestHandler<GetByIdEmployeQuery, Response<EmployeResponseDto>>, GetByIdEmployeQueryHandler>();
        services.AddTransient<IRequestHandler<AddEmployeCommand, Response<EmployeResponseDto>>, AddEmployeCommandHandler>();
        services.AddTransient<IRequestHandler<EditEmployeCommand, Response<EmployeEntity>>, EditEmployeCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteEmployeCommand, Response>, DeleteEmployeCommandHandler>();
        
        services.AddTransient<IRequestHandler<GetAllPokemonQuery, Response<List<PokemonEntity>>>, GetAllPokemonQueryHandler>();

        services.AddMediatR(cfg => {
             cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly); // Keep for behaviors if any, though behaviors are usually generic.
             // Actually, explicitly registering handlers makes scanning redundant for them. 
             // But sticking to explicit is safer.
        });
        
        // Validators
        services.AddTransient<IValidator<EmployeRequestDto>, EmployeRequestDtoValidator>();
        services.AddTransient<IValidator<EditEmployeCommand>, EditEmployeCommandValidator>();
        services.AddTransient<IValidator<AddEmployeCommand>, AddEmployeServiceValidator>();
    }
}

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
        
        // Validators
        services.AddTransient<IValidator<EmployeRequestDto>, EmployeRequestDtoValidator>();
        services.AddTransient<IValidator<EditEmployeCommand>, EditEmployeCommandValidator>();
        services.AddTransient<IValidator<AddEmployeCommand>, AddEmployeServiceValidator>();

        // Manual MediatR Registration (AOT-Compatible)

        services.AddTransient<IMediator, Mediator>();
        services.AddTransient<ISender>(sp => sp.GetRequiredService<IMediator>());
        services.AddTransient<IPublisher>(sp => sp.GetRequiredService<IMediator>());

        // Handlers were explicitly registered below
        
        services.AddTransient<IRequestHandler<AddEmployeCommand, Response<EmployeResponseDto>>, AddEmployeCommandHandler>();
        services.AddTransient<IRequestHandler<EditEmployeCommand, Response<EmployeEntity>>, EditEmployeCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteEmployeCommand, Response<bool>>, DeleteEmployeCommandHandler>();
        services.AddTransient<IRequestHandler<GetAllEmployeQuery, Response<List<EmployeResponseDto>>>, GetAllEmployeQueryHandler>();
        services.AddTransient<IRequestHandler<GetByIdEmployeQuery, Response<EmployeResponseDto>>, GetByIdEmployeQueryHandler>();
        services.AddTransient<IRequestHandler<GetAllPokemonQuery, Response<List<PokemonEntity>>>, GetAllPokemonQueryHandler>();
    }
}

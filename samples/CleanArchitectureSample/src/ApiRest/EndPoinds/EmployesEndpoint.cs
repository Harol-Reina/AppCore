using App.Application.Domain.Entities;
using AppCore.Application.Interfaces;
using AppCore.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using App.Application.Features.Employes.Query;
using App.Application.Features.Employes.Command;

namespace App.ApiRest.EndPoinds;

public class EmployesEndpoint : IEndpointGroupBase {
    public void MapEndpoints(RouteGroupBuilder group, string groupName) {

        group.MapGet("", GetAllEmploye)
            .Produces<Response<List<EmployeEntity>>>();

        group.MapGet("/{EmployeId}", GetEmployeById)
            .Produces<Response<EmployeEntity>>();

        group.MapPost("", PostEmploye)
            .Produces<Response<EmployeEntity>>();

        group.MapPut("", PutEmploye)
            .Produces<Response<EmployeEntity>>();

        group.MapDelete("/{employeId}", DelEmployeById)
            .Produces<Response<bool>>();
    }


    private static async Task<IResult> GetAllEmploye([FromServices] IMediator mediator) {
        return Results.Ok(await mediator.Send(new GetAllEmployeQuery()));
    }

    private static async Task<IResult> GetEmployeById([FromServices] IMediator mediator, int employeId) {
        return Results.Ok(await mediator.Send(new GetByIdEmployeQuery { Id = employeId }));
    }

    private static async Task<IResult> PostEmploye([FromServices] IMediator mediator, AddEmployeCommand addEmployeCommand) {
        return Results.Ok(await mediator.Send(addEmployeCommand));
    }

    private static async Task<IResult> PutEmploye([FromServices] IMediator mediator, EditEmployeCommand editEmployeCommand) {
        return Results.Ok(await mediator.Send(editEmployeCommand));
    }

    private static async Task<IResult> DelEmployeById([FromServices] IMediator mediator, int employeId) {
        return Results.Ok(await mediator.Send(new DeleteEmployeCommand { Id = employeId }));
    }
}

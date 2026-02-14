using App.Application.Domain.Entities;
using OrionSoft.AppCore.Application.Interfaces;
using OrionSoft.AppCore.Application.Wrappers;
using OrionSoft.AppCore.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using App.Application.Features.Employes.Query;
using App.Application.Features.Employes.Command;

namespace App.ApiRest.EndPoinds;

public class EmployesEndpoint : IEndpointGroupBase {
    public void MapEndpoints(RouteGroupBuilder group, string groupName) {

        group.MapGet("", GetAllEmploye)
            .Produces<Response<PaginationResponse<EmployeEntity>>>();

        group.MapGet("/{EmployeId}", GetEmployeById)
            .Produces<Response<EmployeEntity>>();

        group.MapPost("", PostEmploye)
            .Produces<Response<EmployeEntity>>();

        group.MapPut("", PutEmploye)
            .Produces<Response<EmployeEntity>>();

        group.MapDelete("/{employeId}", DelEmployeById)
            .Produces<Response>();
    }


    private static async Task<IResult> GetAllEmploye(
        [FromServices] IMediator mediator,
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string sort = "Id",
        [FromQuery] bool asc = true) {
        return Results.Ok(await mediator.Send(new GetAllEmployeQuery { 
            Page = page, 
            PageSize = pageSize, 
            Sort = sort, 
            Asc = asc 
        }));
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

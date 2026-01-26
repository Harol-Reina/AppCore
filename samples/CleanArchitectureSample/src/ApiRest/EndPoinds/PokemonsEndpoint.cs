using App.Application.Domain.Entities;
using AppCore.Application.Interfaces;
using AppCore.Application.Wrappers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using App.Application.Features.Pokemons.Query;

namespace App.ApiRest.EndPoinds;

public class PokemonsEndpoint : IEndpointGroupBase {
    public void MapEndpoints(RouteGroupBuilder group, string groupName) {

        group.MapGet("", GetAllPokemon)
            .Produces<Response<List<PokemonEntity>>>();
    }


    private static async Task<IResult> GetAllPokemon([FromServices] IMediator mediator) {
        return Results.Ok(await mediator.Send(new GetAllPokemonQuery()));
    }

    
}

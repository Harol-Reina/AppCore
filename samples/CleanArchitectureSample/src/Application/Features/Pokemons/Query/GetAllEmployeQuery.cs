using App.Application.Domain.Entities;
using App.Application.Interfaces.Services;
using OrionSoft.AppCore.Application.Wrappers;
using MediatR;

namespace App.Application.Features.Pokemons.Query;

public class GetAllPokemonQuery : IRequest<Response<List<PokemonEntity>>> { }

public class GetAllPokemonQueryHandler(IPokeService pokeService)
    : IRequestHandler<GetAllPokemonQuery, Response<List<PokemonEntity>>> {
    private readonly IPokeService _employeRepository = pokeService;

    public async Task<Response<List<PokemonEntity>>> Handle(GetAllPokemonQuery request, CancellationToken cancellationToken) {
        var items = await _employeRepository.GetAllAsync();
        return Response<List<PokemonEntity>>.Success(
            "Finish Ok",
            items
        );
    }
}

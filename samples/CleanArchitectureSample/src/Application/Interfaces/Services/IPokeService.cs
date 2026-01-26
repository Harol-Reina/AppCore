using App.Application.Domain.Entities;

namespace App.Application.Interfaces.Services;
public interface IPokeService {
    Task<List<PokemonEntity>> GetAllAsync();
}

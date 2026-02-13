namespace App.Application.Common;

public sealed class AppSettings {
    public required string DefaultConnection { get; init; }
    public required string SchemaDB { get; init; }
    public string PokemonHost { get; init; } = "https://pokeapi.co/api/v2/";
}

using System.Text.Json.Serialization;

namespace App.Application.Domain.Entities;
public class PokemonEntity {
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
    [JsonPropertyName("url")]
    public string Url { get; set; } = null!;
}

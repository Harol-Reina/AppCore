
using System.Text.Json.Serialization;
using App.Application.Domain.Entities;

namespace App.Application.DTOs.Response;
public class PokemonResponse {

    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("next")]
    public string? Next { get; set; }
    [JsonPropertyName("previous")]
    public string? Previous { get; set; }
    [JsonPropertyName("results")]
    public List<PokemonEntity> Results { get; set; } = [];
}

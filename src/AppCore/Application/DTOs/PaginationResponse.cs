using System.Text.Json.Serialization;

namespace AppCore.Application.DTOs;

public class PaginationResponse<T> {

    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    [JsonPropertyName("results")]
    public List<T> Results { get; set; } = new();

    public PaginationResponse(List<T>? data = null) {
        Results = data ?? new List<T>();
        Count = Results.Count;
    }

    public static PaginationResponse<T> Success(List<T> data)
        => new PaginationResponse<T>(data);

}
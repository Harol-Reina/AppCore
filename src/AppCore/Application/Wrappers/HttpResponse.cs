using System.Net;
using System.Text.Json;
using AppCore.Application.Extensions;

namespace AppCore.Application.Wrappers;

public record HttpResponse<T> (HttpStatusCode StatusCode, long Time, JsonDocument? Response = null) {
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public override string ToString()
        => JsonExtend.Serialize(this);
}

using System.Net;
using System.Text.Json;
using OrionSoft.AppCore.Application.Extensions;

namespace OrionSoft.AppCore.Application.Wrappers;

public sealed record HttpResponse<T>(HttpStatusCode StatusCode, long Time, JsonDocument? Response = null) {
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public override string ToString()
        => JsonExtend.Serialize(this);
}

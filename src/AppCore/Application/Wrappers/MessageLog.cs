using AppCore.Application.Extensions;

namespace AppCore.Application.Wrappers;
public record MessageLog {
    public required string Tipo { get; init; }
    public string? Source { get; init; }
    public required dynamic Message { get; init; }
    public required string Metodo { get; init; }
    public required string Path { get; init; }
    public string? StackTrace { get; init; }

    public override string ToString() 
        => JsonExtend.Serialize(this);
}

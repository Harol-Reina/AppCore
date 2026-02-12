using System.Text.Json.Serialization;

namespace AppCore.UnitTests.TestHelpers;

public class JsonTestModel {
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}

[JsonSerializable(typeof(JsonTestModel))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true,
    PropertyNameCaseInsensitive = true)]
public partial class TestJsonContext : JsonSerializerContext { }

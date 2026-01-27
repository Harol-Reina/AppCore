using System.Text.Json.Serialization;

namespace AppCore.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<PakageLeadType>))]
public enum PakageLeadType {
    File, Json
}

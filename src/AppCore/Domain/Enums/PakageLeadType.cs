using System.Text.Json.Serialization;

namespace AppCore.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PakageLeadType {
   File, Json
}

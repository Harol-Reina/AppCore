using System.Text.Json.Serialization;

namespace AppCore.Infrastructure.Data.DAOs.Enum;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuotationStatus {
    /// <summary>
    /// La cotizacion ha sido iniciada
    /// </summary>
    Started,

    /// <summary>
    /// La cotizacion ha sido actualizada
    /// </summary>
    Updated,

    /// <summary>
    /// La cotizacion ha sido finalizada
    /// </summary>
    Finished,

    /// <summary>
    /// La cotizacion ha sido combertida en Poliza
    /// </summary>
    CoverageNote
}

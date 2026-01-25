using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppCore.Application.Extensions.JsonConverters;

/// <summary>
/// JsonConverter genérico para fechas con formato personalizable
/// </summary>
public class GenericDateJsonConverter(string dateFormat) : JsonConverter<DateTime> {
    private readonly string _dateFormat = dateFormat;

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            throw new JsonException($"No se puede convertir valor vacío a DateTime");

        if (DateTime.TryParseExact(value, _dateFormat, null, DateTimeStyles.None, out var date))
            return date;

        throw new JsonException($"No se puede convertir '{value}' a DateTime usando el formato {_dateFormat}");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString(_dateFormat));
    }
}

/// <summary>
/// JsonConverter genérico para fechas nullable con formato personalizable
/// </summary>
public class GenericNullableDateJsonConverter(string dateFormat) : JsonConverter<DateTime?> {
    private readonly string _dateFormat = dateFormat;

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return null;

        return DateTime.TryParseExact(value, _dateFormat, null, DateTimeStyles.None, out var date)
            ? date
            : null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options) {
        if (value.HasValue) {
            writer.WriteStringValue(value.Value.ToString(_dateFormat));
        } else {
            writer.WriteNullValue();
        }
    }
}

/// <summary>
/// Converters específicos usando el converter genérico
/// </summary>
public class DateYearMonthDayJsonConverter : GenericDateJsonConverter {
    public DateYearMonthDayJsonConverter() : base("yyyy-MM-dd") { }
}

public class DateYearMonthDayNoSeparatorJsonConverter : GenericDateJsonConverter {
    public DateYearMonthDayNoSeparatorJsonConverter() : base("yyyyMMdd") { }
}

public class NullableDateYearMonthDayJsonConverter : GenericNullableDateJsonConverter {
    public NullableDateYearMonthDayJsonConverter() : base("yyyy-MM-dd") { }
}

public class NullableDateYearMonthDayNoSeparatorJsonConverter : GenericNullableDateJsonConverter {
    public NullableDateYearMonthDayNoSeparatorJsonConverter() : base("yyyyMMdd") { }
}

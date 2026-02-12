using System.Text.Json;
using AppCore.Application.Extensions.JsonConverters;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Extensions.JsonConverters;

public class GenericNullableDateJsonConverterTests {
    [Fact]
    public void GenericNullableDateJsonConverter_WithCustomFormat_ShouldSerializeCorrectly() {
        // Arrange
        var converter = new GenericNullableDateJsonConverter("dd/MM/yyyy");
        DateTime? date = new DateTime(2024, 12, 25, 0, 0, 0, DateTimeKind.Utc);
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act
        var json = JsonSerializer.Serialize(date, options);

        // Assert
        json.Should().Be("\"25/12/2024\"");
    }

    [Fact]
    public void GenericNullableDateJsonConverter_WithNull_ShouldSerializeAsNull() {
        // Arrange
        var converter = new GenericNullableDateJsonConverter("dd/MM/yyyy");
        DateTime? date = null;
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act
        var json = JsonSerializer.Serialize(date, options);

        // Assert
        json.Should().Be("null");
    }

    [Fact]
    public void GenericNullableDateJsonConverter_WithCustomFormat_ShouldDeserializeCorrectly() {
        // Arrange
        var converter = new GenericNullableDateJsonConverter("dd/MM/yyyy");
        var json = "\"25/12/2024\"";
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act
        var date = JsonSerializer.Deserialize<DateTime?>(json, options);

        // Assert
        date.Should().NotBeNull();
        date!.Value.Year.Should().Be(2024);
        date!.Value.Month.Should().Be(12);
        date!.Value.Day.Should().Be(25);
    }

    [Fact]
    public void GenericNullableDateJsonConverter_WithNullJson_ShouldReturnNull() {
        // Arrange
        var converter = new GenericNullableDateJsonConverter("dd/MM/yyyy");
        var json = "null";
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act
        var date = JsonSerializer.Deserialize<DateTime?>(json, options);

        // Assert
        date.Should().BeNull();
    }

    [Fact]
    public void GenericNullableDateJsonConverter_WithEmptyString_ShouldReturnNull() {
        // Arrange
        var converter = new GenericNullableDateJsonConverter("dd/MM/yyyy");
        var json = "\"\"";
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act
        var date = JsonSerializer.Deserialize<DateTime?>(json, options);

        // Assert
        date.Should().BeNull();
    }

    [Fact]
    public void GenericNullableDateJsonConverter_WithInvalidFormat_ShouldReturnNull() {
        // Arrange
        var converter = new GenericNullableDateJsonConverter("yyyy-MM-dd");
        var json = "\"invalid-date\"";
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act
        var date = JsonSerializer.Deserialize<DateTime?>(json, options);

        // Assert
        date.Should().BeNull();
    }

    [Fact]
    public void GenericNullableDateJsonConverter_ReadNullToken_ShouldReturnNull() {
        // Arrange — directly exercise the Read path where TokenType == Null
        var converter = new GenericNullableDateJsonConverter("dd/MM/yyyy");
        var bytes = System.Text.Encoding.UTF8.GetBytes("null");
        var reader = new Utf8JsonReader(bytes);
        reader.Read(); // advance to the token

        // Act
        var result = converter.Read(ref reader, typeof(DateTime?), new JsonSerializerOptions());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GenericNullableDateJsonConverter_WriteNull_ShouldWriteNullValue() {
        // Arrange — directly exercise the Write path where !value.HasValue
        var converter = new GenericNullableDateJsonConverter("dd/MM/yyyy");
        using var stream = new System.IO.MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act
        converter.Write(writer, null, new JsonSerializerOptions());
        writer.Flush();

        // Assert
        var json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        json.Should().Be("null");
    }

    [Fact]
    public void GenericNullableDateJsonConverter_RoundTrip_ShouldPreserveValue() {
        // Arrange
        var converter = new GenericNullableDateJsonConverter("yyyy-MM-dd HH:mm:ss");
        DateTime? original = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Utc);
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act
        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<DateTime?>(json, options);

        // Assert
        json.Should().Be("\"2024-06-15 14:30:45\"");
        deserialized.Should().Be(original);
    }
}

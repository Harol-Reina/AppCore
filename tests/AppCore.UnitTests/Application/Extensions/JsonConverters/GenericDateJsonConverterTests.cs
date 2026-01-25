using AppCore.Application.Extensions.JsonConverters;
using FluentAssertions;
using System.Text.Json;
using Xunit;

namespace AppCore.UnitTests.Application.Extensions.JsonConverters;

public class GenericDateJsonConverterTests
{
    private class TestModel
    {
        [System.Text.Json.Serialization.JsonConverter(typeof(GenericDateJsonConverter))]
        public DateTime CustomDate { get; set; }
    }

    [Fact]
    public void GenericDateJsonConverter_WithCustomFormat_ShouldSerializeCorrectly()
    {
        // Arrange
        var converter = new GenericDateJsonConverter("dd/MM/yyyy");
        var date = new DateTime(2024, 12, 25);
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act
        var json = JsonSerializer.Serialize(date, options);

        // Assert
        json.Should().Be("\"25/12/2024\"");
    }

    [Fact]
    public void GenericDateJsonConverter_WithCustomFormat_ShouldDeserializeCorrectly()
    {
        // Arrange
        var converter = new GenericDateJsonConverter("dd/MM/yyyy");
        var json = "\"25/12/2024\"";
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act
        var date = JsonSerializer.Deserialize<DateTime>(json, options);

        // Assert
        date.Year.Should().Be(2024);
        date.Month.Should().Be(12);
        date.Day.Should().Be(25);
    }

    [Fact]
    public void GenericDateJsonConverter_WithInvalidFormat_ShouldThrowJsonException()
    {
        // Arrange
        var converter = new GenericDateJsonConverter("yyyy-MM-dd");
        var json = "\"invalid-date\"";
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act
        var act = () => JsonSerializer.Deserialize<DateTime>(json, options);

        // Assert
        act.Should().Throw<JsonException>()
            .WithMessage("*No se puede convertir*");
    }

    [Fact]
    public void GenericDateJsonConverter_WithEmptyString_ShouldThrowJsonException()
    {
        // Arrange
        var converter = new GenericDateJsonConverter("yyyy-MM-dd");
        var json = "\"\"";
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act
        var act = () => JsonSerializer.Deserialize<DateTime>(json, options);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void GenericDateJsonConverter_WithMilitaryTimeFormat_ShouldWork()
    {
        // Arrange
        var converter = new GenericDateJsonConverter("yyyy-MM-dd HH:mm:ss");
        var dateTime = new DateTime(2024, 6, 15, 14, 30, 45);
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);

        // Act
        var json = JsonSerializer.Serialize(dateTime, options);
        var deserialized = JsonSerializer.Deserialize<DateTime>(json, options);

        // Assert
        json.Should().Be("\"2024-06-15 14:30:45\"");
        deserialized.Should().Be(dateTime);
    }
}

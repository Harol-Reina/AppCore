using System.Text.Json;
using AppCore.Application.Extensions.JsonConverters;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Extensions.JsonConverters;

public class StringToLongConverterTests {
    private readonly JsonSerializerOptions _options;

    public StringToLongConverterTests() {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new StringToLongConverter());
    }

    [Fact]
    public void Read_NumericString_ShouldConvertToLong() {
        // Arrange
        var json = "\"123456789\"";

        // Act
        var result = JsonSerializer.Deserialize<long>(json, _options);

        // Assert
        result.Should().Be(123456789L);
    }

    [Fact]
    public void Read_NumericValue_ShouldReturnLong() {
        // Arrange
        var json = "987654321";

        // Act
        var result = JsonSerializer.Deserialize<long>(json, _options);

        // Assert
        result.Should().Be(987654321L);
    }

    [Fact]
    public void Write_LongValue_ShouldSerializeAsString() {
        // Arrange
        var value = 123456789L;

        // Act
        var json = JsonSerializer.Serialize(value, _options);

        // Assert
        json.Should().Be("\"123456789\"");
    }

    [Fact]
    public void Read_InvalidString_ShouldThrowJsonException() {
        // Arrange
        var json = "\"not-a-number\"";

        // Act
        var act = () => JsonSerializer.Deserialize<long>(json, _options);

        // Assert
        act.Should().Throw<JsonException>();
    }
}

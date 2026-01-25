using System.Text.Json;
using AppCore.Application.Extensions.JsonConverters;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Extensions.JsonConverters;

public class NullableDateYearMonthDayJsonConverterTests {
    private readonly JsonSerializerOptions _options;

    public NullableDateYearMonthDayJsonConverterTests() {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new NullableDateYearMonthDayJsonConverter());
    }

    [Fact]
    public void Read_ValidDateString_ShouldDeserializeCorrectly() {
        // Arrange
        var json = "\"2024-03-15\"";

        // Act
        var result = JsonSerializer.Deserialize<DateTime?>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Year.Should().Be(2024);
        result.Value.Month.Should().Be(3);
        result.Value.Day.Should().Be(15);
    }

    [Fact]
    public void Read_NullValue_ShouldDeserializeAsNull() {
        // Arrange
        var json = "null";

        // Act
        var result = JsonSerializer.Deserialize<DateTime?>(json, _options);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Write_ValidDateTime_ShouldSerializeCorrectly() {
        // Arrange
        DateTime? date = new DateTime(2024, 3, 15, 10, 30, 45, DateTimeKind.Utc);

        // Act
        var json = JsonSerializer.Serialize(date, _options);

        // Assert
        json.Should().Be("\"2024-03-15\"");
    }

    [Fact]
    public void Write_NullValue_ShouldSerializeAsNull() {
        // Arrange
        DateTime? date = null;

        // Act
        var json = JsonSerializer.Serialize(date, _options);

        // Assert
        json.Should().Be("null");
    }
}

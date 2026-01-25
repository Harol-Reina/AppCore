using System.Text.Json;
using AppCore.Application.Extensions.JsonConverters;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Extensions.JsonConverters;

public class DateYearMonthDayNoSeparatorJsonConverterTests {
    private readonly JsonSerializerOptions _options;

    public DateYearMonthDayNoSeparatorJsonConverterTests() {
        _options = new JsonSerializerOptions();
        _options.Converters.Add(new DateYearMonthDayNoSeparatorJsonConverter());
    }

    [Fact]
    public void Read_ValidDateString_ShouldDeserializeCorrectly() {
        // Arrange
        var json = "\"20240315\"";

        // Act
        var result = JsonSerializer.Deserialize<DateTime>(json, _options);

        // Assert
        result.Year.Should().Be(2024);
        result.Month.Should().Be(3);
        result.Day.Should().Be(15);
    }

    [Fact]
    public void Write_ValidDateTime_ShouldSerializeCorrectly() {
        // Arrange
        var date = new DateTime(2024, 3, 15, 10, 30, 45, DateTimeKind.Utc);

        // Act
        var json = JsonSerializer.Serialize(date, _options);

        // Assert
        json.Should().Be("\"20240315\"");
    }

    [Theory]
    [InlineData("\"2024-03-15\"")]
    [InlineData("\"2024/03/15\"")]
    [InlineData("\"202403\"")]
    public void Read_InvalidFormat_ShouldThrowJsonException(string json) {
        // Act
        var act = () => JsonSerializer.Deserialize<DateTime>(json, _options);

        // Assert
        act.Should().Throw<JsonException>();
    }
}

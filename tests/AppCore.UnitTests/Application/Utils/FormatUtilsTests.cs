using AppCore.Application.Exceptions;
using AppCore.Application.Utils;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Utils;

public class FormatUtilsTests {
    [Theory]
    [InlineData("550e8400-e29b-41d4-a716-446655440000")]
    [InlineData("6ba7b810-9dad-11d1-80b4-00c04fd430c8")]
    [InlineData("6ba7b811-9dad-11d1-80b4-00c04fd430c8")]
    public void ParseGuid_WithValidGuidString_ShouldReturnParsedGuid(string validGuidString) {
        // Arrange
        var expectedGuid = Guid.Parse(validGuidString);

        // Act
        var result = FormatUtils.ParseGuid(validGuidString);

        // Assert
        result.Should().Be(expectedGuid);
    }

    [Fact]
    public void ParseGuid_WithValidGuidStringAndCustomParameterName_ShouldReturnParsedGuid() {
        // Arrange
        var validGuidString = "550e8400-e29b-41d4-a716-446655440000";
        var expectedGuid = Guid.Parse(validGuidString);
        var parameterName = "CustomId";

        // Act
        var result = FormatUtils.ParseGuid(validGuidString, parameterName);

        // Assert
        result.Should().Be(expectedGuid);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("123")]
    [InlineData("not-a-guid-at-all")]
    [InlineData("550e8400-e29b-41d4-a716")]  // Incomplete GUID
    public void ParseGuid_WithInvalidGuidString_ShouldThrowBadRequestException(string invalidGuidString) {
        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => FormatUtils.ParseGuid(invalidGuidString));
        exception.Message.Should().Contain("Invalid GUID format for Id");
        if (!string.IsNullOrEmpty(invalidGuidString)) {
            exception.Message.Should().Contain(invalidGuidString);
        }
    }

    [Fact]
    public void ParseGuid_WithEmptyGuidString_ShouldThrowBadRequestException() {
        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => FormatUtils.ParseGuid(""));
        exception.Message.Should().Contain("Invalid GUID format for Id");
    }

    [Fact]
    public void ParseGuid_WithNullInput_ShouldThrowBadRequestException() {
        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => FormatUtils.ParseGuid(null));
        exception.Message.Should().Contain("Invalid GUID format for Id");
    }

    [Theory]
    [InlineData("invalid-guid", "UserId")]
    [InlineData("123", "ProductId")]
    public void ParseGuid_WithInvalidGuidStringAndCustomParameterName_ShouldThrowBadRequestExceptionWithCustomParameter(string invalidGuidString, string parameterName) {
        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => FormatUtils.ParseGuid(invalidGuidString, parameterName));
        exception.Message.Should().Contain($"Invalid GUID format for {parameterName}");
        exception.Message.Should().Contain(invalidGuidString);
    }

    [Fact]
    public void ParseGuid_WithEmptyGuidStringAndCustomParameterName_ShouldThrowBadRequestExceptionWithCustomParameter() {
        // Arrange
        var parameterName = "OrderId";

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => FormatUtils.ParseGuid("", parameterName));
        exception.Message.Should().Contain($"Invalid GUID format for {parameterName}");
    }

    [Fact]
    public void ParseGuid_WithNullInputAndCustomParameterName_ShouldThrowBadRequestExceptionWithCustomParameter() {
        // Arrange
        var parameterName = "CustomId";

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => FormatUtils.ParseGuid(null, parameterName));
        exception.Message.Should().Contain($"Invalid GUID format for {parameterName}");
    }

    [Fact]
    public void ParseGuid_WithEmptyGuid_ShouldThrowBadRequestException() {
        // Arrange
        var emptyGuidString = Guid.Empty.ToString();

        // Act
        var result = FormatUtils.ParseGuid(emptyGuidString);

        // Assert
        result.Should().Be(Guid.Empty);
    }

    [Fact]
    public void ParseGuid_WithUppercaseGuidString_ShouldReturnParsedGuid() {
        // Arrange
        var guidString = "550E8400-E29B-41D4-A716-446655440000";
        var expectedGuid = Guid.Parse(guidString);

        // Act
        var result = FormatUtils.ParseGuid(guidString);

        // Assert
        result.Should().Be(expectedGuid);
    }

    [Fact]
    public void ParseGuid_WithLowercaseGuidString_ShouldReturnParsedGuid() {
        // Arrange
        var guidString = "550e8400-e29b-41d4-a716-446655440000";
        var expectedGuid = Guid.Parse(guidString);

        // Act
        var result = FormatUtils.ParseGuid(guidString);

        // Assert
        result.Should().Be(expectedGuid);
    }

    [Fact]
    public void ParseGuid_WithGuidWithoutDashes_ShouldReturnParsedGuid() {
        // Arrange
        var guidWithoutDashes = "550e8400e29b41d4a716446655440000";
        var expectedGuid = new Guid("550e8400-e29b-41d4-a716-446655440000");

        // Act
        var result = FormatUtils.ParseGuid(guidWithoutDashes);

        // Assert - .NET can actually parse GUIDs without dashes
        result.Should().Be(expectedGuid);
    }
}

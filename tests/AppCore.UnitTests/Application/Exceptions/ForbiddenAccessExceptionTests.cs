using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class ForbiddenAccessExceptionTests {
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage() {
        // Arrange & Act
        var exception = new ForbiddenAccessException("Access denied");

        // Assert
        exception.Message.Should().Be("Access denied");
    }

    [Fact]
    public void Constructor_ShouldSetErrorCode() {
        // Arrange & Act
        var exception = new ForbiddenAccessException("Forbidden resource");

        // Assert
        exception.MessageLog.Should().NotBeNull();
        exception.Error.Should().NotBeNull();
        exception.Error.Code.Should().Be("FORBID-001");
        exception.Error.Message.Should().Be("Forbidden resource");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedMessage() {
        // Arrange
        var exception = new ForbiddenAccessException("No access");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("ForbiddenAccessException");
        result.Should().Contain("No access");
    }
}

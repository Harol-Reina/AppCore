using OrionSoft.AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Exceptions;

public class GatewayTimeoutExceptionTests {
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage() {
        // Arrange & Act
        var exception = new GatewayTimeoutException("Upstream service timed out");

        // Assert
        exception.Message.Should().Be("Upstream service timed out");
        exception.Error.Code.Should().Be("TIMEOUT-001");
    }

    [Fact]
    public void Constructor_ShouldInheritFromCustomException() {
        // Arrange & Act
        var exception = new GatewayTimeoutException("Timeout");

        // Assert
        exception.Should().BeAssignableTo<CustomException>();
    }

    [Fact]
    public void Constructor_ShouldCaptureCallerInformation() {
        // Arrange & Act
        var exception = new GatewayTimeoutException("Test timeout");

        // Assert
        exception.MessageLog.Method.Should().NotBeNullOrEmpty();
        exception.MessageLog.Path.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedMessage() {
        // Arrange
        var exception = new GatewayTimeoutException("Gateway timed out");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("GatewayTimeoutException");
        result.Should().Contain("Gateway timed out");
    }
}

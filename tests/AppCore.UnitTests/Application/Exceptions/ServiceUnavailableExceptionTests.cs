using OrionSoft.AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Exceptions;

public class ServiceUnavailableExceptionTests {
    [Fact]
    public void Constructor_WithServiceNameAndMessage_ShouldSetProperties() {
        // Arrange & Act
        var exception = new ServiceUnavailableException("PaymentGateway", "Payment service is down");

        // Assert
        exception.ServiceName.Should().Be("PaymentGateway");
        exception.Message.Should().Be("Payment service is down");
        exception.Error.Code.Should().Be("SVC-UNAVAIL-001");
    }

    [Fact]
    public void Constructor_ShouldInheritFromCustomException() {
        // Arrange & Act
        var exception = new ServiceUnavailableException("Redis", "Cache unavailable");

        // Assert
        exception.Should().BeAssignableTo<CustomException>();
    }

    [Fact]
    public void Constructor_ShouldCaptureCallerInformation() {
        // Arrange & Act
        var exception = new ServiceUnavailableException("DB", "Database down");

        // Assert
        exception.MessageLog.Method.Should().NotBeNullOrEmpty();
        exception.MessageLog.Path.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedMessage() {
        // Arrange
        var exception = new ServiceUnavailableException("API", "External API unavailable");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("ServiceUnavailableException");
        result.Should().Contain("External API unavailable");
    }
}

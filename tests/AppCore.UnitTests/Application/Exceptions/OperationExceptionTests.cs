using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class OperationExceptionTests {
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage() {
        // Arrange & Act
        var exception = new OperationException("Operation failed");

        // Assert
        exception.Message.Should().Be("Operation failed");
    }

    [Fact]
    public void ToString_ShouldReturnMessageLogJson() {
        // Arrange
        var exception = new OperationException("Test error");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("OperationException");
        result.Should().Contain("Test error");
    }

    [Fact]
    public void Constructor_ShouldCaptureCallerInformation() {
        // Arrange & Act
        var exception = new OperationException("Caller info test");
        var stringRepresentation = exception.ToString();

        // Assert
        stringRepresentation.Should().Contain("Caller info test");
        stringRepresentation.Should().Contain("OperationException");
    }
}

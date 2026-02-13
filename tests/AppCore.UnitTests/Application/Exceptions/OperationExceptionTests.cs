using OrionSoft.AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Exceptions;

public class OperationExceptionTests {
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage() {
        // Arrange & Act
        var exception = new OperationException("Operation failed");

        // Assert
        exception.Message.Should().Be("Operation failed");
    }

    [Fact]
    public void Constructor_ShouldInheritFromCustomException() {
        // Arrange & Act
        var exception = new OperationException("Test error");

        // Assert
        exception.Should().BeAssignableTo<CustomException>();
        exception.Error.Code.Should().Be("OPERATION-001");
        exception.Error.Message.Should().Be("Test error");
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

        // Assert
        exception.MessageLog.Should().NotBeNull();
        exception.MessageLog.Method.Should().NotBeNullOrEmpty();
        exception.ToString().Should().Contain("OperationException");
    }
}

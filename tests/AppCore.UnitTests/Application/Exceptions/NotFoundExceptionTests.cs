using OrionSoft.AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Exceptions;

public class NotFoundExceptionTests {
    [Fact]
    public void Constructor_WithDefaultMessage_ShouldUseDefaultMessage() {
        // Arrange & Act
        var exception = new NotFoundException();

        // Assert
        exception.Message.Should().Be("The requested resource could not be found.");
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetMessage() {
        // Arrange & Act
        var exception = new NotFoundException("User not found");

        // Assert
        exception.Message.Should().Be("User not found");
    }

    [Fact]
    public void Constructor_ShouldSetErrorCode() {
        // Arrange & Act
        var exception = new NotFoundException("Product with ID 123 not found");

        // Assert
        exception.MessageLog.Should().NotBeNull();
        var error = exception.Error;
        error.Code.Should().Be("NOT-FOUND-001");
        error.Message.Should().Be("Product with ID 123 not found");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedMessage() {
        // Arrange
        var exception = new NotFoundException("Resource missing");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("NotFoundException");
        result.Should().Contain("Resource missing");
    }

    [Fact]
    public void Constructor_ShouldCaptureCallerInformation() {
        // Arrange & Act
        var exception = new NotFoundException("Item not found");
        var stringRepresentation = exception.ToString();

        // Assert
        stringRepresentation.Should().Contain("Item not found");
        stringRepresentation.Should().Contain("NotFoundException");
    }

    [Fact]
    public void Constructor_WithEntityAndKey_ShouldFormatMessage() {
        // Arrange & Act
        var exception = new NotFoundException("User", 123);

        // Assert
        exception.Message.Should().Be("User with key '123' was not found.");
        exception.Error.Code.Should().Be("NOT-FOUND-002");
    }

    [Fact]
    public void Constructor_WithEntityAndStringKey_ShouldFormatMessage() {
        // Arrange & Act
        var exception = new NotFoundException("Product", (object)"ABC-456");

        // Assert
        exception.Message.Should().Be("Product with key 'ABC-456' was not found.");
        exception.Error.Code.Should().Be("NOT-FOUND-002");
    }

    [Fact]
    public void Constructor_WithEntityAndKey_ShouldCaptureCallerInfo() {
        // Arrange & Act
        var exception = new NotFoundException("Order", 99);

        // Assert
        exception.MessageLog.Method.Should().NotBeNullOrEmpty();
        exception.MessageLog.Path.Should().NotBeNullOrEmpty();
    }
}

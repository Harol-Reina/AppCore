using OrionSoft.AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Exceptions;

public class ConflictExceptionTests {
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage() {
        // Arrange & Act
        var exception = new ConflictException("Resource already exists");

        // Assert
        exception.Message.Should().Be("Resource already exists");
        exception.Error.Code.Should().Be("CONFLICT-001");
    }

    [Fact]
    public void Constructor_ShouldInheritFromCustomException() {
        // Arrange & Act
        var exception = new ConflictException("Conflict");

        // Assert
        exception.Should().BeAssignableTo<CustomException>();
    }

    [Fact]
    public void Constructor_WithEntityAndKey_ShouldFormatMessage() {
        // Arrange & Act
        var exception = new ConflictException("User", (object)"john@test.com");

        // Assert
        exception.Message.Should().Be("User with key 'john@test.com' already exists.");
        exception.Error.Code.Should().Be("CONFLICT-001");
    }

    [Fact]
    public void Constructor_ShouldCaptureCallerInformation() {
        // Arrange & Act
        var exception = new ConflictException("Test conflict");

        // Assert
        exception.MessageLog.Method.Should().NotBeNullOrEmpty();
        exception.MessageLog.Path.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedMessage() {
        // Arrange
        var exception = new ConflictException("Duplicate entry");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("ConflictException");
        result.Should().Contain("Duplicate entry");
    }
}

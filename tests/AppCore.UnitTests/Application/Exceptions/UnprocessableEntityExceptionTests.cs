using OrionSoft.AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Exceptions;

public class UnprocessableEntityExceptionTests {
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage() {
        // Arrange & Act
        var exception = new UnprocessableEntityException("Cannot process entity");

        // Assert
        exception.Message.Should().Be("Cannot process entity");
        exception.Error.Code.Should().Be("UNPROCESSABLE-001");
    }

    [Fact]
    public void Constructor_ShouldInheritFromCustomException() {
        // Arrange & Act
        var exception = new UnprocessableEntityException("Unprocessable");

        // Assert
        exception.Should().BeAssignableTo<CustomException>();
    }

    [Fact]
    public void Constructor_ShouldCaptureCallerInformation() {
        // Arrange & Act
        var exception = new UnprocessableEntityException("Test");

        // Assert
        exception.MessageLog.Method.Should().NotBeNullOrEmpty();
        exception.MessageLog.Path.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedMessage() {
        // Arrange
        var exception = new UnprocessableEntityException("Semantic error");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("UnprocessableEntityException");
        result.Should().Contain("Semantic error");
    }
}

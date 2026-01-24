using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class ForbiddenAccessExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Access denied";

        // Act
        var exception = new ForbiddenAccessException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Exception_ShouldInheritFromException()
    {
        // Arrange & Act
        var exception = new ForbiddenAccessException("test");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Theory]
    [InlineData("Insufficient permissions")]
    [InlineData("Access forbidden")]
    [InlineData("User not authorized for this resource")]
    public void Constructor_WithVariousMessages_ShouldSetMessage(string message)
    {
        // Act
        var exception = new ForbiddenAccessException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedMessage()
    {
        // Arrange
        var exception = new ForbiddenAccessException("Test forbidden message");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
    }
}

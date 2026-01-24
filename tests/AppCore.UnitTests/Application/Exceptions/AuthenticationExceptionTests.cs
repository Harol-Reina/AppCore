using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class AuthenticationExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Authentication failed";

        // Act
        var exception = new AuthenticationException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Exception_ShouldInheritFromException()
    {
        // Arrange & Act
        var exception = new AuthenticationException("test");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Theory]
    [InlineData("Invalid credentials")]
    [InlineData("Token expired")]
    [InlineData("Unauthorized access")]
    public void Constructor_WithVariousMessages_ShouldSetMessage(string message)
    {
        // Act
        var exception = new AuthenticationException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedMessage()
    {
        // Arrange
        var exception = new AuthenticationException("Test auth message");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
    }
}

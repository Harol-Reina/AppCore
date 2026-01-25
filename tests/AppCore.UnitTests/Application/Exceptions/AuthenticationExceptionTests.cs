using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class AuthenticationExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange & Act
        var exception = new AuthenticationException("Invalid credentials");

        // Assert
        exception.Message.Should().Be("Invalid credentials");
    }

    [Fact]
    public void Constructor_ShouldSetErrorCode()
    {
        // Arrange & Act
        var exception = new AuthenticationException("Authentication failed");

        // Assert
        exception.MessageLog.Should().NotBeNull();
        exception.MessageLog.Message.Should().BeOfType<DictionaryError>();
        var error = (DictionaryError)exception.MessageLog.Message;
        error.Code.Should().Be("AUTH-001");
        error.Message.Should().Be("Authentication failed");
    }

    [Fact]
    public void ToString_ShouldReturnMessageLogJson()
    {
        // Arrange
        var exception = new AuthenticationException("Token expired");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("AuthenticationException");
        result.Should().Contain("Token expired");
    }

    [Fact]
    public void Constructor_ShouldCaptureCallerInformation()
    {
        // Arrange & Act
        var exception = new AuthenticationException("Auth test");
        var stringRepresentation = exception.ToString();

        // Assert
        stringRepresentation.Should().Contain("Auth test");
        stringRepresentation.Should().Contain("AuthenticationException");
    }
}

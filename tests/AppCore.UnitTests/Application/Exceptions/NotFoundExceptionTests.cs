using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_WithDefaultMessage_ShouldUseDefaultMessage()
    {
        // Arrange & Act
        var exception = new NotFoundException();

        // Assert
        exception.Message.Should().Be("The requested resource could not be found.");
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetMessage()
    {
        // Arrange & Act
        var exception = new NotFoundException("User not found");

        // Assert
        exception.Message.Should().Be("User not found");
    }

    [Fact]
    public void Constructor_ShouldSetErrorCode()
    {
        // Arrange & Act
        var exception = new NotFoundException("Product with ID 123 not found");

        // Assert
        exception.MessageLog.Should().NotBeNull();
        var error = (DictionaryError)exception.MessageLog.Message;
        error.Code.Should().Be("NOT-FOUND-001");
        error.Message.Should().Be("Product with ID 123 not found");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedMessage()
    {
        // Arrange
        var exception = new NotFoundException("Resource missing");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("NotFoundException");
        result.Should().Contain("Resource missing");
    }

    [Fact]
    public void Constructor_ShouldCaptureCallerInformation()
    {
        // Arrange & Act
        var exception = new NotFoundException("Item not found");
        var stringRepresentation = exception.ToString();

        // Assert
        stringRepresentation.Should().Contain("Item not found");
        stringRepresentation.Should().Contain("NotFoundException");
    }
}

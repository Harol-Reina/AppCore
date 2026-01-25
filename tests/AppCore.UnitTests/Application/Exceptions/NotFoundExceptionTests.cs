using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_WithDefaultMessage_ShouldSetDefaultMessage()
    {
        // Act
        var exception = new NotFoundException();

        // Assert
        exception.Message.Should().Be("The requested resource could not be found.");
        exception.MessageLog.Should().NotBeNull();
        exception.Should().BeAssignableTo<CustomException>();
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetCustomMessage()
    {
        // Arrange
        var customMessage = "User not found";

        // Act
        var exception = new NotFoundException(customMessage);

        // Assert
        exception.Message.Should().Be(customMessage);
        exception.MessageLog.Should().NotBeNull();
        exception.Should().BeAssignableTo<CustomException>();
    }

    [Fact]
    public void Exception_ShouldInheritFromCustomException()
    {
        // Arrange & Act
        var exception = new NotFoundException();

        // Assert
        exception.Should().BeAssignableTo<CustomException>();
    }

    [Fact]
    public void MessageLog_ShouldContainCorrectProperties()
    {
        // Arrange
        var message = "Test not found message";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        exception.MessageLog.Tipo.Should().Be("NotFoundException");
        ((DictionaryError)exception.MessageLog.Message).Message.Should().Be(message);
        exception.MessageLog.Metodo.Should().NotBeNullOrEmpty();
        exception.MessageLog.Path.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToString_ShouldReturnMessageLogString()
    {
        // Arrange
        var exception = new NotFoundException("Test message");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be(exception.MessageLog.ToString());
    }
}

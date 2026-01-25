using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_WithDefaultMessage_ShouldSetDefaultMessageAndStatusCode404()
    {
        // Act
        var exception = new NotFoundException();

        // Assert
        exception.Message.Should().Be("The requested resource could not be found.");
        exception.StatusCode.Should().Be(404);
        exception.MessageLog.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomMessage_ShouldSetCustomMessageAndStatusCode404()
    {
        // Arrange
        var customMessage = "User not found";

        // Act
        var exception = new NotFoundException(customMessage);

        // Assert
        exception.Message.Should().Be(customMessage);
        exception.StatusCode.Should().Be(404);
        exception.MessageLog.Should().NotBeNull();
    }

    [Fact]
    public void Exception_ShouldInheritFromHttpBaseException()
    {
        // Arrange & Act
        var exception = new NotFoundException();

        // Assert
        exception.Should().BeAssignableTo<HttpBaseException>();
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

using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class SerializerExceptionTests
{
    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetProperties()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var message = "Serialization failed";

        // Act
        var exception = new SerializerException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void MessageLog_ShouldBeAccessible()
    {
        // Arrange
        var innerException = new Exception("Test error");
        var exception = new SerializerException("Error", innerException);

        // Act
        var messageLog = exception.MessageLog;

        // Assert
        messageLog.Should().NotBeNull();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var exception = new SerializerException("Test error", new Exception("Inner"));

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("SerializerException");
    }
}

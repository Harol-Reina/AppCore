using OrionSoft.AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Exceptions;

public class ExceptionsTests {

    [Fact]
    public void ApiDBException_WithInnerException_ShouldFormatMessage() {
        // Arrange
        var inner = new Exception("Inner Error");
        var ex = new Exception("Outer Error", inner);

        // Act
        var apiException = new ApiDBException(ex);

        // Assert
        apiException.Should().NotBeNull();
        apiException.Message.Should().Contain("An error occurred validating an operation in the DB");
        apiException.ToString().Should().Contain("Inner Error");
    }

    [Fact]
    public void ApiDBException_WithNestedInnerExceptions_ShouldCollectAllMessages() {
        // Arrange
        var innermost = new Exception("Innermost DB error");
        var middle = new Exception("Middle DB error", innermost);
        var outer = new Exception("Outer DB error", middle);

        // Act
        var apiException = new ApiDBException(outer);

        // Assert
        var text = apiException.ToString();
        text.Should().Contain("Middle DB error");
        text.Should().Contain("Innermost DB error");
    }

    [Fact]
    public void ApiDBException_WithoutInnerException_ShouldNotContainStackTrace() {
        // Arrange
        var ex = new Exception("Simple error");

        // Act
        var apiException = new ApiDBException(ex);

        // Assert
        apiException.ToString().Should().Contain("Simple error");
    }

    [Fact]
    public void SerializerException_WithException_ShouldFormatMessage() {
        // Arrange
        var inner = new Exception("Serialization Failed");

        // Act
        var exception = new SerializerException(inner);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("An error occurred while serializing or deserializing an object");
        exception.MessageLog.Should().NotBeNull();
        exception.MessageLog.Message.GetString().Should().Be("Serialization Failed");
    }

    [Fact]
    public void SerializerException_WithNestedInnerExceptions_ShouldCollectAllMessages() {
        // Arrange
        var innermost = new Exception("Root cause");
        var middle = new Exception("JSON parse error", innermost);
        var outer = new Exception("Serialization failed", middle);

        // Act
        var exception = new SerializerException(outer);

        // Assert
        var text = exception.ToString();
        text.Should().Contain("JSON parse error");
        text.Should().Contain("Root cause");
    }

    [Fact]
    public void SerializerException_ToString_ShouldReturnMessageLogString() {
        // Arrange
        var inner = new Exception("Test error");

        // Act
        var exception = new SerializerException(inner);

        // Assert
        exception.ToString().Should().NotBeNullOrEmpty();
        exception.ToString().Should().Be(exception.MessageLog.ToString());
    }

    [Fact]
    public void SerializerException_WithMessage_ShouldFormatMessage() {
        // Arrange
        var message = "Custom Serialization Error";

        // Act
        var exception = new SerializerException(message);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("An error occurred while serializing or deserializing an object");
        exception.MessageLog.Should().NotBeNull();
        exception.MessageLog.Message.GetString().Should().Be(message);
    }

    [Fact]
    public void MappingException_GetMessageLog_ShouldReturnMessageLog() {
        // Arrange
        var exception = new MappingException("Mapping failed", new InvalidOperationException("Source null"));

        // Act
        var messageLog = exception.GetMessageLog();

        // Assert
        messageLog.Should().NotBeNull();
        messageLog.Message.GetString().Should().Be("Mapping failed");
        messageLog.Type.Should().Be(nameof(MappingException));
    }
}

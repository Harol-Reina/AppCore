using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

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
    public void SerializerException_WithException_ShouldFormatMessage() {
        // Arrange
        var inner = new Exception("Serialization Failed");

        // Act
        var exception = new SerializerException(inner);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("An error occurred while serializing or deserializing an object");
        exception.MessageLog.Should().NotBeNull();
        exception.MessageLog.Message.Should().Be("Serialization Failed");
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
        exception.MessageLog.Message.Should().Be(message);
    }
}

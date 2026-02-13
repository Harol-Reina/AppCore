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
        apiException.Should().BeAssignableTo<CustomException>();
        apiException.Message.Should().Contain("An error occurred validating an operation in the DB");
        apiException.Error.Code.Should().Be("API-DB-001");
        apiException.Error.Exception.Should().Contain("Inner Error");
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
        apiException.Error.Exception.Should().Contain("Middle DB error");
        apiException.Error.Exception.Should().Contain("Innermost DB error");
    }

    [Fact]
    public void ApiDBException_WithoutInnerException_ShouldHaveExceptionMessage() {
        // Arrange
        var ex = new Exception("Simple error");

        // Act
        var apiException = new ApiDBException(ex);

        // Assert
        apiException.Error.Exception.Should().Contain("Simple error");
    }

    [Fact]
    public void SerializerException_WithException_ShouldFormatMessage() {
        // Arrange
        var inner = new Exception("Serialization Failed");

        // Act
        var exception = new SerializerException(inner);

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeAssignableTo<CustomException>();
        exception.Message.Should().Contain("An error occurred while serializing or deserializing an object");
        exception.MessageLog.Should().NotBeNull();
        exception.Error.Code.Should().Be("SERIALIZER-001");
        exception.Error.Exception.Should().Contain("Serialization Failed");
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
        exception.Error.Exception.Should().Contain("JSON parse error");
        exception.Error.Exception.Should().Contain("Root cause");
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
        exception.Error.Code.Should().Be("SERIALIZER-002");
        exception.Error.ProviderMessage.Should().NotBeNull();
    }

    [Fact]
    public void MappingException_ShouldInheritFromCustomException() {
        // Arrange
        var exception = new MappingException("Mapping failed", new InvalidOperationException("Source null"));

        // Act & Assert
        exception.Should().BeAssignableTo<CustomException>();
        exception.MessageLog.Should().NotBeNull();
        exception.Error.Code.Should().Be("MAPPING-001");
        exception.Error.Message.Should().Be("Mapping failed");
    }

    [Fact]
    public void MappingException_WithInnerException_ShouldPopulateErrors() {
        // Arrange & Act
        var exception = new MappingException("Mapping failed", new InvalidOperationException("Source null"));

        // Assert
        exception.Errors.Should().ContainKey("InnerExceptionType");
        exception.Errors["InnerExceptionType"].Should().Be("InvalidOperationException");
        exception.Errors["InnerExceptionMessage"].Should().Be("Source null");
    }
}

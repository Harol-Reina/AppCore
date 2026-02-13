using OrionSoft.AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Exceptions;

public class CustomExceptionTests {
    [Fact]
    public void Constructor_WithDictionaryError_ShouldSetMessageLog() {
        // Arrange
        var error = new DictionaryError("TEST-001", "Test error message");

        // Act
        var exception = new CustomException(error);

        // Assert
        exception.Message.Should().Be("Test error message");
        exception.MessageLog.Should().NotBeNull();
        exception.Error.Should().Be(error);
    }

    [Fact]
    public void Constructor_ShouldCaptureCallerInformation() {
        // Arrange
        var error = new DictionaryError("TEST-002", "Another test");

        // Act
        var exception = new CustomException(error);
        var stringRepresentation = exception.ToString();

        // Assert
        exception.MessageLog.Method.Should().NotBeNullOrEmpty();
        exception.MessageLog.Path.Should().NotBeNullOrEmpty();
        stringRepresentation.Should().Contain("Another test");
    }

    [Fact]
    public void ToString_ShouldReturnMessageLogString() {
        // Arrange
        var error = new DictionaryError("TEST-003", "Third test error");
        var exception = new CustomException(error);

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("CustomException");
        result.Should().Contain("Third test error");
    }

    [Fact]
    public void MessageLog_ShouldContainExceptionType() {
        // Arrange
        var error = new DictionaryError("TEST-004", "Type test");

        // Act
        var exception = new CustomException(error);

        // Assert
        exception.MessageLog.Type.Should().Be("CustomException");
    }

    [Fact]
    public void Constructor_WithComplexError_ShouldPreserveAllData() {
        // Arrange
        var innerException = new InvalidOperationException("Inner");
        var error = new DictionaryError("TEST-005", "Complex error", """{"key":"value"}""", innerException.ToString());

        // Act
        var exception = new CustomException(error);

        // Assert
        exception.Message.Should().Be("Complex error");
        exception.Error.Code.Should().Be("TEST-005");
        exception.Error.ProviderMessage.Should().NotBeNull();
        exception.Error.Exception.Should().Contain("Inner");
    }
}

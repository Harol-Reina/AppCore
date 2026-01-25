using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class DictionaryErrorTests {
    [Fact]
    public void Constructor_WithCodeAndMessage_ShouldSetProperties() {
        // Arrange & Act
        var error = new DictionaryError("ERR-001", "Error message");

        // Assert
        error.Code.Should().Be("ERR-001");
        error.Message.Should().Be("Error message");
        error.ProviderMessage.Should().BeNull();
        error.Exception.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithProviderMessage_ShouldParseJson() {
        // Arrange & Act
        var error = new DictionaryError("ERR-002", "Error with provider", """{"detail":"Additional info"}""");

        // Assert
        error.Code.Should().Be("ERR-002");
        error.Message.Should().Be("Error with provider");
        error.ProviderMessage.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithException_ShouldSetException() {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var error = new DictionaryError("ERR-003", "Error with exception", exception: innerException);

        // Assert
        error.Code.Should().Be("ERR-003");
        error.Message.Should().Be("Error with exception");
        error.Exception.Should().Be(innerException);
    }

    [Fact]
    public void Constructor_WithNullProviderMessage_ShouldNotSetProviderMessage() {
        // Arrange & Act
        var error = new DictionaryError("ERR-004", "Error message", null);

        // Assert
        error.ProviderMessage.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyProviderMessage_ShouldNotSetProviderMessage() {
        // Arrange & Act
        var error = new DictionaryError("ERR-005", "Error message", "");

        // Assert
        error.ProviderMessage.Should().BeNull();
    }

    [Fact]
    public void Record_ShouldSupportWithExpressions() {
        // Arrange
        var original = new DictionaryError("ERR-006", "Original message");

        // Act
        var modified = original with { Message = "Modified message" };

        // Assert
        modified.Code.Should().Be("ERR-006");
        modified.Message.Should().Be("Modified message");
        original.Message.Should().Be("Original message"); // Original unchanged
    }

    [Fact]
    public void Record_ShouldSupportEqualityComparison() {
        // Arrange
        var error1 = new DictionaryError("ERR-007", "Same error");
        var error2 = new DictionaryError("ERR-007", "Same error");
        var error3 = new DictionaryError("ERR-008", "Different error");

        // Assert
        error1.Should().Be(error2);
        error1.Should().NotBe(error3);
    }

    [Fact]
    public void DefaultConstructor_ShouldCreateEmptyInstance() {
        // Arrange & Act
        var error = new DictionaryError { Code = "TEST", Message = "Test message" };

        // Assert
        error.Code.Should().Be("TEST");
        error.Message.Should().Be("Test message");
    }
}

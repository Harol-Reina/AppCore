using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class MappingExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Mapping failed";

        // Act
        var exception = new MappingException(message);

        // Assert
        exception.Message.Should().Contain(message);
    }

    [Fact]
    public void Constructor_WithErrors_ShouldSetErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Entity", new[] { "Cannot map property" } }
        };

        // Act
        var exception = new MappingException("Error", errors);

        // Assert
        exception.Errors.Should().NotBeNull();
        exception.Errors.Should().ContainKey("Entity");
    }

    [Fact]
    public void Constructor_WithCallerInfo_ShouldIncludeInfo()
    {
        // Arrange & Act
        var exception = new MappingException("Test error", "TestMethod", "Test.cs", 10);

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Contain("Test error");
    }

    [Fact]
    public void StatusCode_ShouldBe500()
    {
        // Arrange
        var exception = new MappingException("Error");

        // Act
        var statusCode = exception.StatusCode;

        // Assert
        statusCode.Should().Be(500);
    }
}

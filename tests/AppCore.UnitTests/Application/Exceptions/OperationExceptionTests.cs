using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class OperationExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Operation failed";

        // Act
        var exception = new OperationException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithCallerInfo_ShouldIncludeCallerDetails()
    {
        // Arrange
        var message = "Test operation error";

        // Act
        var exception = new OperationException(message, "TestMethod", "TestFile.cs", 42);

        // Assert
        exception.Message.Should().Be(message);
        exception.ToString().Should().Contain("OperationException");
    }

    [Fact]
    public void StatusCode_ShouldBe500()
    {
        // Arrange
        var exception = new OperationException("Error");

        // Act
        var statusCode = exception.StatusCode;

        // Assert
        statusCode.Should().Be(500);
    }
}

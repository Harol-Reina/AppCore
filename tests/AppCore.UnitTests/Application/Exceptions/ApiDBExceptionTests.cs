using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class ApiDBExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Database error occurred";

        // Act
        var exception = new ApiDBException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        var message = "Database connection failed";
        var innerException = new InvalidOperationException("Connection timeout");

        // Act
        var exception = new ApiDBException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ToString_ShouldContainErrorDetails()
    {
        // Arrange
        var exception = new ApiDBException("Test error");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("ApiDBException");
    }
}

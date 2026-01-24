using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class HttpBaseExceptionTests
{
    [Fact]
    public void Constructor_WithMessageAndStatusCode_ShouldSetProperties()
    {
        // Arrange
        var message = "Test error message";
        var statusCode = 500;

        // Act
        var exception = new HttpBaseException(message, statusCode);

        // Assert
        exception.Message.Should().Be(message);
        exception.StatusCode.Should().Be(statusCode);
    }

    [Theory]
    [InlineData(400, "Bad Request")]
    [InlineData(401, "Unauthorized")]
    [InlineData(403, "Forbidden")]
    [InlineData(404, "Not Found")]
    [InlineData(500, "Internal Server Error")]
    public void Constructor_WithDifferentStatusCodes_ShouldSetCorrectStatusCode(int statusCode, string message)
    {
        // Act
        var exception = new HttpBaseException(message, statusCode);

        // Assert
        exception.StatusCode.Should().Be(statusCode);
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Exception_ShouldInheritFromException()
    {
        // Arrange & Act
        var exception = new HttpBaseException("test", 400);

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }
}

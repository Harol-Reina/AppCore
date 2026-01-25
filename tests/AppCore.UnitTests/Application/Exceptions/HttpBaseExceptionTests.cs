using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class HttpBaseExceptionTests
{
    [Fact]
    public void Constructor_WithStatusCode_ShouldSetStatusCode()
    {
        // Arrange & Act
        var exception = new HttpBaseException("Not found", 404);

        // Assert
        exception.StatusCode.Should().Be(404);
    }

    [Fact]
    public void Constructor_ShouldInheritFromCustomException()
    {
        // Arrange & Act
        var exception = new HttpBaseException("Server error", 500);

        // Assert
        exception.Should().BeAssignableTo<CustomException>();
    }

    [Fact]
    public void Constructor_ShouldCreateDictionaryErrorWithStatusCode()
    {
        // Arrange & Act
        var exception = new HttpBaseException("Unauthorized", 401, "TestMethod", "/path/test.cs", 42);

        // Assert
        exception.StatusCode.Should().Be(401);
        exception.MessageLog.Should().NotBeNull();
        exception.MessageLog.Message.Should().BeOfType<DictionaryError>();
    }

    [Fact]
    public void Constructor_WithCallerInfo_ShouldPopulateMessageLog()
    {
        // Arrange & Act
        var exception = new HttpBaseException("Bad request", 400, "MyMethod", "/src/file.cs", 10);

        // Assert
        var error = exception.MessageLog.Message as DictionaryError;
        error.Should().NotBeNull();
        error!.Code.Should().Be("HTTP-400");
        error.Message.Should().Be("Bad request");
    }

    [Theory]
    [InlineData(200, "HTTP-200")]
    [InlineData(404, "HTTP-404")]
    [InlineData(500, "HTTP-500")]
    public void Constructor_WithDifferentStatusCodes_ShouldCreateCorrectErrorCode(int statusCode, string expectedCode)
    {
        // Arrange & Act
        var exception = new HttpBaseException("Test", statusCode);

        // Assert
        var error = exception.MessageLog.Message as DictionaryError;
        error!.Code.Should().Be(expectedCode);
    }
}

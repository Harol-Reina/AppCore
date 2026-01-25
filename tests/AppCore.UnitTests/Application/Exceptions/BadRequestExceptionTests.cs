using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class BadRequestExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange & Act
        var exception = new BadRequestException("Invalid input data");

        // Assert
        exception.Message.Should().Be("Invalid input data");
    }

    [Fact]
    public void Constructor_ShouldSetErrorCode()
    {
        // Arrange & Act
        var exception = new BadRequestException("Bad request");

        // Assert
        exception.MessageLog.Should().NotBeNull();
        var error = (DictionaryError)exception.MessageLog.Message;
        error.Code.Should().Be("BAD-REQ-001");
        error.Message.Should().Be("Bad request");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedMessage()
    {
        // Arrange
        var exception = new BadRequestException("Invalid parameter format");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().Contain("BadRequestException");
        result.Should().Contain("Invalid parameter format");
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ShouldStillWork()
    {
        // Arrange & Act
        var exception = new BadRequestException(string.Empty);

        // Assert
        exception.Message.Should().BeEmpty();
        var error = (DictionaryError)exception.MessageLog.Message;
        error.Code.Should().Be("BAD-REQ-001");
    }
}

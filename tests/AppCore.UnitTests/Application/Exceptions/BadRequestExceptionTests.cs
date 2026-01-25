using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

public class BadRequestExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Invalid request data";

        // Act
        var exception = new BadRequestException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.Should().BeAssignableTo<CustomException>();
    }

    [Fact]
    public void Exception_ShouldInheritFromCustomException()
    {
        // Arrange & Act
        var exception = new BadRequestException("test");

        // Assert
        exception.Should().BeAssignableTo<CustomException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Valid message")]
    [InlineData("Message with special characters: !@#$%^&*()")]
    public void Constructor_WithVariousMessages_ShouldSetMessage(string message)
    {
        // Act
        var exception = new BadRequestException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.Should().BeAssignableTo<CustomException>();
    }
}

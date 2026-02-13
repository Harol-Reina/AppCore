using OrionSoft.AppCore.Application.Wrappers;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Wrappers;

public class ResponseTests {
    [Fact]
    public void Success_WithMessageAndData_ShouldCreateSuccessResponse() {
        // Arrange
        var message = "Operation completed successfully";
        var data = new { Id = 1, Name = "Test" };

        // Act
        var response = Response<object>.Success(message, data);

        // Assert
        response.Should().NotBeNull();
        response.Succeeded.Should().BeTrue();
        response.Message.Should().Be(message);
        response.Data.Should().Be(data);
    }

    [Fact]
    public void Success_WithDataOnly_ShouldCreateResponseWithData() {
        // Arrange
        var data = new { Id = 1, Name = "Test" };

        // Act
        var response = Response<object>.Success(data);

        // Assert
        response.Should().NotBeNull();
        response.Succeeded.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Message.Should().BeNull();
    }

    [Fact]
    public void Failure_WithMessage_ShouldCreateFailureResponse() {
        // Arrange
        var errorMessage = "Operation failed";

        // Act
        var response = Response<object>.Failure(errorMessage);

        // Assert
        response.Should().NotBeNull();
        response.Succeeded.Should().BeFalse();
        response.Message.Should().Be(errorMessage);
        response.Data.Should().BeNull();
    }
}

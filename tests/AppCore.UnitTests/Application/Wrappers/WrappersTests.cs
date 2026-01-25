using AppCore.Application.Wrappers;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Wrappers;

public class WrappersTests {

    [Fact]
    public void CustomErrorResponse_ShouldInitializeCorrectly() {
        // Arrange
        var errorObj = new { code = "TEST", detail = "Details" };

        // Act
        var response = new CustomErrorResponse(errorObj);

        // Assert
        response.Should().NotBeNull();
        response.Error.Should().Be(errorObj);
    }

    [Fact]
    public void MappingErrorResponse_ShouldInitializeCorrectly() {
        // Arrange
        var title = "Validation Failed";
        var errors = new Dictionary<string, string> {
            { "Field1", "Error1" },
            { "Field2", "Error2" }
        };

        // Act
        var response = new MappingErrorResponse(title, errors);

        // Assert
        response.Should().NotBeNull();
        response.Title.Should().Be(title);
        response.Errors.Should().BeEquivalentTo(errors);
        response.Errors.Count.Should().Be(2);
    }

    [Fact]
    public void ErrorResponse_ShouldInitializeCorrectly() {
        // Arrange
        var message = "An error occurred";

        // Act
        var response = new ErrorResponse(message);

        // Assert
        response.Should().NotBeNull();
        response.Message.Should().Be(message);
    }
}

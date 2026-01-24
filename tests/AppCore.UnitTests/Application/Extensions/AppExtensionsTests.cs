using AppCore.Application.Extensions;
using AppCore.Application.Wrappers;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Extensions;

public class AppExtensionsTests {
    [Fact]
    public void Success_WithMessageAndData_ShouldReturnSuccessResponse() {
        // Arrange
        var response = new Response<string>();
        var message = "Operation successful";
        var data = "test data";

        // Act
        var result = response.Success(message, data);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be(message);
        result.Data.Should().Be(data);
    }

    [Fact]
    public void Success_WithDataOnly_ShouldReturnSuccessResponseWithData() {
        // Arrange
        var response = new Response<int>();
        var data = 42;

        // Act
        var result = response.Success(data: data);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().Be(data);
        result.Message.Should().BeNull();
    }

    [Fact]
    public void Success_WithMessageOnly_ShouldReturnSuccessResponseWithMessage() {
        // Arrange
        var response = new Response<object>();
        var message = "Success message";

        // Act
        var result = response.Success(message);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be(message);
        result.Data.Should().BeNull();
    }

    [Fact]
    public void Success_WithNullValues_ShouldHandleNullsCorrectly() {
        // Arrange
        var response = new Response<string>();

        // Act
        var result = response.Success(null, null);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().BeNull();
        result.Data.Should().BeNull();
    }

    [Fact]
    public void Failure_WithMessage_ShouldReturnFailureResponse() {
        // Arrange
        var response = new Response<string>();
        var errorMessage = "Operation failed";

        // Act
        var result = response.Failure(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be(errorMessage);
        result.Data.Should().BeNull();
    }

    [Fact]
    public void Failure_WithEmptyMessage_ShouldReturnResponseWithEmptyMessage() {
        // Arrange
        var response = new Response<int>();
        var errorMessage = "";

        // Act
        var result = response.Failure(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be("");
        result.Data.Should().Be(default(int));
    }

    [Fact]
    public void Success_WithComplexObject_ShouldWorkCorrectly() {
        // Arrange
        var response = new Response<TestComplexObject>();
        var message = "Complex operation successful";
        var data = new TestComplexObject { Id = 1, Name = "Test", IsActive = true };

        // Act
        var result = response.Success(message, data);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Be(message);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Name.Should().Be("Test");
        result.Data.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Extensions_ShouldReturnNewInstancesNotModifyOriginal() {
        // Arrange
        var originalResponse = new Response<string>("original", "original data");

        // Act
        var successResult = originalResponse.Success("new message", "new data");
        var failureResult = originalResponse.Failure("failure message");

        // Assert
        // Original response should remain unchanged
        originalResponse.Message.Should().Be("original");
        originalResponse.Data.Should().Be("original data");

        // New instances should have new values
        successResult.Message.Should().Be("new message");
        successResult.Data.Should().Be("new data");

        failureResult.Message.Should().Be("failure message");
        failureResult.Data.Should().BeNull();
    }

    [Fact]
    public void Success_WithGenericTypes_ShouldWorkWithDifferentTypes() {
        // Arrange & Act
        var stringResponse = new Response<string>().Success("message", "data");
        var intResponse = new Response<int>().Success("number", 123);
        var boolResponse = new Response<bool>().Success("boolean", true);

        // Assert
        stringResponse.Data.Should().Be("data");
        intResponse.Data.Should().Be(123);
        boolResponse.Data.Should().BeTrue();
    }
}

public class TestComplexObject {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

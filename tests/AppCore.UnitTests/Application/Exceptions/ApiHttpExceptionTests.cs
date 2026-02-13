using OrionSoft.AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Exceptions;

public class ApiHttpExceptionTests {
    [Fact]
    public void Constructor_WithSimpleException_ShouldNotThrow() {
        // Arrange & Act
        var act = () => {
            try {
                throw new Exception("Connection failed");
            } catch (Exception ex) {
                return new ApiHttpException(ex);
            }
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ShouldSetCorrectMessage() {
        // Arrange & Act
        try {
            throw new InvalidOperationException("Test error");
        } catch (Exception ex) {
            var exception = new ApiHttpException(ex);

            // Assert
            exception.Message.Should().Be("The connection to the requested URL cannot be made.");
            exception.Should().BeAssignableTo<CustomException>();
            exception.MessageLog.Should().NotBeNull();
            exception.Error.Code.Should().Be("API-HTTP-001");
        }
    }

    [Fact]
    public void ToString_ShouldReturnMessageLogString() {
        // Arrange & Act
        try {
            throw new Exception("Test");
        } catch (Exception ex) {
            var exception = new ApiHttpException(ex);
            var result = exception.ToString();

            // Assert
            result.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void MessageLog_ShouldContainExceptionType() {
        // Arrange & Act
        try {
            throw new InvalidOperationException("Test exception");
        } catch (Exception ex) {
            var exception = new ApiHttpException(ex, "MyMethod");

            // Assert
            exception.MessageLog.Type.Should().Be("ApiHttpException");
            exception.MessageLog.Method.Should().Be("MyMethod");
        }
    }

    [Fact]
    public void Constructor_ShouldCaptureInnerExceptionMessages() {
        // Arrange
        var inner = new Exception("Inner cause");
        var outer = new Exception("Outer cause", inner);

        // Act
        var exception = new ApiHttpException(outer);

        // Assert
        exception.Error.Exception.Should().Contain("Outer cause");
        exception.Error.Exception.Should().Contain("Inner cause");
    }
}

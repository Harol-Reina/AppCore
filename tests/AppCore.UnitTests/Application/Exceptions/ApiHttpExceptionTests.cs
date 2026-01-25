using AppCore.Application.Exceptions;
using FluentAssertions;
using Xunit;

namespace AppCore.UnitTests.Application.Exceptions;

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
            exception.MessageLog.Should().NotBeNull();
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
            exception.MessageLog.Tipo.Should().Be("ApiHttpException");
            exception.MessageLog.Metodo.Should().Be("MyMethod");
        }
    }
}

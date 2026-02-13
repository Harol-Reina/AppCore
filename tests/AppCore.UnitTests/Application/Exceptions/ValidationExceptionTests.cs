using OrionSoft.AppCore.Application.Exceptions;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Application.Exceptions;

public class ValidationExceptionTests {
    [Fact]
    public void Constructor_Default_ShouldSetErrorCode() {
        // Act
        var exception = new ValidationException();

        // Assert
        var error = exception.Error;
        error.Code.Should().Be("VAL-000");
        error.Message.Should().Be("Validation failed");
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage() {
        // Act
        var exception = new ValidationException("Custom validation error");

        // Assert
        exception.Message.Should().Be("Custom validation error");
        var error = exception.Error;
        error.Code.Should().Be("VAL-001");
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithValidationFailures_ShouldGroupErrors() {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Email", "Email is required"),
            new("Email", "Email must be valid"),
            new("Password", "Password is too short")
        };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        var error = exception.Error;
        error.Code.Should().Be("VAL-002");
        exception.Errors.Should().HaveCount(2);
        exception.Errors["Email"].Should().HaveCount(2);
        exception.Errors["Password"].Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_WithPropertyAndMessage_ShouldCreateSingleError() {
        // Act
        var exception = new ValidationException("Username", "Username is already taken");

        // Assert
        var error = exception.Error;
        error.Code.Should().Be("VAL-003");
        exception.Errors.Should().ContainKey("Username");
        exception.Errors["Username"].Should().Contain("Username is already taken");
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldWrapException() {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new ValidationException(innerException);

        // Assert
        var error = exception.Error;
        error.Code.Should().Be("VAL-004");
        exception.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithValidationProblemDetails_ShouldCopyErrors() {
        // Arrange
        var problemDetails = new ValidationProblemDetails {
            Errors =
            {
                { "Field1", new[] { "Error 1", "Error 2" } },
                { "Field2", new[] { "Error 3" } }
            }
        };

        // Act
        var exception = new ValidationException(problemDetails);

        // Assert
        var error = exception.Error;
        error.Code.Should().Be("VAL-005");
        exception.Errors.Should().HaveCount(2);
        exception.Errors["Field1"].Should().HaveCount(2);
        exception.Errors["Field2"].Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_WithEmptyFailures_ShouldHaveEmptyErrors() {
        // Arrange
        var failures = new List<ValidationFailure>();

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Errors.Should().BeEmpty();
    }
}

using AppCore.Application.Exceptions;
using FluentValidation.Results;
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Mvc;

namespace AppCore.UnitTests.Application.Exceptions;

public class ValidationExceptionTests
{
    [Fact]
    public void Constructor_WithValidationFailures_ShouldGroupErrors()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Email", "Email is invalid"),
            new("Name", "Name must be at least 2 characters")
        };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Errors.Should().HaveCount(2);
        exception.Errors["Name"].Should().HaveCount(2);
        exception.Errors["Name"].Should().Contain("Name is required");
        exception.Errors["Name"].Should().Contain("Name must be at least 2 characters");
        exception.Errors["Email"].Should().HaveCount(1);
        exception.Errors["Email"].Should().Contain("Email is invalid");
    }

    [Fact]
    public void Constructor_WithSinglePropertyError_ShouldCreateSingleError()
    {
        // Arrange
        var propertyName = "Username";
        var errorMessage = "Username is required";

        // Act
        var exception = new ValidationException(propertyName, errorMessage);

        // Assert
        exception.Errors.Should().HaveCount(1);
        exception.Errors[propertyName].Should().HaveCount(1);
        exception.Errors[propertyName][0].Should().Be(errorMessage);
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldCreateErrorFromException()
    {
        // Arrange
        var innerException = new ArgumentException("Invalid argument");
        var outerException = new InvalidOperationException("Operation failed", innerException)
        {
            Source = "TestSource"  // Ensure Source is not null
        };

        // Act
        var exception = new ValidationException(outerException);

        // Assert
        exception.Errors.Should().HaveCount(1);
        exception.Errors.Should().ContainKey("TestSource");
        exception.Errors["TestSource"][0].Should().Be(outerException.Message);
    }

    [Fact]
    public void Constructor_WithValidationProblemDetails_ShouldUseExistingErrors()
    {
        // Arrange
        var problemDetails = new ValidationProblemDetails
        {
            Errors = new Dictionary<string, string[]>
            {
                {"Field1", new[] {"Error1", "Error2"}},
                {"Field2", new[] {"Error3"}}
            }
        };

        // Act
        var exception = new ValidationException(problemDetails);

        // Assert
        exception.Errors.Should().BeEquivalentTo(problemDetails.Errors);
    }

    [Fact]
    public void Constructor_WithEmptyFailures_ShouldCreateEmptyErrors()
    {
        // Arrange
        var failures = new List<ValidationFailure>();

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Exception_ShouldInheritFromException()
    {
        // Arrange & Act
        var exception = new ValidationException("test", "error");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedMessage()
    {
        // Arrange
        var exception = new ValidationException("TestProperty", "Test error message");

        // Act
        var result = exception.ToString();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("ValidationException");
    }

    [Fact]
    public void Constructor_WithNullPropertyName_ShouldHandleGracefully()
    {
        // Act
        var exception = new ValidationException(string.Empty, "Error message");

        // Assert
        exception.Errors.Should().HaveCount(1);
        exception.Errors.Should().ContainKey("");
    }

    [Fact]
    public void Constructor_WithMultipleFailuresForSameProperty_ShouldGroupCorrectly()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("Password", "Password is required"),
            new("Password", "Password must be at least 8 characters"),
            new("Password", "Password must contain uppercase letter")
        };

        // Act
        var exception = new ValidationException(failures);

        // Assert
        exception.Errors.Should().HaveCount(1);
        exception.Errors["Password"].Should().HaveCount(3);
        exception.Errors["Password"].Should().Contain("Password is required");
        exception.Errors["Password"].Should().Contain("Password must be at least 8 characters");
        exception.Errors["Password"].Should().Contain("Password must contain uppercase letter");
    }
}

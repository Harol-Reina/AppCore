using AppCore.Application.Behaviours;
using AppCore.Application.Exceptions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;
using AppValidationException = AppCore.Application.Exceptions.ValidationException;

namespace AppCore.UnitTests.Application.Behaviours;

// Test request and response for testing
public class TestRequest : IRequest<TestResponse> {
    public string? Name { get; set; }
    public int Age { get; set; }
}

public class TestResponse {
    public string? Message { get; set; }
}

public class TestRequestValidator : AbstractValidator<TestRequest> {
    public TestRequestValidator() {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");

        RuleFor(x => x.Age)
            .GreaterThan(0)
            .WithMessage("Age must be greater than 0");
    }
}

public class ValidationBehaviourTests {
    [Fact]
    public async Task Handle_WithNoValidators_ShouldProceedToNext() {
        // Arrange
        var validators = new List<IValidator<TestRequest>>();
        var behaviour = new ValidationBehaviour<TestRequest, TestResponse>(validators);
        var request = new TestRequest { Name = "John", Age = 25 };
        var expectedResponse = new TestResponse { Message = "Success" };

        var nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        var result = await behaviour.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldProceedToNext() {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new TestRequestValidator() };
        var behaviour = new ValidationBehaviour<TestRequest, TestResponse>(validators);
        var request = new TestRequest { Name = "John", Age = 25 };
        var expectedResponse = new TestResponse { Message = "Success" };

        var nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        var result = await behaviour.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowValidationException() {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new TestRequestValidator() };
        var behaviour = new ValidationBehaviour<TestRequest, TestResponse>(validators);
        var request = new TestRequest { Name = "", Age = -1 }; // Invalid request

        var nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        nextMock.Setup(x => x()).ReturnsAsync(new TestResponse());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppValidationException>(
            () => behaviour.Handle(request, nextMock.Object, CancellationToken.None));

        exception.Errors.Should().HaveCount(2);
        exception.Errors["Name"].Should().Contain("Name is required");
        exception.Errors["Age"].Should().Contain("Age must be greater than 0");

        nextMock.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithPartiallyInvalidRequest_ShouldThrowValidationException() {
        // Arrange
        var validators = new List<IValidator<TestRequest>> { new TestRequestValidator() };
        var behaviour = new ValidationBehaviour<TestRequest, TestResponse>(validators);
        var request = new TestRequest { Name = "John", Age = -1 }; // Partially invalid

        var nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        nextMock.Setup(x => x()).ReturnsAsync(new TestResponse());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppValidationException>(
            () => behaviour.Handle(request, nextMock.Object, CancellationToken.None));

        exception.Errors.Should().HaveCount(1);
        exception.Errors["Age"].Should().Contain("Age must be greater than 0");

        nextMock.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldAggregateValidationErrors() {
        // Arrange
        var validator1 = new Mock<IValidator<TestRequest>>();
        var validator2 = new Mock<IValidator<TestRequest>>();

        var failures1 = new List<ValidationFailure>
        {
            new("Name", "Name validation error from validator 1")
        };

        var failures2 = new List<ValidationFailure>
        {
            new("Age", "Age validation error from validator 2")
        };

        validator1.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new ValidationResult(failures1));

        validator2.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(new ValidationResult(failures2));

        var validators = new List<IValidator<TestRequest>> { validator1.Object, validator2.Object };
        var behaviour = new ValidationBehaviour<TestRequest, TestResponse>(validators);
        var request = new TestRequest { Name = "John", Age = 25 };

        var nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppValidationException>(
            () => behaviour.Handle(request, nextMock.Object, CancellationToken.None));

        exception.Errors.Should().HaveCount(2);
        exception.Errors["Name"].Should().Contain("Name validation error from validator 1");
        exception.Errors["Age"].Should().Contain("Age validation error from validator 2");

        nextMock.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToValidators() {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), cancellationToken))
                     .ReturnsAsync(new ValidationResult());

        var validators = new List<IValidator<TestRequest>> { validatorMock.Object };
        var behaviour = new ValidationBehaviour<TestRequest, TestResponse>(validators);
        var request = new TestRequest { Name = "John", Age = 25 };
        var expectedResponse = new TestResponse { Message = "Success" };

        var nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        var result = await behaviour.Handle(request, nextMock.Object, cancellationToken);

        // Assert
        validatorMock.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), cancellationToken), Times.Once);
        result.Should().Be(expectedResponse);
    }
}

using OrionSoft.AppCore.Application.Exceptions;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace OrionSoft.AppCore.SpecFlow.StepDefinitions;

[Binding]
public class ExceptionHandlingStepDefinitions {
    private Exception? _thrownException;
    private string? _errorMessage;
    private Dictionary<string, string>? _errorDetails;

    [Given(@"I am working with the AppCore exception system")]
    public void GivenIAmWorkingWithTheAppCoreExceptionSystem() {
        // Initialize context for exception testing
    }

    [Given(@"I have invalid input data")]
    public void GivenIHaveInvalidInputData() {
        _errorMessage = "Validation failed for the provided input";
        _errorDetails = new Dictionary<string, string>
        {
            { "Name", "Name is required" },
            { "Email", "Email format is invalid" }
        };
    }

    [Given(@"I am looking for an entity that doesn't exist")]
    public void GivenIAmLookingForAnEntityThatDoesntExist() {
        _errorMessage = "User with ID 999 was not found";
    }

    [Given(@"I receive malformed request data")]
    public void GivenIReceiveMalformedRequestData() {
        _errorMessage = "The request data format is invalid";
    }

    [Given(@"I have an authentication failure")]
    public void GivenIHaveAnAuthenticationFailure() {
        _errorMessage = "Authentication failed: Invalid credentials";
    }

    [Given(@"I have various AppCore exceptions")]
    public void GivenIHaveVariousAppCoreExceptions() {
        // Will be used for hierarchy testing
    }

    [When(@"I create a ValidationException with error details")]
    public void WhenICreateAValidationExceptionWithErrorDetails() {
        _thrownException = new ValidationException("ValidationError", _errorMessage!);
    }

    [When(@"I create a NotFoundException with entity details")]
    public void WhenICreateANotFoundExceptionWithEntityDetails() {
        _thrownException = new NotFoundException(_errorMessage!);
    }

    [When(@"I create a BadRequestException with error details")]
    public void WhenICreateABadRequestExceptionWithErrorDetails() {
        _thrownException = new BadRequestException(_errorMessage!);
    }

    [When(@"I create an AuthenticationException with details")]
    public void WhenICreateAnAuthenticationExceptionWithDetails() {
        _thrownException = new AuthenticationException(_errorMessage!);
    }

    [When(@"I check their inheritance chain")]
    public void WhenICheckTheirInheritanceChain() {
        // This step is for verification in Then steps
    }

    [Then(@"the exception should have the validation message")]
    public void ThenTheExceptionShouldHaveTheValidationMessage() {
        _thrownException.Should().NotBeNull();
        _thrownException!.Message.Should().Be(_errorMessage);
    }

    [Then(@"the exception should be of type ValidationException")]
    public void ThenTheExceptionShouldBeOfTypeValidationException() {
        _thrownException.Should().NotBeNull();
        _thrownException.Should().BeOfType<ValidationException>();
    }

    [Then(@"the exception should inherit from CustomException")]
    public void ThenTheExceptionShouldInheritFromCustomException() {
        _thrownException.Should().NotBeNull();
        _thrownException.Should().BeAssignableTo<CustomException>();
    }

    [Then(@"the exception should have a descriptive message")]
    public void ThenTheExceptionShouldHaveADescriptiveMessage() {
        _thrownException.Should().NotBeNull();
        _thrownException!.Message.Should().Be(_errorMessage);
    }

    [Then(@"the exception should be of type NotFoundException")]
    public void ThenTheExceptionShouldBeOfTypeNotFoundException() {
        _thrownException.Should().NotBeNull();
        _thrownException.Should().BeOfType<NotFoundException>();
    }

    [Then(@"the exception should have the error message")]
    public void ThenTheExceptionShouldHaveTheErrorMessage() {
        _thrownException.Should().NotBeNull();
        _thrownException!.Message.Should().Be(_errorMessage);
    }

    [Then(@"the exception should be of type BadRequestException")]
    public void ThenTheExceptionShouldBeOfTypeBadRequestException() {
        _thrownException.Should().NotBeNull();
        _thrownException.Should().BeOfType<BadRequestException>();
    }

    [Then(@"the exception should have the authentication message")]
    public void ThenTheExceptionShouldHaveTheAuthenticationMessage() {
        _thrownException.Should().NotBeNull();
        _thrownException!.Message.Should().Be(_errorMessage);
    }

    [Then(@"the exception should be of type AuthenticationException")]
    public void ThenTheExceptionShouldBeOfTypeAuthenticationException() {
        _thrownException.Should().NotBeNull();
        _thrownException.Should().BeOfType<AuthenticationException>();
    }

    [Then(@"all public exceptions should inherit from CustomException")]
    public void ThenAllPublicExceptionsShouldInheritFromCustomException() {
        var exceptionTypes = new[]
        {
            typeof(ValidationException),
            typeof(NotFoundException),
            typeof(BadRequestException),
            typeof(AuthenticationException),
            typeof(ForbiddenAccessException)
        };

        foreach (var exceptionType in exceptionTypes) {
            exceptionType.Should().BeAssignableTo<CustomException>($"{exceptionType.Name} should inherit from CustomException");
        }
    }

    [Then(@"CustomException should inherit from Exception")]
    public void ThenCustomExceptionShouldInheritFromException() {
        typeof(CustomException).Should().BeAssignableTo<Exception>();
    }

    [Then(@"all exceptions should maintain proper inheritance hierarchy")]
    public void ThenAllExceptionsShouldMaintainProperInheritanceHierarchy() {
        // Verify that the inheritance chain is correct:
        // Exception -> CustomException -> Specific Exceptions

        typeof(CustomException).BaseType.Should().Be(typeof(Exception));

        var specificExceptions = new[]
        {
            typeof(ValidationException),
            typeof(NotFoundException),
            typeof(BadRequestException),
            typeof(AuthenticationException),
            typeof(ForbiddenAccessException)
        };

        foreach (var exceptionType in specificExceptions) {
            exceptionType.BaseType.Should().Be(typeof(CustomException));
        }
    }
}

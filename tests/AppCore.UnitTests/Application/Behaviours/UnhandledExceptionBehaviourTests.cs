using AppCore.Application.Behaviours;
using AppCore.Application.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Xunit;
using Moq;
using static AppCore.Application.Exceptions.CustomException;

namespace AppCore.UnitTests.Application.Behaviours;

public class UnhandledExceptionBehaviourTests
{
    private readonly Mock<ILogger<UnhandledExceptionBehaviour<TestRequest, TestResponse>>> _loggerMock;
    private readonly UnhandledExceptionBehaviour<TestRequest, TestResponse> _behaviour;

    public UnhandledExceptionBehaviourTests()
    {
        _loggerMock = new Mock<ILogger<UnhandledExceptionBehaviour<TestRequest, TestResponse>>>();
        _behaviour = new UnhandledExceptionBehaviour<TestRequest, TestResponse>(_loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithNoException_ShouldProceedToNext()
    {
        // Arrange
        var request = new TestRequest { Name = "John", Age = 25 };
        var expectedResponse = new TestResponse { Message = "Success" };
        
        var nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        nextMock.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        var result = await _behaviour.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        nextMock.Verify(x => x(), Times.Once);
    }

    [Theory]
    [InlineData(typeof(ApiDBException))]
    [InlineData(typeof(ValidationException))]
    [InlineData(typeof(OperationException))]
    [InlineData(typeof(NotFoundException))]
    [InlineData(typeof(BadRequestException))]
    [InlineData(typeof(ForbiddenAccessException))]
    [InlineData(typeof(AuthenticationException))]
    [InlineData(typeof(MappingException))]
    [InlineData(typeof(SerializerException))]
    [InlineData(typeof(CustomException))]
    public async Task Handle_WithHandledException_ShouldLogAndRethrow(Type exceptionType)
    {
        // Arrange
        var request = new TestRequest { Name = "John", Age = 25 };
        var exception = CreateExceptionOfType(exceptionType);
        
        var nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        nextMock.Setup(x => x()).ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync(exceptionType,
            () => _behaviour.Handle(request, nextMock.Object, CancellationToken.None));
        
        thrownException.Should().Be(exception);
        
        // Verify logging
        VerifyLoggerWasCalled(LogLevel.Error);
    }

    [Fact]
    public async Task Handle_WithUnhandledException_ShouldLogAndRethrow()
    {
        // Arrange
        var request = new TestRequest { Name = "John", Age = 25 };
        var exception = new ArgumentException("Unhandled exception");
        
        var nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        nextMock.Setup(x => x()).ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<ArgumentException>(
            () => _behaviour.Handle(request, nextMock.Object, CancellationToken.None));
        
        thrownException.Should().Be(exception);
        
        // Verify logging
        VerifyLoggerWasCalled(LogLevel.Error);
    }

    [Fact]
    public async Task Handle_WithExceptionWithInnerException_ShouldLogInnerExceptionMessages()
    {
        // Arrange
        var request = new TestRequest { Name = "John", Age = 25 };
        var innerException = new InvalidOperationException("Inner exception message");
        var outerException = new ArgumentException("Outer exception message", innerException);
        
        var nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        nextMock.Setup(x => x()).ThrowsAsync(outerException);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _behaviour.Handle(request, nextMock.Object, CancellationToken.None));
        
        // Verify logging occurred
        VerifyLoggerWasCalled(LogLevel.Error);
    }

    [Fact]
    public async Task Handle_WithNestedInnerExceptions_ShouldProcessAllInnerExceptions()
    {
        // Arrange
        var request = new TestRequest { Name = "John", Age = 25 };
        var innermost = new InvalidOperationException("Innermost exception");
        var middle = new ArgumentException("Middle exception", innermost);
        var outer = new NotSupportedException("Outer exception", middle);
        
        var nextMock = new Mock<RequestHandlerDelegate<TestResponse>>();
        nextMock.Setup(x => x()).ThrowsAsync(outer);

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(
            () => _behaviour.Handle(request, nextMock.Object, CancellationToken.None));
        
        // Verify logging occurred
        VerifyLoggerWasCalled(LogLevel.Error);
    }

    private static Exception CreateExceptionOfType(Type exceptionType)
    {
        return exceptionType.Name switch
        {
            nameof(ApiDBException) => new ApiDBException(new InvalidOperationException("Test DB exception")),
            nameof(ValidationException) => new ValidationException("TestProperty", "Test error"),
            nameof(OperationException) => new OperationException("Test operation exception"),
            nameof(NotFoundException) => new NotFoundException("Test message"),
            nameof(BadRequestException) => new BadRequestException("Test message"),
            nameof(ForbiddenAccessException) => new ForbiddenAccessException("Test message"),
            nameof(AuthenticationException) => new AuthenticationException("Test message"),
            nameof(MappingException) => new MappingException(new AutoMapper.AutoMapperMappingException("Test mapping")),
            nameof(SerializerException) => new SerializerException(new System.Text.Json.JsonException("Test serialization")),
            nameof(CustomException) => new CustomException(new DictionaryError("TEST-001", "Test custom exception")),
            _ => throw new ArgumentException($"Unknown exception type: {exceptionType.Name}")
        };
    }

    private void VerifyLoggerWasCalled(LogLevel logLevel)
    {
        _loggerMock.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}

using AppCore.Infrastructure.Services;
using AppCore.Application.Interfaces;
using AppCore.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using Xunit;
using Moq;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;

namespace AppCore.UnitTests.Infrastructure.Services;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<HttpContext> _httpContextMock;
    private readonly Mock<HttpRequest> _httpRequestMock;
    private readonly CurrentUserService _currentUserService;

    public CurrentUserServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _httpContextMock = new Mock<HttpContext>();
        _httpRequestMock = new Mock<HttpRequest>();
        
        _httpContextMock.Setup(x => x.Request).Returns(_httpRequestMock.Object);
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContextMock.Object);
        
        _currentUserService = new CurrentUserService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void Constructor_WithNullHttpContextAccessor_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new CurrentUserService(null!));
        exception.ParamName.Should().Be("httpContextAccessor");
    }

    [Fact]
    public void GetXtraceId_WithXTraceIDHeader_ShouldReturnHeaderValue()
    {
        // Arrange
        var expectedXtraceId = "trace-123456";
        var headers = new HeaderDictionary
        {
            { "X-Trace-ID", new StringValues(expectedXtraceId) }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act
        var result = _currentUserService.GetXtraceId();

        // Assert
        result.Should().Be(expectedXtraceId);
    }

    [Fact]
    public void GetXtraceId_WithoutXTraceIDHeader_ShouldReturnGeneratedGuid()
    {
        // Arrange
        var headers = new HeaderDictionary();
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act
        var result = _currentUserService.GetXtraceId();

        // Assert
        Guid.TryParse(result, out _).Should().BeTrue("result should be a valid GUID");
    }

    [Fact]
    public void GetXtraceId_WithEmptyXTraceIDHeader_ShouldReturnGeneratedGuid()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { "X-Trace-ID", new StringValues("") }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act
        var result = _currentUserService.GetXtraceId();

        // Assert
        Guid.TryParse(result, out _).Should().BeTrue("result should be a valid GUID when header is empty");
    }

    [Fact]
    public void GetXtraceId_WithWhitespaceXTraceIDHeader_ShouldReturnTrimmedValue()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { "X-Trace-ID", new StringValues("  trace-123  ") }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act
        var result = _currentUserService.GetXtraceId();

        // Assert
        result.Should().Be("trace-123");
    }

    [Fact]
    public void Service_ShouldImplementICurrentUserService()
    {
        // Assert
        _currentUserService.Should().BeAssignableTo<ICurrentUserService>();
    }

    [Fact]
    public void GetXtraceId_WithNullHttpContext_ShouldThrowAuthenticationException()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act & Assert
        Assert.Throws<AuthenticationException>(() => service.GetXtraceId());
    }

    [Fact]
    public void GetXtraceId_WithMultipleHeaderValues_ShouldReturnFirst()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { "X-Trace-ID", new StringValues(new[] { "trace-1", "trace-2" }) }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act
        var result = _currentUserService.GetXtraceId();

        // Assert
        result.Should().Be("trace-1");
    }
}

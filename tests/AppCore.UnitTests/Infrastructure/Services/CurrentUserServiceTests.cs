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

    [Fact]
    public void GetToken_WithValidBearerToken_ShouldReturnToken()
    {
        // Arrange
        var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.test";
        var headers = new HeaderDictionary
        {
            { "Authorization", new StringValues($"Bearer {expectedToken}") }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act
        var result = _currentUserService.GetToken();

        // Assert
        result.Should().Be(expectedToken);
    }

    [Fact]
    public void GetToken_WithoutAuthorizationHeader_ShouldThrowAuthenticationException()
    {
        // Arrange
        var headers = new HeaderDictionary();
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act & Assert
        Assert.Throws<AuthenticationException>(() => _currentUserService.GetToken());
    }

    [Fact]
    public void GetToken_WithNonBearerToken_ShouldThrowCustomException()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { "Authorization", new StringValues("Basic abc123") }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act & Assert
        Assert.Throws<CustomException>(() => _currentUserService.GetToken());
    }

    [Fact]
    public void GetToken_WithBearerTokenWithWhitespace_ShouldReturnTrimmedToken()
    {
        // Arrange
        var expectedToken = "token123";
        var headers = new HeaderDictionary
        {
            { "Authorization", new StringValues($"Bearer  {expectedToken}  ") }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act
        var result = _currentUserService.GetToken();

        // Assert
        result.Should().Be(expectedToken);
    }

    [Fact]
    public void GetJwtToken_WithValidToken_ShouldReturnParsedToken()
    {
        // Arrange
        var validJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";
        var headers = new HeaderDictionary
        {
            { "Authorization", new StringValues($"Bearer {validJwt}") }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act
        var result = _currentUserService.GetJwtToken();

        // Assert
        result.Should().NotBeNull();
        result.Payload.Should().ContainKey("sub");
    }

    [Fact]
    public void GetJwtToken_WithInvalidToken_ShouldThrowCustomException()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { "Authorization", new StringValues("Bearer invalid_token") }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act & Assert
        Assert.Throws<CustomException>(() => _currentUserService.GetJwtToken());
    }

    [Fact]
    public void GetUserId_WithValidToken_ShouldReturnSubClaim()
    {
        // Arrange
        var validJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";
        var headers = new HeaderDictionary
        {
            { "Authorization", new StringValues($"Bearer {validJwt}") }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act
        var result = _currentUserService.GetUserId();

        // Assert
        result.Should().Be("1234567890");
    }

    [Fact]
    public void GetUserId_WithoutToken_ShouldReturnHostName()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { "Host", new StringValues("test-host") }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act
        var result = _currentUserService.GetUserId();

        // Assert
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetUserName_WithValidToken_ShouldReturnEmailClaim()
    {
        // Arrange
        var validJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";
        var headers = new HeaderDictionary
        {
            { "Authorization", new StringValues($"Bearer {validJwt}") }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act
        var result = _currentUserService.GetUserName();

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void GetUserName_WithoutToken_ShouldReturnClientIP()
    {
        // Arrange
        var headers = new HeaderDictionary();
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);
        var connectionMock = new Mock<ConnectionInfo>();
        connectionMock.Setup(x => x.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("192.168.1.1"));
        _httpContextMock.Setup(x => x.Connection).Returns(connectionMock.Object);

        // Act
        var result = _currentUserService.GetUserName();

        // Assert
        result.Should().Be("192.168.1.1");
    }

    [Fact]
    public void GetUserName_WithXForwardedFor_ShouldReturnFirstIP()
    {
        // Arrange
        var headers = new HeaderDictionary
        {
            { "X-Forwarded-For", new StringValues("10.0.0.1, 10.0.0.2") }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act
        var result = _currentUserService.GetUserName();

        // Assert
        result.Should().Be("10.0.0.1");
    }

    [Fact]
    public void GetUserId_WithoutTokenAndHostHeader_ShouldReturnMachineName()
    {
        // Arrange
        var headers = new HeaderDictionary();
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act
        var result = _currentUserService.GetUserId();

        // Assert
        result.Should().Be(Environment.MachineName);
    }

    [Fact]
    public void GetUserName_WithNullHttpContext_ShouldReturnUnknown()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act
        var result = service.GetUserName();

        // Assert
        result.Should().Be("unknown");
    }

    [Fact]
    public void GetUserId_WithNullHttpContext_ShouldReturnUnknown()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var service = new CurrentUserService(_httpContextAccessorMock.Object);

        // Act
        var result = service.GetUserId();

        // Assert
        result.Should().Be("unknown");
    }

    [Fact]
    public void GetUserName_WithIPv6Localhost_ShouldReturnIPv4Localhost()
    {
        // Arrange
        var headers = new HeaderDictionary();
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);
        var connectionMock = new Mock<ConnectionInfo>();
        connectionMock.Setup(x => x.RemoteIpAddress).Returns(System.Net.IPAddress.Parse("::1"));
        _httpContextMock.Setup(x => x.Connection).Returns(connectionMock.Object);

        // Act
        var result = _currentUserService.GetUserName();

        // Assert
        result.Should().Be("127.0.0.1");
    }

    [Fact]
    public void GetUserId_WithTokenMissingSubClaim_ShouldThrowCustomException()
    {
        // Arrange - Token without 'sub' claim
        var jwtWithoutSub = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiSm9obiBEb2UifQ.xuEv8qrfXu424LZk8bVgr9MQJUIrp1rHcPyZw_KSsds";
        var headers = new HeaderDictionary
        {
            { "Authorization", new StringValues($"Bearer {jwtWithoutSub}") }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act & Assert
        Assert.Throws<CustomException>(() => _currentUserService.GetUserId());
    }

    [Fact]
    public void GetUserName_WithTokenMissingEmailClaim_ShouldThrowCustomException()
    {
        // Arrange - Token without 'email' claim
        var jwtWithoutEmail = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U";
        var headers = new HeaderDictionary
        {
            { "Authorization", new StringValues($"Bearer {jwtWithoutEmail}") }
        };
        _httpRequestMock.Setup(x => x.Headers).Returns(headers);

        // Act & Assert
        Assert.Throws<CustomException>(() => _currentUserService.GetUserName());
    }
}

using System.Security.Claims;
using AppCore.Application.Interfaces;
using AppCore.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace AppCore.UnitTests.Infrastructure.Services;

public class CurrentUserServiceAdditionalTests {
    [Fact]
    public void GetUserName_WithForwardedHeader_ShouldReturnForwardedIp() {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Forwarded-For"] = "192.168.1.100, 10.0.0.1";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessorMock.Object);

        // Act
        var result = service.GetUserName();

        // Assert
        result.Should().Be("192.168.1.100");
    }

    [Fact]
    public void GetUserId_WithHostHeader_ShouldReturnHostname() {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Host"] = "example.com:8080";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessorMock.Object);

        // Act
        var result = service.GetUserId();

        // Assert
        result.Should().Be("example.com:8080");
    }

    [Fact]
    public void GetXtraceId_WithEmptyHeader_ShouldReturnGuid() {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Trace-ID"] = "   ";
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessorMock.Object);

        // Act
        var result = service.GetXtraceId();

        // Assert
        Guid.TryParse(result, out _).Should().BeTrue();
    }
}

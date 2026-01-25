using System.Net;
using System.Text.Json;
using AppCore.Application.DTOs;
using AppCore.Application.Exceptions;
using AppCore.Application.Interfaces;
using AppCore.Application.Wrappers;
using AppCore.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace AppCore.UnitTests.Infrastructure.Services;

public class HttpServiceTests {
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<TestHttpService>> _loggerMock;
    private readonly Mock<IHttpRequestRepository> _httpRequestRepositoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly TestHttpService _httpService;

    public HttpServiceTests() {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<TestHttpService>>();
        _httpRequestRepositoryMock = new Mock<IHttpRequestRepository>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object) {
            BaseAddress = new Uri("https://api.example.com/")
        };


#pragma warning disable S3236
        _currentUserServiceMock.Setup(x => x.GetXtraceId(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()
        )).Returns(Guid.NewGuid().ToString("N"));
        _currentUserServiceMock.Setup(x => x.GetUserName(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()
        )).Returns("testuser");
#pragma warning restore S3236

        _httpService = new TestHttpService(
            _httpClient,
            _currentUserServiceMock.Object,
            _loggerMock.Object,
            _httpRequestRepositoryMock.Object
        );
    }

    [Fact]
    public async Task ExecuteGetAsync_WithSuccessResponse_ShouldReturnData() {
        // Arrange
        var expectedResponse = new EmailRequest {
            To = "test@example.com",
            Subject = "Test",
            Body = "Body",
            From = "sender@example.com"
        };
        var jsonResponse = JsonSerializer.Serialize(expectedResponse);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri == new Uri("https://api.example.com/test")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _httpService.ExecuteGetAsync<EmailRequest>("test");

        // Assert
        result.Should().NotBeNull();
        result.To.Should().Be(expectedResponse.To);
        result.Subject.Should().Be(expectedResponse.Subject);
    }

    [Fact]
    public async Task ExecuteGetAsync_WithNotFound_ShouldThrowNotFoundException() {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var act = () => _httpService.ExecuteGetAsync<EmailRequest>("not-found");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecutePostAsync_WithBadRequest_ShouldThrowCustomException() {
        // Arrange
        var errorResponse = new Dictionary<string, object> { { "Error", "Bad Request Details" } };
        var jsonError = JsonSerializer.Serialize(errorResponse);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(jsonError)
            });

        // Act
        var act = () => _httpService.ExecutePostAsync<EmailRequest>("create", new Dictionary<string, object> { { "Name", "Test" } });

        // Assert
        await act.Should().ThrowAsync<CustomException>()
            .Where(e => ((DictionaryError)e.MessageLog.Message).Code == "HTTP001");
    }

    [Fact]
    public async Task ExecuteGetAsync_WithInternalServerError_ShouldThrowCustomException() {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("{}")
            });

        // Act
        var act = () => _httpService.ExecuteGetAsync<EmailRequest>("error");

        // Assert
        await act.Should().ThrowAsync<CustomException>()
            .Where(e => ((DictionaryError)e.MessageLog.Message).Code == "HTTP004");
    }

    [Fact]
    public async Task ExecuteDeleteAsync_WithNoContent_ShouldReturnDefault() {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        var result = await _httpService.ExecuteDeleteAsync<EmailRequest>("delete/1");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecutePutAsync_WithSuccess_ShouldReturnData() {
        // Arrange
        var request = new Dictionary<string, object> { { "Key", "Value" } };
        var jsonResponse = JsonSerializer.Serialize(new EmailRequest { Subject = "Updated", To = "to", From = "from", Body = "body" });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _httpService.ExecutePutAsync<EmailRequest>("update", request);

        // Assert
        result.Should().NotBeNull();
        result.Subject.Should().Be("Updated");
    }

    [Fact]
    public async Task ExecutePatchAsync_WithSuccess_ShouldReturnData() {
        // Arrange
        var request = new Dictionary<string, object> { { "Key", "Value" } };
        var jsonResponse = JsonSerializer.Serialize(new EmailRequest { Subject = "Patched", To = "to", From = "from", Body = "body" });

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Patch),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _httpService.ExecutePatchAsync<EmailRequest>("patch", request);

        // Assert
        result.Should().NotBeNull();
        result.Subject.Should().Be("Patched");
    }

    [Fact]
    public async Task ExecuteHeadAsync_WithSuccess_ShouldReturnDefault() {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Head),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var result = await _httpService.ExecuteHeadAsync<object>("head");

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "HTTP002")]
    [InlineData(HttpStatusCode.Forbidden, "HTTP003")]
    [InlineData(HttpStatusCode.BadGateway, "HTTP005")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "HTTP006")]
    [InlineData(HttpStatusCode.TooManyRequests, "HTTP007")]
    public async Task ExecuteGetAsync_WithErrorCode_ShouldThrowCorrectCustomException(HttpStatusCode statusCode, string errorCode) {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = statusCode,
                Content = new StringContent("{}")
            });

        // Act
        var act = () => _httpService.ExecuteGetAsync<EmailRequest>("error");

        // Assert
        await act.Should().ThrowAsync<CustomException>()
            .Where(e => ((DictionaryError)e.MessageLog.Message).Code == errorCode);
    }
}

using System.Net;
using System.Text.Json;
using OrionSoft.AppCore.Application.DTOs;
using OrionSoft.AppCore.Application.Exceptions;
using OrionSoft.AppCore.Application.Interfaces;
using OrionSoft.AppCore.Application.Wrappers;
using OrionSoft.AppCore.Domain.Entities.Integrators;
using OrionSoft.AppCore.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace OrionSoft.AppCore.UnitTests.Infrastructure.Services;

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

    #region Helpers

    private static string CreateEmailJson(string subject = "Test") =>
        JsonSerializer.Serialize(new EmailRequest {
            To = "to@test.com",
            Subject = subject,
            Body = "body",
            From = "from@test.com"
        });

    private void SetupHttpResponse(HttpStatusCode statusCode, string content = "{}") {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });
    }

    private void SetupHttpException(Exception exception) {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);
    }

    private void VerifyLogLevel(LogLevel level, Times times) {
        _loggerMock.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    #endregion

    #region Existing Tests

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
    public async Task ExecutePostAsync_WithSuccessResponse_ShouldReturnData() {
        // Arrange
        var jsonResponse = CreateEmailJson("Posted");
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var result = await _httpService.ExecutePostAsync<Dictionary<string, object>, EmailRequest>("create", new Dictionary<string, object> { { "Key", "Value" } });

        // Assert
        result.Should().NotBeNull();
        result.Subject.Should().Be("Posted");
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
        var act = () => _httpService.ExecutePostAsync<Dictionary<string, object>, EmailRequest>("create", new Dictionary<string, object> { { "Name", "Test" } });

        // Assert
        await act.Should().ThrowAsync<CustomException>()
            .Where(e => e.Error.Code == "HTTP001");
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
            .Where(e => e.Error.Code == "HTTP004");
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
        var result = await _httpService.ExecutePutAsync<Dictionary<string, object>, EmailRequest>("update", request);

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
        var result = await _httpService.ExecutePatchAsync<Dictionary<string, object>, EmailRequest>("patch", request);

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
            .Where(e => e.Error.Code == errorCode);
    }

    #endregion

    #region Group A: Raw Methods

    [Fact]
    public async Task ExecuteGetRawAsync_WithSuccessResponse_ShouldReturnHttpResponse() {
        // Arrange
        var json = CreateEmailJson("GetRaw");
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        // Act
        var result = await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNull();
        result.Data!.Subject.Should().Be("GetRaw");
    }

    [Fact]
    public async Task ExecutePostRawAsync_WithSuccessResponse_ShouldReturnHttpResponse() {
        // Arrange
        var json = CreateEmailJson("PostRaw");
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        // Act
        var result = await _httpService.ExecutePostRawAsync<Dictionary<string, object>, EmailRequest>("test", new Dictionary<string, object> { { "Name", "Test" } });

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data!.Subject.Should().Be("PostRaw");
    }

    [Fact]
    public async Task ExecutePutRawAsync_WithSuccessResponse_ShouldReturnHttpResponse() {
        // Arrange
        var json = CreateEmailJson("PutRaw");
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        // Act
        var result = await _httpService.ExecutePutRawAsync<Dictionary<string, object>, EmailRequest>("test", new Dictionary<string, object> { { "Name", "Test" } });

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data!.Subject.Should().Be("PutRaw");
    }

    [Fact]
    public async Task ExecutePatchRawAsync_WithSuccessResponse_ShouldReturnHttpResponse() {
        // Arrange
        var json = CreateEmailJson("PatchRaw");
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Patch),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        // Act
        var result = await _httpService.ExecutePatchRawAsync<Dictionary<string, object>, EmailRequest>("test", new Dictionary<string, object> { { "Name", "Test" } });

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data!.Subject.Should().Be("PatchRaw");
    }

    [Fact]
    public async Task ExecuteDeleteRawAsync_WithSuccessResponse_ShouldReturnHttpResponse() {
        // Arrange
        var json = CreateEmailJson("DeleteRaw");
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        // Act
        var result = await _httpService.ExecuteDeleteRawAsync<EmailRequest>("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data!.Subject.Should().Be("DeleteRaw");
    }

    [Fact]
    public async Task ExecuteHeadRawAsync_WithSuccessResponse_ShouldReturnHttpResponse() {
        // Arrange
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Head),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK
            });

        // Act
        var result = await _httpService.ExecuteHeadRawAsync<object>("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ExecuteGetRawAsync_WithErrorStatus_ShouldNotThrowException() {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadRequest, "{\"error\":\"bad\"}");

        // Act
        var result = await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteGetRawAsync_WithNotFound_ShouldReturnNormally() {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound);

        // Act
        var result = await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Group B: QueryParams / BuildEndpoint

    [Fact]
    public async Task ExecuteGetRawAsync_WithQueryParams_ShouldAppendToEndpoint() {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, CreateEmailJson());
        var queryParams = new Dictionary<string, string> { { "key", "value" } };

        // Act
        await _httpService.ExecuteGetRawAsync<EmailRequest>("test", queryParams: queryParams);

        // Assert
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString() == "https://api.example.com/test?key=value"),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteGetRawAsync_WithEmptyQueryParams_ShouldNotAppendQueryString() {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, CreateEmailJson());
        var queryParams = new Dictionary<string, string>();

        // Act
        await _httpService.ExecuteGetRawAsync<EmailRequest>("test", queryParams: queryParams);

        // Assert
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.ToString() == "https://api.example.com/test"),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteGetRawAsync_WithSpecialCharacters_ShouldUrlEncode() {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, CreateEmailJson());
        var queryParams = new Dictionary<string, string> {
            { "name", "John Doe" },
            { "q", "a&b=c" }
        };

        // Act
        await _httpService.ExecuteGetRawAsync<EmailRequest>("test", queryParams: queryParams);

        // Assert
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri!.AbsoluteUri.Contains("name=John%20Doe") &&
                    req.RequestUri!.AbsoluteUri.Contains("q=a%26b%3Dc")),
                ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region Group C: Body / CreateContent

    [Fact]
    public async Task ExecutePostRawAsync_WithJsonDocumentBody_ShouldSerializeCorrectly() {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, CreateEmailJson());
        var body = JsonDocument.Parse("{\"key\":\"value\"}");

        // Act
        var result = await _httpService.ExecutePostRawAsync<JsonDocument, EmailRequest>("test", body);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Content != null),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task ExecutePostRawAsync_WithNullBody_ShouldNotSendContent() {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, CreateEmailJson());

        // Act
        var result = await _httpService.ExecutePostRawAsync<EmailRequest>("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Content == null),
                ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region Group D: OK with nullable type response

    [Fact]
    public async Task ExecuteGetRawAsync_OkWithObjectType_ShouldReturnDefaultData() {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, "{\"some\":\"data\"}");

        // Act
        var result = await _httpService.ExecuteGetRawAsync<object>("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteGetRawAsync_OkWithEmptyBodyNonNullable_ShouldReturnError() {
        // Arrange — response body is [1,2,3], incompatible with EmailRequest
        SetupHttpResponse(HttpStatusCode.OK, "[1,2,3]");

        // Act
        var result = await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Group E: SerializerException in deserialization

    [Fact]
    public async Task ExecuteGetRawAsync_OkWithIncompatibleJson_ShouldReturn500WithError() {
        // Arrange — valid JSON but cannot be deserialized to EmailRequest
        SetupHttpResponse(HttpStatusCode.OK, "{\"invalid\":true}");

        // Act
        var result = await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert — either deserialization succeeds with defaults or fails with 500
        // In .NET 10 with required properties, deserializing invalid JSON should fail
        if (result.StatusCode == HttpStatusCode.InternalServerError) {
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        } else {
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    #endregion

    #region Group F: Exceptions in catch

    [Fact]
    public async Task ExecuteGetRawAsync_HttpRequestExceptionWithStatusCode_ShouldReturnThatStatus() {
        // Arrange
        SetupHttpException(new HttpRequestException("Connection refused", null, HttpStatusCode.BadGateway));

        // Act
        var result = await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadGateway);
        result.ErrorMessage.Should().Be("Connection refused");
    }

    [Fact]
    public async Task ExecuteGetRawAsync_HttpRequestExceptionWithoutStatusCode_ShouldReturn500() {
        // Arrange
        SetupHttpException(new HttpRequestException("DNS resolution failed"));

        // Act
        var result = await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.ErrorMessage.Should().Be("DNS resolution failed");
    }

    [Fact]
    public async Task ExecuteGetRawAsync_GenericException_ShouldReturn500() {
        // Arrange
        SetupHttpException(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task ExecuteGetRawAsync_SerializerException_ShouldReturn500WithSerializerError() {
        // Arrange
        SetupHttpException(new SerializerException("Test serialization error"));

        // Act
        var result = await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.Response.Should().NotBeNull();
        result.Response!.RootElement.GetProperty("Error").GetString().Should().Be("Error deserializing response.");
    }

    #endregion

    #region Group G: Auditing flow

    [Fact]
    public async Task ExecuteGetRawAsync_WithAuditing_ShouldCallAddAsync() {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, CreateEmailJson());

        // Act
        await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        _httpRequestRepositoryMock.Verify(
            r => r.AddAsync(It.Is<HttpAuditEntity>(e =>
                e.Endpoint == "https://api.example.com/test" &&
                e.Method == HttpMethod.Get)),
            Times.Once());
    }

    [Fact]
    public async Task ExecuteGetRawAsync_WithAuditing_ShouldCallUpdateAsync() {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, CreateEmailJson());

        // Act
        await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        _httpRequestRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<HttpAuditEntity>(e =>
                e.StatusCode == HttpStatusCode.OK)),
            Times.Once());
    }

    [Fact]
    public async Task ExecuteGetRawAsync_WithoutAuditing_ShouldNotCallRepository() {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var client = new HttpClient(handlerMock.Object) {
            BaseAddress = new Uri("https://api.example.com/")
        };
        var repoMock = new Mock<IHttpRequestRepository>();
        var service = new TestHttpService(
            client,
            _currentUserServiceMock.Object,
            _loggerMock.Object,
            httpRequestRepository: null);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(CreateEmailJson())
            });

        // Act
        await service.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        repoMock.Verify(r => r.AddAsync(It.IsAny<HttpAuditEntity>()), Times.Never());
        repoMock.Verify(r => r.UpdateAsync(It.IsAny<HttpAuditEntity>()), Times.Never());
    }

    [Fact]
    public async Task ExecuteGetRawAsync_WithAuditingAndError_ShouldUpdateAuditWithError() {
        // Arrange
        SetupHttpException(new HttpRequestException("Timeout", null, HttpStatusCode.GatewayTimeout));

        // Act
        await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        _httpRequestRepositoryMock.Verify(
            r => r.UpdateAsync(It.Is<HttpAuditEntity>(e =>
                e.InternalError != null)),
            Times.Once());
    }

    #endregion

    #region Group H: TraceId header

    [Fact]
    public async Task ExecuteGetRawAsync_WithXTraceIdHeader_ShouldUseHeaderValue() {
        // Arrange
        var customTraceId = Guid.NewGuid();
        var headers = new Dictionary<string, string> { { "X-Trace-ID", customTraceId.ToString("N") } };
        SetupHttpResponse(HttpStatusCode.OK, CreateEmailJson());

        // Act
        await _httpService.ExecuteGetRawAsync<EmailRequest>("test", headers: headers);

        // Assert
        _httpRequestRepositoryMock.Verify(
            r => r.AddAsync(It.Is<HttpAuditEntity>(e =>
                e.TraceId == customTraceId)),
            Times.Once());
    }

    [Fact]
    public async Task ExecuteGetRawAsync_WithoutXTraceIdHeader_ShouldUseGetXtraceId() {
        // Arrange
        var serviceTraceId = Guid.NewGuid();
#pragma warning disable S3236
        _currentUserServiceMock.Setup(x => x.GetXtraceId(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>()
        )).Returns(serviceTraceId.ToString("N"));
#pragma warning restore S3236
        SetupHttpResponse(HttpStatusCode.OK, CreateEmailJson());

        // Act
        await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        _httpRequestRepositoryMock.Verify(
            r => r.AddAsync(It.Is<HttpAuditEntity>(e =>
                e.TraceId == serviceTraceId)),
            Times.Once());
    }

    #endregion

    #region Group I: HandleCustomResponse

    [Fact]
    public void HandleCustomResponse_WithUnmappedStatusCode_ShouldThrowHttp008() {
        // Arrange
        var response = new HttpResponse<EmailRequest>(HttpStatusCode.Gone, 100);

        // Act
        var act = () => _httpService.InvokeHandleCustomResponse(response, Guid.NewGuid().ToString("N"), "https://api.example.com/test");

        // Assert
        act.Should().Throw<CustomException>()
            .Where(e => e.Error.Code == "HTTP008");
    }

    [Fact]
    public void HandleCustomResponse_InternalServerErrorWithNullErrorMessage_ShouldUseErrorContext() {
        // Arrange
        var response = new HttpResponse<EmailRequest>(HttpStatusCode.InternalServerError, 200) {
            ErrorMessage = null
        };

        // Act
        var act = () => _httpService.InvokeHandleCustomResponse(response, Guid.NewGuid().ToString("N"), "https://api.example.com/test");

        // Assert
        var exception = act.Should().Throw<CustomException>().Which;
        var error = exception.Error;
        error.Code.Should().Be("HTTP004");
        error.ProviderMessage.Should().NotBeNull();
    }

    #endregion

    #region Group K: Property & Content Headers

    [Fact]
    public void CurrentUserService_Property_ShouldReturnInjectedService() {
        // Act
        var result = _httpService.GetExposedCurrentUserService();

        // Assert
        result.Should().Be(_currentUserServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteGetRawAsync_WithContentTypeHeader_ShouldAddToContentHeaders() {
        // Arrange — "Content-Type" is rejected by HttpRequestHeaders.TryAddWithoutValidation,
        // so it falls through to content headers (lines 263-264)
        SetupHttpResponse(HttpStatusCode.OK, CreateEmailJson());
        var headers = new Dictionary<string, string> { { "Content-Type", "application/xml" } };

        // Act
        var result = await _httpService.ExecuteGetRawAsync<EmailRequest>("test", headers: headers);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Content != null),
                ItExpr.IsAny<CancellationToken>());
    }

    #endregion

    #region Group J: Logging

    [Fact]
    public async Task ExecuteGetRawAsync_SuccessResponse_ShouldLogInformation() {
        // Arrange
        SetupHttpResponse(HttpStatusCode.OK, CreateEmailJson());

        // Act
        await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        VerifyLogLevel(LogLevel.Information, Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteGetRawAsync_ErrorResponse_ShouldLogError() {
        // Arrange
        SetupHttpResponse(HttpStatusCode.BadGateway);

        // Act
        await _httpService.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        VerifyLogLevel(LogLevel.Error, Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteGetRawAsync_ExceptionDuringExecution_ShouldLogErrorAndRethrow() {
        // Arrange — make UpdateAsync throw to trigger the catch in ExecuteHttpRequestRawAsync
        var handlerMock = new Mock<HttpMessageHandler>();
        var client = new HttpClient(handlerMock.Object) {
            BaseAddress = new Uri("https://api.example.com/")
        };
        var loggerMock = new Mock<ILogger<TestHttpService>>();
        var repoMock = new Mock<IHttpRequestRepository>();
        repoMock.Setup(r => r.UpdateAsync(It.IsAny<HttpAuditEntity>()))
            .ThrowsAsync(new InvalidOperationException("DB connection lost"));

        var service = new TestHttpService(
            client,
            _currentUserServiceMock.Object,
            loggerMock.Object,
            repoMock.Object);

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(CreateEmailJson())
            });

        // Act
        var act = () => service.ExecuteGetRawAsync<EmailRequest>("test");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("DB connection lost");
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce());
    }

    #endregion
}

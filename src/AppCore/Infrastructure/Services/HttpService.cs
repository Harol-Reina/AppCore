using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using AppCore.Application.Exceptions;
using AppCore.Application.Extensions;
using AppCore.Application.Interfaces;
using AppCore.Application.Wrappers;
using AppCore.Domain.Entities.Integrators;
using AppCore.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AppCore.Infrastructure.Services;

public abstract class HttpService(HttpClient httpClient,
                                  ICurrentUserService currentUserService,
                                  ILogger logger,
                                  IHttpRequestRepository? httpRequestRepository = null) {
    private readonly Stopwatch _timer = new();
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IHttpRequestRepository? _httpRequestRepository = httpRequestRepository;
    private readonly ILogger _logger = logger;
    private readonly bool _enableAuditing = httpRequestRepository != null;

    protected ICurrentUserService currentUserService => _currentUserService;

    private static bool IsSuccessStatus(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.OK
                    or HttpStatusCode.Created
                    or HttpStatusCode.Accepted
                    or HttpStatusCode.NoContent;

    #region HTTP Methods

    protected async Task<T> ExecuteGetAsync<T>(string endpoint,
                                               Dictionary<string, string>? headers = null,
                                               Dictionary<string, string>? queryParams = null) {
        var fullEndpoint = BuildEndpoint(endpoint, queryParams);
        return await ExecuteHttpRequestAsync<T>(fullEndpoint, null, headers, HttpMethod.Get);
    }

    protected async Task<T> ExecutePostAsync<T>(string endpoint,
                                                object? body = null,
                                                Dictionary<string, string>? headers = null) {
        return await ExecuteHttpRequestAsync<T>(endpoint, body, headers, HttpMethod.Post);
    }

    protected async Task<T> ExecutePutAsync<T>(string endpoint,
                                               object? body = null,
                                               Dictionary<string, string>? headers = null) {
        return await ExecuteHttpRequestAsync<T>(endpoint, body, headers, HttpMethod.Put);
    }

    protected async Task<T> ExecutePatchAsync<T>(string endpoint,
                                                 object? body = null,
                                                 Dictionary<string, string>? headers = null) {
        return await ExecuteHttpRequestAsync<T>(endpoint, body, headers, HttpMethod.Patch);
    }

    protected async Task<T> ExecuteDeleteAsync<T>(string endpoint,
                                                  object? body = null,
                                                  Dictionary<string, string>? headers = null)
        => await ExecuteHttpRequestAsync<T>(endpoint, body, headers, HttpMethod.Delete);

    protected async Task<T> ExecuteHeadAsync<T>(string endpoint,
                                                object? body = null,
                                                Dictionary<string, string>? headers = null)
        => await ExecuteHttpRequestAsync<T>(endpoint, body, headers, HttpMethod.Head);

    protected Task<HttpResponse<T>> ExecutePostRawAsync<T>(string endpoint,
                                                           object? body = null,
                                                           Dictionary<string, string>? headers = null) =>
    ExecuteHttpRequestRawAsync<T>(endpoint, body, headers, HttpMethod.Post);

    protected Task<HttpResponse<T>> ExecuteGetRawAsync<T>(string endpoint,
                                                          Dictionary<string, string>? headers = null,
                                                          Dictionary<string, string>? queryParams = null) {
        var fullEndpoint = BuildEndpoint(endpoint, queryParams);
        return ExecuteHttpRequestRawAsync<T>(fullEndpoint, null, headers, HttpMethod.Get);
    }

    protected Task<HttpResponse<T>> ExecutePutRawAsync<T>(string endpoint,
                                                          object? body = null,
                                                          Dictionary<string, string>? headers = null)
        => ExecuteHttpRequestRawAsync<T>(endpoint, body, headers, HttpMethod.Put);

    protected Task<HttpResponse<T>> ExecutePatchRawAsync<T>(string endpoint,
                                                            object? body = null,
                                                            Dictionary<string, string>? headers = null)
         => ExecuteHttpRequestRawAsync<T>(endpoint, body, headers, HttpMethod.Patch);

    protected Task<HttpResponse<T>> ExecuteDeleteRawAsync<T>(string endpoint,
                                                             object? body = null,
                                                             Dictionary<string, string>? headers = null)
        => ExecuteHttpRequestRawAsync<T>(endpoint, body, headers, HttpMethod.Delete);

    protected Task<HttpResponse<T>> ExecuteHeadRawAsync<T>(string endpoint,
                                                           Dictionary<string, string>? headers = null)
        => ExecuteHttpRequestRawAsync<T>(endpoint, null, headers, HttpMethod.Head);

    #endregion


    private string ResolveTraceId(Dictionary<string, string>? headers) {
        if (headers is not null && headers.TryGetValue("X-Trace-ID", out var existingTraceId))
            return existingTraceId;
        return _currentUserService.GetXtraceId();
    }

    protected async Task<HttpResponse<T>> ExecuteHttpRequestRawAsync<T>(string endpoint,
                                                                        object? body,
                                                                        Dictionary<string, string>? headers,
                                                                        HttpMethod method) {
        HttpAuditEntity? auditEntity = null;
        var traceId = ResolveTraceId(headers);

        if (_enableAuditing && _httpRequestRepository != null)
            auditEntity = await CreateAuditEntity(traceId, endpoint, method, body, headers);

        try {
            _logger.LogInformation("Executing {Method} request to {Endpoint}", method, endpoint);

            var httpResponse = await ExecuteHttpMethod<T>(endpoint, body, headers, method);

            if (auditEntity != null && _httpRequestRepository != null)
                await UpdateAuditEntityAsync(auditEntity, httpResponse);

            if (!IsSuccessStatus(httpResponse.StatusCode) && httpResponse.StatusCode != HttpStatusCode.NotFound) {
                _logger.LogError(
                    "HTTP request failed with status {StatusCode} for endpoint {Endpoint}",
                    httpResponse.StatusCode, endpoint);
            } else {
                _logger.LogInformation(
                    "HTTP request completed successfully for endpoint {Endpoint} in {ElapsedMs}ms",
                    endpoint, httpResponse.Time);
            }

            return httpResponse;
        } catch (Exception ex) {
            _logger.LogError(ex, "Error executing HTTP request to {Endpoint}", endpoint);
            throw;
        }
    }

    private async Task<T> ExecuteHttpRequestAsync<T>(string endpoint,
                                                     object? body,
                                                     Dictionary<string, string>? headers,
                                                     HttpMethod method) {
        var httpResponse = await ExecuteHttpRequestRawAsync<T>(endpoint, body, headers, method);

        if (!IsSuccessStatus(httpResponse.StatusCode)) {
            _logger.LogWarning(
                "HTTP request failed with status {StatusCode} for endpoint {Endpoint}",
                httpResponse.StatusCode, endpoint);
            var baseUrl = _httpClient.BaseAddress?.ToString() ?? "Unknown";
            var traceId = ResolveTraceId(headers);
            HandleCustomResponseAsync(httpResponse, traceId, $"{baseUrl}{endpoint}");
        }

        return httpResponse.Data!;
    }

    private async Task<HttpResponse<T>> ExecuteHttpMethod<T>(string endpoint,
                                                              object? body,
                                                              Dictionary<string, string>? headers,
                                                              HttpMethod method) {
        try {
            var requestMessage = CreateHttpRequestMessage(method, endpoint, body, headers);
            var (response, elapsedMs) = await SendTimedRequestAsync(requestMessage);

            if (response.StatusCode == HttpStatusCode.OK)
                return await ParseSuccessResponseAsync<T>(response, elapsedMs);

            var responseJson = JsonExtend.ToJsonDocument(await response.Content.ReadAsStringAsync());
            return new HttpResponse<T>(response.StatusCode, elapsedMs, responseJson);
        } catch (Exception ex) {
            return HandleExecutionException<T>(ex);
        }
    }

    private static HttpRequestMessage CreateHttpRequestMessage(HttpMethod method,
                                                                string endpoint,
                                                                object? body,
                                                                Dictionary<string, string>? headers) {
        var message = new HttpRequestMessage(method, endpoint);
        if (body is not null)
            message.Content = CreateContent(body);
        AddHeaders(message, headers);
        return message;
    }

    private async Task<(HttpResponseMessage Response, long ElapsedMs)> SendTimedRequestAsync(
        HttpRequestMessage requestMessage) {
        _timer.Restart();
        var response = await _httpClient.SendAsync(requestMessage);
        _timer.Stop();
        return (response, _timer.ElapsedMilliseconds);
    }

    private static async Task<HttpResponse<T>> ParseSuccessResponseAsync<T>(
        HttpResponseMessage response, long elapsedMs) {
        var responseJson = JsonExtend.ToJsonDocument(await response.Content.ReadAsStringAsync());
        bool isNullable = typeof(T) == typeof(object);

        if (!isNullable && string.IsNullOrEmpty(responseJson?.RootElement.GetRawText()))
            return new HttpResponse<T>(HttpStatusCode.InternalServerError, elapsedMs, responseJson) {
                ErrorMessage = "Response content is empty but expected to be non-nullable type."
            };

        try {
            return new HttpResponse<T>(response.StatusCode, elapsedMs, responseJson) {
                Data = isNullable ? default : JsonExtend.Deserialize<T>(responseJson!.RootElement.GetRawText())
            };
        } catch (SerializerException ex) {
            return new HttpResponse<T>(HttpStatusCode.InternalServerError, elapsedMs, responseJson) {
                ErrorMessage = ex.MessageLog.ToString()
            };
        }
    }

    private HttpResponse<T> HandleExecutionException<T>(Exception ex) {
        if (_timer.IsRunning)
            _timer.Stop();

        return ex switch {
            HttpRequestException httpEx => new HttpResponse<T>(
                httpEx.StatusCode ?? HttpStatusCode.InternalServerError,
                _timer.ElapsedMilliseconds,
                JsonExtend.ToJsonDocument(new Dictionary<string, object> { { "Error", "Making HTTP request." } })) {
                ErrorMessage = httpEx.Message
            },
            SerializerException serEx => new HttpResponse<T>(
                HttpStatusCode.InternalServerError,
                _timer.ElapsedMilliseconds,
                JsonExtend.ToJsonDocument(new Dictionary<string, object> { { "Error", "Error deserializing response." } })) {
                ErrorMessage = serEx.MessageLog.ToString()
            },
            _ => new HttpResponse<T>(
                HttpStatusCode.InternalServerError,
                _timer.ElapsedMilliseconds,
                JsonExtend.ToJsonDocument(new Dictionary<string, object> { { "Error", ex.Message } }))
        };
    }

    private static StringContent CreateContent(object? body) {
        return body switch {
            null => new StringContent("", Encoding.UTF8, "application/json"),
            JsonDocument element => new StringContent(element.RootElement.GetRawText(), Encoding.UTF8, "application/json"),
            object obj => new StringContent(JsonExtend.Serialize(obj), Encoding.UTF8, "application/json")
        };
    }

    private static void AddHeaders(HttpRequestMessage httpRequestMessage, Dictionary<string, string>? headers) {
        if (headers == null) return;

        var requestHeaders = headers.Where(h => httpRequestMessage.Headers.TryAddWithoutValidation(h.Key, h.Value));
        var contentHeaders = headers.Except(requestHeaders);

        foreach (var header in contentHeaders) {
            httpRequestMessage.Content ??= new StringContent(string.Empty);
            httpRequestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    private async Task<HttpAuditEntity> CreateAuditEntity(string traceId,
                                                          string endpoint,
                                                          HttpMethod method,
                                                          object? body,
                                                          Dictionary<string, string>? headers) {
        var baseUrl = _httpClient.BaseAddress?.ToString() ?? "Unknown";
        var auditEntity = new HttpAuditEntity(
            Guid.Parse(traceId),
            $"{baseUrl}{endpoint}",
            method,
            body,
            headers) { };
        await _httpRequestRepository!.AddAsync(auditEntity);
        return auditEntity;
    }

    private async Task UpdateAuditEntityAsync<T>(HttpAuditEntity auditEntity,
                                                 HttpResponse<T> httpResponse) {
        auditEntity.ElapsedMilliseconds = httpResponse.Time;
        auditEntity.StatusCode = httpResponse.StatusCode;
        auditEntity.Response = httpResponse.Response != null
            ? JsonExtend.ToJsonDocument(httpResponse.Response)
            : null;
        auditEntity.InternalError = httpResponse.ErrorMessage;
        auditEntity.UpdatedAt = DateTime.Now;
        auditEntity.UpdatedBy = _currentUserService.GetUserName();

        await _httpRequestRepository!.UpdateAsync(auditEntity);
    }


    private static string BuildEndpoint(string endpoint, Dictionary<string, string>? queryParams) {
        if (queryParams == null || queryParams.Count == 0)
            return endpoint;
        var queryString = string.Join("&",
            queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{endpoint}?{queryString}";
    }

    protected virtual void HandleCustomResponseAsync<T>(HttpResponse<T> httpResponse,
                                                        string traceId,
                                                        string endpoint,
                                                        [CallerMemberName] string memberName = "",
                                                        [CallerFilePath] string sourceFilePath = "",
                                                        [CallerLineNumber] int sourceLineNumber = 0) {
        var errorContext = new Dictionary<string, object> {
            { "TraceId", traceId },
            { "Endpoint", endpoint },
            { "StatusCode", httpResponse.StatusCode },
            { "ElapsedMilliseconds", httpResponse.Time },
            { "ErrorMessage", httpResponse.ErrorMessage ?? "None" },
            { "Response", httpResponse.Response ?? JsonDocument.Parse("{}") }
        };

        var errorMessage = JsonExtend.Serialize(errorContext);

        throw httpResponse.StatusCode switch {
            //  Los NotFounds no se manejan como excepciones, sino como respuestas HTTP normales.
            HttpStatusCode.NotFound => new NotFoundException(),

            HttpStatusCode.BadRequest => new CustomException(
                new("HTTP001", "Bad request", errorMessage),
                memberName, sourceFilePath, sourceLineNumber),

            HttpStatusCode.Unauthorized => new CustomException(
                new("HTTP002", "Unauthorized", errorMessage),
                memberName, sourceFilePath, sourceLineNumber),

            HttpStatusCode.Forbidden => new CustomException(
                new("HTTP003", "Forbidden", errorMessage),
                memberName, sourceFilePath, sourceLineNumber),

            HttpStatusCode.InternalServerError => new CustomException(
                new("HTTP004", "Internal server error",
                    httpResponse.ErrorMessage ?? errorMessage),
                memberName, sourceFilePath, sourceLineNumber),

            HttpStatusCode.BadGateway => new CustomException(
                new("HTTP005", "Bad gateway", errorMessage),
                memberName, sourceFilePath, sourceLineNumber),

            HttpStatusCode.ServiceUnavailable => new CustomException(
                new("HTTP006", "Service unavailable", errorMessage),
                memberName, sourceFilePath, sourceLineNumber),

            HttpStatusCode.TooManyRequests => new CustomException(
                new("HTTP007", "Rate limit exceeded", errorMessage),
                memberName, sourceFilePath, sourceLineNumber),

            _ => new CustomException(
                new("HTTP008", "HTTP request failed", errorMessage),
                memberName, sourceFilePath, sourceLineNumber)
        };
    }
}

using System.Collections.Frozen;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using OrionSoft.AppCore.Application.Exceptions;
using OrionSoft.AppCore.Application.Extensions;
using OrionSoft.AppCore.Application.Interfaces;
using OrionSoft.AppCore.Application.Wrappers;
using OrionSoft.AppCore.Domain.Entities.Integrators;
using OrionSoft.AppCore.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace OrionSoft.AppCore.Infrastructure.Services;

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

    #region HTTP Methods (AOT-compatible generic overloads)

    protected async Task<T> ExecuteGetAsync<T>(string endpoint,
                                               Dictionary<string, string>? headers = null,
                                               Dictionary<string, string>? queryParams = null) {
        var fullEndpoint = BuildEndpoint(endpoint, queryParams);
        return await ExecuteHttpRequestAsync<T>(fullEndpoint, null, headers, HttpMethod.Get).ConfigureAwait(false);
    }

    protected async Task<T> ExecutePostAsync<TBody, T>(string endpoint,
                                                        TBody body,
                                                        Dictionary<string, string>? headers = null) {
        return await ExecuteHttpRequestAsync<T>(endpoint, JsonExtend.Serialize(body), headers, HttpMethod.Post).ConfigureAwait(false);
    }

    protected async Task<T> ExecutePutAsync<TBody, T>(string endpoint,
                                                       TBody body,
                                                       Dictionary<string, string>? headers = null) {
        return await ExecuteHttpRequestAsync<T>(endpoint, JsonExtend.Serialize(body), headers, HttpMethod.Put).ConfigureAwait(false);
    }

    protected async Task<T> ExecutePatchAsync<TBody, T>(string endpoint,
                                                         TBody body,
                                                         Dictionary<string, string>? headers = null) {
        return await ExecuteHttpRequestAsync<T>(endpoint, JsonExtend.Serialize(body), headers, HttpMethod.Patch).ConfigureAwait(false);
    }

    protected async Task<T> ExecuteDeleteAsync<TBody, T>(string endpoint,
                                                          TBody body,
                                                          Dictionary<string, string>? headers = null)
        => await ExecuteHttpRequestAsync<T>(endpoint, JsonExtend.Serialize(body), headers, HttpMethod.Delete).ConfigureAwait(false);

    protected async Task<T> ExecuteHeadAsync<TBody, T>(string endpoint,
                                                        TBody body,
                                                        Dictionary<string, string>? headers = null)
        => await ExecuteHttpRequestAsync<T>(endpoint, JsonExtend.Serialize(body), headers, HttpMethod.Head).ConfigureAwait(false);

    protected Task<HttpResponse<T>> ExecutePostRawAsync<TBody, T>(string endpoint,
                                                                   TBody body,
                                                                   Dictionary<string, string>? headers = null) =>
        ExecuteHttpRequestRawAsync<T>(endpoint, JsonExtend.Serialize(body), headers, HttpMethod.Post);

    protected Task<HttpResponse<T>> ExecuteGetRawAsync<T>(string endpoint,
                                                          Dictionary<string, string>? headers = null,
                                                          Dictionary<string, string>? queryParams = null) {
        var fullEndpoint = BuildEndpoint(endpoint, queryParams);
        return ExecuteHttpRequestRawAsync<T>(fullEndpoint, null, headers, HttpMethod.Get);
    }

    protected Task<HttpResponse<T>> ExecutePutRawAsync<TBody, T>(string endpoint,
                                                                  TBody body,
                                                                  Dictionary<string, string>? headers = null)
        => ExecuteHttpRequestRawAsync<T>(endpoint, JsonExtend.Serialize(body), headers, HttpMethod.Put);

    protected Task<HttpResponse<T>> ExecutePatchRawAsync<TBody, T>(string endpoint,
                                                                    TBody body,
                                                                    Dictionary<string, string>? headers = null)
        => ExecuteHttpRequestRawAsync<T>(endpoint, JsonExtend.Serialize(body), headers, HttpMethod.Patch);

    protected Task<HttpResponse<T>> ExecuteDeleteRawAsync<TBody, T>(string endpoint,
                                                                     TBody body,
                                                                     Dictionary<string, string>? headers = null)
        => ExecuteHttpRequestRawAsync<T>(endpoint, JsonExtend.Serialize(body), headers, HttpMethod.Delete);

    protected Task<HttpResponse<T>> ExecuteHeadRawAsync<T>(string endpoint,
                                                           Dictionary<string, string>? headers = null)
        => ExecuteHttpRequestRawAsync<T>(endpoint, null, headers, HttpMethod.Head);

    protected async Task<T> ExecutePostAsync<T>(string endpoint,
                                                Dictionary<string, string>? headers = null)
        => await ExecuteHttpRequestAsync<T>(endpoint, null, headers, HttpMethod.Post).ConfigureAwait(false);

    protected async Task<T> ExecutePutAsync<T>(string endpoint,
                                               Dictionary<string, string>? headers = null)
        => await ExecuteHttpRequestAsync<T>(endpoint, null, headers, HttpMethod.Put).ConfigureAwait(false);

    protected async Task<T> ExecutePatchAsync<T>(string endpoint,
                                                 Dictionary<string, string>? headers = null)
        => await ExecuteHttpRequestAsync<T>(endpoint, null, headers, HttpMethod.Patch).ConfigureAwait(false);

    protected async Task<T> ExecuteDeleteAsync<T>(string endpoint,
                                                  Dictionary<string, string>? headers = null)
        => await ExecuteHttpRequestAsync<T>(endpoint, null, headers, HttpMethod.Delete).ConfigureAwait(false);

    protected async Task<T> ExecuteHeadAsync<T>(string endpoint,
                                                Dictionary<string, string>? headers = null)
        => await ExecuteHttpRequestAsync<T>(endpoint, null, headers, HttpMethod.Head).ConfigureAwait(false);

    protected Task<HttpResponse<T>> ExecutePostRawAsync<T>(string endpoint,
                                                           Dictionary<string, string>? headers = null)
        => ExecuteHttpRequestRawAsync<T>(endpoint, null, headers, HttpMethod.Post);

    protected Task<HttpResponse<T>> ExecutePutRawAsync<T>(string endpoint,
                                                          Dictionary<string, string>? headers = null)
        => ExecuteHttpRequestRawAsync<T>(endpoint, null, headers, HttpMethod.Put);

    protected Task<HttpResponse<T>> ExecutePatchRawAsync<T>(string endpoint,
                                                            Dictionary<string, string>? headers = null)
        => ExecuteHttpRequestRawAsync<T>(endpoint, null, headers, HttpMethod.Patch);

    protected Task<HttpResponse<T>> ExecuteDeleteRawAsync<T>(string endpoint,
                                                             Dictionary<string, string>? headers = null)
        => ExecuteHttpRequestRawAsync<T>(endpoint, null, headers, HttpMethod.Delete);

    #endregion


    private string ResolveTraceId(Dictionary<string, string>? headers) {
        if (headers is not null && headers.TryGetValue("X-Trace-ID", out var existingTraceId))
            return existingTraceId;
        return _currentUserService.GetXtraceId();
    }

    protected async Task<HttpResponse<T>> ExecuteHttpRequestRawAsync<T>(string endpoint,
                                                                        string? serializedBody,
                                                                        Dictionary<string, string>? headers,
                                                                        HttpMethod method) {
        HttpAuditEntity? auditEntity = null;
        var traceId = ResolveTraceId(headers);

        if (_enableAuditing && _httpRequestRepository != null)
            auditEntity = await CreateAuditEntity(traceId, endpoint, method, serializedBody, headers).ConfigureAwait(false);

        try {
            _logger.LogInformation("Executing {Method} request to {Endpoint}", method, endpoint);

            var httpResponse = await ExecuteHttpMethod<T>(endpoint, serializedBody, headers, method).ConfigureAwait(false);

            if (auditEntity != null && _httpRequestRepository != null)
                await UpdateAuditEntityAsync(auditEntity, httpResponse).ConfigureAwait(false);

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
                                                     string? serializedBody,
                                                     Dictionary<string, string>? headers,
                                                     HttpMethod method) {
        var httpResponse = await ExecuteHttpRequestRawAsync<T>(endpoint, serializedBody, headers, method).ConfigureAwait(false);

        if (!IsSuccessStatus(httpResponse.StatusCode)) {
            _logger.LogWarning(
                "HTTP request failed with status {StatusCode} for endpoint {Endpoint}",
                httpResponse.StatusCode, endpoint);
            var baseUrl = _httpClient.BaseAddress?.ToString() ?? "Unknown";
            var traceId = ResolveTraceId(headers);
            HandleCustomResponse(httpResponse, traceId, $"{baseUrl}{endpoint}");
        }

        return httpResponse.Data!;
    }

    private async Task<HttpResponse<T>> ExecuteHttpMethod<T>(string endpoint,
                                                              string? serializedBody,
                                                              Dictionary<string, string>? headers,
                                                              HttpMethod method) {
        try {
            var requestMessage = CreateHttpRequestMessage(method, endpoint, serializedBody, headers);
            var (response, elapsedMs) = await SendTimedRequestAsync(requestMessage).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK)
                return await ParseSuccessResponseAsync<T>(response, elapsedMs).ConfigureAwait(false);

            var responseJson = JsonExtend.ToJsonDocument(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            return new HttpResponse<T>(response.StatusCode, elapsedMs, responseJson);
        } catch (Exception ex) {
            return HandleExecutionException<T>(ex);
        }
    }

    private static HttpRequestMessage CreateHttpRequestMessage(HttpMethod method,
                                                                string endpoint,
                                                                string? serializedBody,
                                                                Dictionary<string, string>? headers) {
        var message = new HttpRequestMessage(method, endpoint);
        if (serializedBody is not null)
            message.Content = CreateContent(serializedBody);
        AddHeaders(message, headers);
        return message;
    }

    private async Task<(HttpResponseMessage Response, long ElapsedMs)> SendTimedRequestAsync(
        HttpRequestMessage requestMessage) {
        _timer.Restart();
        var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        _timer.Stop();
        return (response, _timer.ElapsedMilliseconds);
    }

    private static async Task<HttpResponse<T>> ParseSuccessResponseAsync<T>(
        HttpResponseMessage response, long elapsedMs) {
        var responseJson = JsonExtend.ToJsonDocument(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
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
                JsonExtend.ToJsonDocument<Dictionary<string, object>>(new Dictionary<string, object> { { "Error", "Making HTTP request." } })) {
                ErrorMessage = httpEx.Message
            },
            SerializerException serEx => new HttpResponse<T>(
                HttpStatusCode.InternalServerError,
                _timer.ElapsedMilliseconds,
                JsonExtend.ToJsonDocument<Dictionary<string, object>>(new Dictionary<string, object> { { "Error", "Error deserializing response." } })) {
                ErrorMessage = serEx.MessageLog.ToString()
            },
            _ => new HttpResponse<T>(
                HttpStatusCode.InternalServerError,
                _timer.ElapsedMilliseconds,
                JsonExtend.ToJsonDocument<Dictionary<string, object>>(new Dictionary<string, object> { { "Error", ex.Message } }))
        };
    }

    private static StringContent CreateContent(string serializedBody) {
        return new StringContent(serializedBody, Encoding.UTF8, "application/json");
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
                                                          string? serializedBody,
                                                          Dictionary<string, string>? headers) {
        var baseUrl = _httpClient.BaseAddress?.ToString() ?? "Unknown";
        var bodyDoc = serializedBody is not null ? JsonDocument.Parse(serializedBody) : null;
        var headersDoc = headers is not null ? JsonExtend.ToJsonDocument<Dictionary<string, string>>(headers) : null;
        var auditEntity = new HttpAuditEntity(
            Guid.Parse(traceId),
            $"{baseUrl}{endpoint}",
            method,
            bodyDoc,
            headersDoc);
        await _httpRequestRepository!.AddAsync(auditEntity).ConfigureAwait(false);
        return auditEntity;
    }

    private async Task UpdateAuditEntityAsync<T>(HttpAuditEntity auditEntity,
                                                 HttpResponse<T> httpResponse) {
        auditEntity.ElapsedMilliseconds = httpResponse.Time;
        auditEntity.StatusCode = httpResponse.StatusCode;
        auditEntity.Response = httpResponse.Response;
        auditEntity.InternalError = httpResponse.ErrorMessage;
        auditEntity.UpdatedAt = DateTime.Now;
        auditEntity.UpdatedBy = _currentUserService.GetUserName();

        await _httpRequestRepository!.UpdateAsync(auditEntity).ConfigureAwait(false);
    }


    private static string BuildEndpoint(string endpoint, Dictionary<string, string>? queryParams) {
        if (queryParams == null || queryParams.Count == 0)
            return endpoint;
        var queryString = string.Join("&",
            queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{endpoint}?{queryString}";
    }

    /// <summary>
    /// Declarative mapping of HTTP status codes to error codes and messages.
    /// </summary>
    private static readonly FrozenDictionary<HttpStatusCode, (string Code, string Message)> _httpErrorMappings =
        new Dictionary<HttpStatusCode, (string Code, string Message)> {
            [HttpStatusCode.BadRequest] = ("HTTP001", "Bad request"),
            [HttpStatusCode.Unauthorized] = ("HTTP002", "Unauthorized"),
            [HttpStatusCode.Forbidden] = ("HTTP003", "Forbidden"),
            [HttpStatusCode.InternalServerError] = ("HTTP004", "Internal server error"),
            [HttpStatusCode.BadGateway] = ("HTTP005", "Bad gateway"),
            [HttpStatusCode.ServiceUnavailable] = ("HTTP006", "Service unavailable"),
            [HttpStatusCode.TooManyRequests] = ("HTTP007", "Rate limit exceeded"),
        }.ToFrozenDictionary();

    private static readonly (string Code, string Message) _defaultHttpError = ("HTTP008", "HTTP request failed");

    protected virtual void HandleCustomResponse<T>(HttpResponse<T> httpResponse,
                                                        string traceId,
                                                        string endpoint,
                                                        [CallerMemberName] string memberName = "",
                                                        [CallerFilePath] string sourceFilePath = "",
                                                        [CallerLineNumber] int sourceLineNumber = 0) {
        if (httpResponse.StatusCode == HttpStatusCode.NotFound)
            throw new NotFoundException();

        var errorContext = new Dictionary<string, object> {
            { "TraceId", traceId },
            { "Endpoint", endpoint },
            { "StatusCode", httpResponse.StatusCode },
            { "ElapsedMilliseconds", httpResponse.Time },
            { "ErrorMessage", httpResponse.ErrorMessage ?? "None" },
            { "Response", httpResponse.Response ?? JsonDocument.Parse("{}") }
        };

        var errorMessage = JsonExtend.Serialize(errorContext);
        var (code, message) = _httpErrorMappings.GetValueOrDefault(httpResponse.StatusCode, _defaultHttpError);
        var providerMessage = code == "HTTP004"
            ? httpResponse.ErrorMessage ?? errorMessage
            : errorMessage;

        throw new CustomException(
            new(code, message, providerMessage),
            memberName, sourceFilePath, sourceLineNumber);
    }
}

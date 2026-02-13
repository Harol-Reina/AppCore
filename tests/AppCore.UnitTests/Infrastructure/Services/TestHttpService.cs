using OrionSoft.AppCore.Application.Interfaces;
using OrionSoft.AppCore.Application.Wrappers;
using OrionSoft.AppCore.Domain.Interfaces;
using OrionSoft.AppCore.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace OrionSoft.AppCore.UnitTests.Infrastructure.Services;

internal class TestHttpService : HttpService {
    public TestHttpService(HttpClient httpClient,
                           ICurrentUserService currentUserService,
                           ILogger<TestHttpService> logger,
                           IHttpRequestRepository? httpRequestRepository = null)
        : base(httpClient, currentUserService, logger, httpRequestRepository) {
    }

    public new Task<T> ExecuteGetAsync<T>(string endpoint,
                                          Dictionary<string, string>? headers = null,
                                          Dictionary<string, string>? queryParams = null)
        => base.ExecuteGetAsync<T>(endpoint, headers, queryParams);

    public new Task<T> ExecutePostAsync<T>(string endpoint,
                                           Dictionary<string, string>? headers = null)
        => base.ExecutePostAsync<T>(endpoint, headers);

    public new Task<T> ExecutePostAsync<TBody, T>(string endpoint,
                                               TBody body,
                                               Dictionary<string, string>? headers = null)
        => base.ExecutePostAsync<TBody, T>(endpoint, body, headers);

    public new Task<T> ExecutePutAsync<T>(string endpoint,
                                          Dictionary<string, string>? headers = null)
        => base.ExecutePutAsync<T>(endpoint, headers);

    public new Task<T> ExecutePutAsync<TBody, T>(string endpoint,
                                              TBody body,
                                              Dictionary<string, string>? headers = null)
        => base.ExecutePutAsync<TBody, T>(endpoint, body, headers);

    public new Task<T> ExecuteDeleteAsync<T>(string endpoint,
                                             Dictionary<string, string>? headers = null)
        => base.ExecuteDeleteAsync<T>(endpoint, headers);

    public new Task<T> ExecuteDeleteAsync<TBody, T>(string endpoint,
                                                 TBody body,
                                                 Dictionary<string, string>? headers = null)
        => base.ExecuteDeleteAsync<TBody, T>(endpoint, body, headers);

    public new Task<T> ExecutePatchAsync<T>(string endpoint,
                                            Dictionary<string, string>? headers = null)
        => base.ExecutePatchAsync<T>(endpoint, headers);

    public new Task<T> ExecutePatchAsync<TBody, T>(string endpoint,
                                                TBody body,
                                                Dictionary<string, string>? headers = null)
        => base.ExecutePatchAsync<TBody, T>(endpoint, body, headers);

    public new Task<T> ExecuteHeadAsync<T>(string endpoint,
                                           Dictionary<string, string>? headers = null)
        => base.ExecuteHeadAsync<T>(endpoint, headers);

    public new Task<T> ExecuteHeadAsync<TBody, T>(string endpoint,
                                               TBody body,
                                               Dictionary<string, string>? headers = null)
        => base.ExecuteHeadAsync<TBody, T>(endpoint, body, headers);

    public new Task<HttpResponse<T>> ExecuteGetRawAsync<T>(string endpoint,
                                                            Dictionary<string, string>? headers = null,
                                                            Dictionary<string, string>? queryParams = null)
        => base.ExecuteGetRawAsync<T>(endpoint, headers, queryParams);

    public new Task<HttpResponse<T>> ExecutePostRawAsync<T>(string endpoint,
                                                             Dictionary<string, string>? headers = null)
        => base.ExecutePostRawAsync<T>(endpoint, headers);

    public new Task<HttpResponse<T>> ExecutePostRawAsync<TBody, T>(string endpoint,
                                                                TBody body,
                                                                Dictionary<string, string>? headers = null)
        => base.ExecutePostRawAsync<TBody, T>(endpoint, body, headers);

    public new Task<HttpResponse<T>> ExecutePutRawAsync<T>(string endpoint,
                                                            Dictionary<string, string>? headers = null)
        => base.ExecutePutRawAsync<T>(endpoint, headers);

    public new Task<HttpResponse<T>> ExecutePutRawAsync<TBody, T>(string endpoint,
                                                               TBody body,
                                                               Dictionary<string, string>? headers = null)
        => base.ExecutePutRawAsync<TBody, T>(endpoint, body, headers);

    public new Task<HttpResponse<T>> ExecutePatchRawAsync<T>(string endpoint,
                                                              Dictionary<string, string>? headers = null)
        => base.ExecutePatchRawAsync<T>(endpoint, headers);

    public new Task<HttpResponse<T>> ExecutePatchRawAsync<TBody, T>(string endpoint,
                                                                 TBody body,
                                                                 Dictionary<string, string>? headers = null)
        => base.ExecutePatchRawAsync<TBody, T>(endpoint, body, headers);

    public new Task<HttpResponse<T>> ExecuteDeleteRawAsync<T>(string endpoint,
                                                               Dictionary<string, string>? headers = null)
        => base.ExecuteDeleteRawAsync<T>(endpoint, headers);

    public new Task<HttpResponse<T>> ExecuteDeleteRawAsync<TBody, T>(string endpoint,
                                                                  TBody body,
                                                                  Dictionary<string, string>? headers = null)
        => base.ExecuteDeleteRawAsync<TBody, T>(endpoint, body, headers);

    public new Task<HttpResponse<T>> ExecuteHeadRawAsync<T>(string endpoint,
                                                             Dictionary<string, string>? headers = null)
        => base.ExecuteHeadRawAsync<T>(endpoint, headers);

    public void InvokeHandleCustomResponse<T>(HttpResponse<T> response, string traceId, string endpoint)
        => HandleCustomResponse(response, traceId, endpoint);

    public ICurrentUserService GetExposedCurrentUserService() => currentUserService;
}

using AppCore.Application.Interfaces;
using AppCore.Domain.Interfaces;
using AppCore.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace AppCore.UnitTests.Infrastructure.Services;

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
                                           object? body = null,
                                           Dictionary<string, string>? headers = null)
        => base.ExecutePostAsync<T>(endpoint, body, headers);

    public new Task<T> ExecutePutAsync<T>(string endpoint,
                                          object? body = null,
                                          Dictionary<string, string>? headers = null)
        => base.ExecutePutAsync<T>(endpoint, body, headers);

    public new Task<T> ExecuteDeleteAsync<T>(string endpoint,
                                             object? body = null,
                                             Dictionary<string, string>? headers = null)
        => base.ExecuteDeleteAsync<T>(endpoint, body, headers);

    public new Task<T> ExecutePatchAsync<T>(string endpoint,
                                            object? body = null,
                                            Dictionary<string, string>? headers = null)
        => base.ExecutePatchAsync<T>(endpoint, body, headers);

    public new Task<T> ExecuteHeadAsync<T>(string endpoint,
                                           object? body = null,
                                           Dictionary<string, string>? headers = null)
        => base.ExecuteHeadAsync<T>(endpoint, body, headers);
}

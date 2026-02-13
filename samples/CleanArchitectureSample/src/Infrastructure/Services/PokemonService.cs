using System.Net;
using App.Application.Common;
using App.Application.Domain.Entities;
using App.Application.DTOs.Response;
using App.Application.Interfaces.Services;
using OrionSoft.AppCore.Application.Extensions;
using OrionSoft.AppCore.Application.Interfaces;
using OrionSoft.AppCore.Domain.Interfaces;
using OrionSoft.AppCore.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Services;

public class PokemonService : HttpService, IPokeService {
    public PokemonService(HttpClient httpClient,
                          ICurrentUserService currentUserService,
                          IHttpRequestRepository httpRequestRepository,
                          ILogger<PokemonService> logger,
                          AppSettings appSettings) : base(httpClient, currentUserService, logger, httpRequestRepository) {
        httpClient.BaseAddress = new Uri(appSettings.PokemonHost);
    }

    public async Task<List<PokemonEntity>> GetAllAsync() {
        var traceId = currentUserService.GetXtraceId();
        var headers = new Dictionary<string, string>{
            { "X-Trace-ID", traceId! }
        };

        var result = await ExecuteGetAsync<PokemonResponse>(
            $"pokemon/",
            headers);
        return result.Results;
    }

}

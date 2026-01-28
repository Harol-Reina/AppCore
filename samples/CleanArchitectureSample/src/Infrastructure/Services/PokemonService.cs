using System.Net;
using App.Application.Common;
using App.Application.Domain.Entities;
using App.Application.DTOs.Response;
using App.Application.Interfaces.Services;
using AppCore.Application.Extensions;
using AppCore.Application.Interfaces;
using AppCore.Domain.Interfaces;
using AppCore.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Services;

public class PokemonService : HttpService, IPokeService {
    public PokemonService(HttpClient httpClient,
                          ICurrentUserService currentUserService,
                          IHttpRequestRepository httpRequestRepository,
                          ILogger<PokemonService> logger) : base(httpClient, currentUserService, logger, httpRequestRepository) {
        httpClient.BaseAddress = new Uri(AppConstants.PokemonHost);
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

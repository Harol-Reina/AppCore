using App.ApiRest.EndPoinds;
using OrionSoft.AppCore.Application.Interfaces;

namespace App.ApiRest.Extensions;

public static class WebApplicationExtensions {
    public static void MapEndpoints(this WebApplication app) {
        // Explicit registration is required for AOT compatibility
        app.MapGroupInner<EmployesEndpoint>();
        app.MapGroupInner<PokemonsEndpoint>();
    }

    private static void MapGroupInner<T>(this WebApplication app) where T : IEndpointGroupBase, new() {
        var instance = new T();
        var typeName = typeof(T).Name;
        var groupName = typeName.Replace("Endpoint", "");
        var routeGroup = app.MapGroup($"/api/v1/{groupName.ToLower()}");
        instance.MapEndpoints(routeGroup, groupName);
    }


}

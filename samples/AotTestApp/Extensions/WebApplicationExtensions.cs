using AotTestApp.Endpoints;
using AppCore.Application.Interfaces;

namespace AotTestApp.Extensions;

public static class WebApplicationExtensions {
    public static void MapEndpoints(this WebApplication app) {
        app.MapGroupInner<ExceptionsEndpoint>();
    }

    private static void MapGroupInner<T>(this WebApplication app) where T : IEndpointGroupBase, new() {
        var instance = new T();
        var typeName = typeof(T).Name;
        var groupName = typeName.Replace("Endpoint", "");
        var routeGroup = app.MapGroup($"/api/v1/{groupName.ToLower()}");
        instance.MapEndpoints(routeGroup, groupName);
    }
}

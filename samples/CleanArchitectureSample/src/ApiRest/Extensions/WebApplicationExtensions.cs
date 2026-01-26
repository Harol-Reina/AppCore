using System.Reflection;
using AppCore.Application.Interfaces;

namespace App.ApiRest.Extensions;

public static class WebApplicationExtensions {
    public static void MapEndpoints(this WebApplication app) {
        var interfaceType = typeof(IEndpointGroupBase);
        var endpointGroupTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => interfaceType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var type in endpointGroupTypes) {
            if (Activator.CreateInstance(type) is IEndpointGroupBase instance) {
                var groupName = GetGroupName(type.Name);
                var routeGroup = app.MapGroup($"/api/v1/{groupName.ToLower()}");
                instance.MapEndpoints(routeGroup, groupName);
            }
        }
    }

    private static string GetGroupName(string typeName) {
        var groupName = typeName.Replace("Endpoint", "");
        return groupName;
    }
}

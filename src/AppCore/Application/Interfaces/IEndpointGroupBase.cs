using Microsoft.AspNetCore.Routing;

namespace AppCore.Application.Interfaces;

public interface IEndpointGroupBase {
    void MapEndpoints(RouteGroupBuilder group, string groupName);
}

using Microsoft.AspNetCore.Routing;

namespace OrionSoft.AppCore.Application.Interfaces;

public interface IEndpointGroupBase {
    void MapEndpoints(RouteGroupBuilder group, string groupName);
}

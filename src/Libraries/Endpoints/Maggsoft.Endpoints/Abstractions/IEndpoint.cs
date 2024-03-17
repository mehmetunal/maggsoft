using Microsoft.AspNetCore.Routing;

namespace Maggsoft.Endpoints.Abstractions;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

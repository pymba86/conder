using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conder.Gateway
{
    public interface IDownstreamBuilder
    {
        string GetDownstream(RouteConfig routeConfig, HttpRequest request, RouteData data);
    }
}
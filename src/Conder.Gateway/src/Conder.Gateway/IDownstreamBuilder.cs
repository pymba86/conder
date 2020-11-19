using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conder.Gateway
{
    internal interface IDownstreamBuilder
    {
        string GetDownstream(RouteConfig routeConfig, HttpRequest request, RouteData data);
    }
}
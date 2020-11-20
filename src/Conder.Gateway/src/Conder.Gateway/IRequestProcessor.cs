using Microsoft.AspNetCore.Http;

namespace Conder.Gateway
{
    public interface IRequestProcessor
    {
        ExecutionData Process(RouteConfig routeConfig, HttpContext context);
    }
}
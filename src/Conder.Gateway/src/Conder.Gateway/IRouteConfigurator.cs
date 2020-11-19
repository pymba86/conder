using Conder.Gateway.Configuration;

namespace Conder.Gateway
{
    internal interface IRouteConfigurator
    {
        RouteConfig Configure(Module module, Route route);
    }
}
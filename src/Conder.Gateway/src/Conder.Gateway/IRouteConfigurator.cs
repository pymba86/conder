using Conder.Gateway.Configuration;

namespace Conder.Gateway
{
    public interface IRouteConfigurator
    {
        RouteConfig Configure(Module module, Route route);
    }
}
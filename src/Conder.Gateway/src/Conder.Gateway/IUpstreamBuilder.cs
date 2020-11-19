using Conder.Gateway.Configuration;

namespace Conder.Gateway
{
    internal interface IUpstreamBuilder
    {
        string Build(Module module, Route route);
    }
}
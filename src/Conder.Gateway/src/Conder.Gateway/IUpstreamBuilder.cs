using Conder.Gateway.Configuration;

namespace Conder.Gateway
{
    public interface IUpstreamBuilder
    {
        string Build(Module module, Route route);
    }
}
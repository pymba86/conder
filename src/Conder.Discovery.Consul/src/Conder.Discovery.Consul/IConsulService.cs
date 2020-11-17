using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Conder.Discovery.Consul.Models;

namespace Conder.Discovery.Consul
{
    public interface IConsulService
    {
        Task<HttpResponseMessage> RegisterServiceAsync(ServiceRegistration registration);
        Task<HttpResponseMessage> DeregisterServiceAsync(string id);
        Task<IDictionary<string, ServiceAgent>> GetServiceAgentsAsync(string service = null);
    }
}
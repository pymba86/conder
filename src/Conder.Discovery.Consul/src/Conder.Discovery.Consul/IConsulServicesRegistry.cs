using System.Threading.Tasks;
using Conder.Discovery.Consul.Models;

namespace Conder.Discovery.Consul
{
    public interface IConsulServicesRegistry
    {
        Task<ServiceAgent> GetAsync(string name);
    }
}
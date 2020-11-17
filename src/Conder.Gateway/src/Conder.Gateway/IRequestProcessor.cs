using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Conder.Gateway
{
    public interface IRequestProcessor
    {
        Task<ExecutionData> ProcessAsync(RouteConfig routeConfig, HttpContext context);
    }
}
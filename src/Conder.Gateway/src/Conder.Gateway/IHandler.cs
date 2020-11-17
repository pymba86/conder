using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Conder.Gateway.Configuration;

namespace Conder.Gateway
{
    public interface IHandler
    {
        string GetInfo(Route route);
        Task HandleAsync(HttpContext context, RouteConfig config);
    }
}
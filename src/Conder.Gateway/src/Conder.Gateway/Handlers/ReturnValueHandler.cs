using System.Threading.Tasks;
using Conder.Gateway.Configuration;
using Microsoft.AspNetCore.Http;

namespace Conder.Gateway.Handlers
{
    internal sealed class ReturnValueHandler : IHandler
    {
        public string GetInfo(Route route)
            => $"return a value: '{route.ReturnValue}'";

        public async Task HandleAsync(HttpContext context, RouteConfig config)
        {
            await context.Response.WriteAsync(config.Route?.ReturnValue ?? string.Empty);
        }
    }
}
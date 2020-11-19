using System;
using Conder.Gateway.Options;
using Microsoft.AspNetCore.Routing;

namespace Conder.Gateway.Requests
{
    public class RouterProvider : IRouteProvider
    {
        private readonly GatewayOptions _options;

        public RouterProvider(GatewayOptions options)
        {
            _options = options;
        }

        public Action<IEndpointRouteBuilder> Build() => routerBuilder =>
        {
           
        };
    }
}
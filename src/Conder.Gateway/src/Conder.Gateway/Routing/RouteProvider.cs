using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Conder.Gateway.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Conder.Gateway.Routing
{
    public class RouteProvider : IRouteProvider
    {
        private readonly GatewayOptions _options;
        private readonly IDictionary<string, Action<IEndpointRouteBuilder, string, RouteConfig>> _methods;
        private readonly ILogger<RouteProvider> _logger;
        private readonly IRequestHandlerManager _requestHandlerManager;
        private readonly IUpstreamBuilder _upstreamBuilder;
        private readonly IRouteConfigurator _routeConfigurator;

        public RouteProvider(GatewayOptions options, ILogger<RouteProvider> logger,
            IRequestHandlerManager requestHandlerManager, IUpstreamBuilder upstreamBuilder,
            IRouteConfigurator routeConfigurator)
        {
            _options = options;
            _logger = logger;
            _requestHandlerManager = requestHandlerManager;
            _upstreamBuilder = upstreamBuilder;
            _routeConfigurator = routeConfigurator;
            _methods = new Dictionary<string, Action<IEndpointRouteBuilder, string, RouteConfig>>
            {
                ["get"] = (builder, path, routeConfig) =>
                    builder.MapGet(path, ctx => Handle(ctx, routeConfig)),
                ["post"] = (builder, path, routeConfig) =>
                    builder.MapPost(path, ctx => Handle(ctx, routeConfig)),
                ["put"] = (builder, path, routeConfig) =>
                    builder.MapPut(path, ctx => Handle(ctx, routeConfig)),
                ["delete"] = (builder, path, routeConfig) =>
                    builder.MapDelete(path, ctx => Handle(ctx, routeConfig))
            };
        }

        private async Task Handle(HttpContext context, RouteConfig routeConfig)
        {
            await _requestHandlerManager.HandleAsync(
                routeConfig.Route.Use, context, routeConfig);
        }

        public Action<IEndpointRouteBuilder> Build() => routerBuilder =>
        {
            var enabledModules = _options.Modules
                .Where(m => m.Value.Enabled != false);

            foreach (var (name, module) in enabledModules)
            {
                _logger.LogInformation($"Building routes for a the module: '{name}'");

                foreach (var route in module.Routes)
                {
                    if (string.IsNullOrWhiteSpace(route.Method) && route.Methods is null)
                    {
                        throw new ArgumentException(
                            "Both, route 'method' and 'methods' cannot be empty");
                    }

                    route.Upstream = _upstreamBuilder.Build(module, route);

                    var routeConfig = _routeConfigurator.Configure(module, route);

                    if (!string.IsNullOrWhiteSpace(route.Method))
                    {
                        _methods[route.Method](routerBuilder, route.Upstream, routeConfig);
                    }

                    if (route.Methods is null)
                    {
                        continue;
                    }

                    foreach (var method in route.Methods)
                    {
                        var methodType = method.ToLowerInvariant();
                        _methods[methodType](routerBuilder, route.Upstream, routeConfig);
                    }
                }
            }
        };
    }
}
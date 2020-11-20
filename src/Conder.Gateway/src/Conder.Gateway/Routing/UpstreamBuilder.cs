using System.Collections.Generic;
using Conder.Gateway.Configuration;
using Microsoft.Extensions.Logging;

namespace Conder.Gateway.Routing
{
    public class UpstreamBuilder : IUpstreamBuilder
    {
        private readonly IRequestHandlerManager _requestHandlerManager;
        private readonly ILogger<UpstreamBuilder> _logger;

        public UpstreamBuilder(ILogger<UpstreamBuilder> logger,
            IRequestHandlerManager requestHandlerManager)
        {
            _logger = logger;
            _requestHandlerManager = requestHandlerManager;
        }

        public string Build(Module module, Route route)
        {
            var path = module.Path;
            var upstream = string.IsNullOrWhiteSpace(route.Upstream)
                ? string.Empty : route.Upstream;

            if (!string.IsNullOrWhiteSpace(path))
            {
                var modulePath = path.EndsWith("/")
                    ? path.Substring(0, path.Length - 1)
                    : path;

                if (!upstream.StartsWith("/"))
                {
                    upstream = $"/{upstream}";
                }

                upstream = $"{modulePath}{upstream}";
            }

            if (upstream.EndsWith("/"))
            {
                upstream = upstream.Substring(0, upstream.Length - 1);
            }

            if (route.MatchAll)
            {
                upstream = $"{upstream}/{{*url}}";
            }

            var handler = _requestHandlerManager.Get(route.Use);
            var routeInfo = handler.GetInfo(route);
            
            var methods = new HashSet<string>();

            if (!string.IsNullOrWhiteSpace(route.Method))
            {
                methods.Add(route.Method.ToUpperInvariant());
            }

            if (route.Methods is {})
            {
                foreach (var method in route.Methods)
                {
                    if (string.IsNullOrWhiteSpace(method))
                    {
                        continue;
                    }

                    methods.Add(method.ToUpperInvariant());
                }
            }
            
            _logger.LogInformation($"Added route for upstream [{string.Join(", ", methods)}]" +
                                   $"'{upstream}' -> {routeInfo}");

            return upstream;
        }
    }
}
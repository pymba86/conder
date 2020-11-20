using System;
using System.Collections.Generic;
using System.Linq;
using Conder.Gateway.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conder.Gateway.Requests
{
    public class RequestProcessor : IRequestProcessor
    {
        private static readonly IDictionary<string, string> EmptyClaims = new Dictionary<string, string>();
        private const string ContentTypeApplicationJson = "application/json";
        private const string ContentTypeTextPlain = "text/plain";
        private const string ContentTypeHeader = "Content-Type";

        private readonly GatewayOptions _options;
        private readonly IDownstreamBuilder _downstreamBuilder;

        public RequestProcessor(IDownstreamBuilder downstreamBuilder, GatewayOptions options)
        {
            _downstreamBuilder = downstreamBuilder;
            _options = options;
        }

        public ExecutionData Process(RouteConfig routeConfig, HttpContext context)
        {
            context.Request.Headers.TryGetValue(ContentTypeHeader, out var contentType);
            
            var contentTypeValue = contentType.ToString().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(contentTypeValue) || contentTypeValue.Contains(ContentTypeTextPlain))
            {
                contentTypeValue = ContentTypeApplicationJson;
            }

            var (requestId, resourceId, traceId) = GenerateIds(context.Request, routeConfig);

            var routeData = context.GetRouteData();
            
            var executionData = new ExecutionData
            {
                RequestId = requestId,
                ResourceId = resourceId,
                TraceId = traceId,
                UserId = context.Request.HttpContext.User?.Identity?.Name,
                Claims = context.Request.HttpContext.User?.Claims?
                    .ToDictionary(c => c.Type, c => c.Value) ?? EmptyClaims,
                ContentType =  contentTypeValue,
                Route = routeConfig.Route,
                Context = context,
                Data = routeData,
                Downstream = _downstreamBuilder.GetDownstream(routeConfig, context.Request, routeData)
            };

            return executionData;
        }

        private (string, string, string) GenerateIds(HttpRequest request, RouteConfig routeConfig)
        {
            var requestId = string.Empty;
            var resourceId = string.Empty;
            var traceId = string.Empty;

            if (routeConfig.Route.GenerateRequestId == true ||
                _options.GenerateRequestId == true && routeConfig.Route.GenerateRequestId != false)
            {
                requestId = Guid.NewGuid().ToString("N");
            }

            if (!(request.Method is "GET" || request.Method is "DELETE") &&
                (routeConfig.Route.ResourceId?.Generate == true ||
                 _options.ResourceId?.Generate == true && routeConfig.Route.ResourceId?.Generate != false))
            {
                resourceId = Guid.NewGuid().ToString("N");
            }

            if (routeConfig.Route.GenerateTraceId == true ||
                _options.GenerateTraceId == true && routeConfig.Route.GenerateTraceId != false)
            {
                traceId = request.HttpContext.TraceIdentifier;
            }

            return (requestId, resourceId, traceId);
        }
    }
}
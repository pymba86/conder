using System.Collections.Generic;
using Conder.Gateway.Configuration;
using Conder.Gateway.Extensions;

namespace Conder.Gateway.Options
{
    public class GatewayOptions : IOptions
    {
        public Http Http { get; set; }
        public bool UseForwardedHeaders { get; set; }
        public bool? PassQueryString { get; set; }
        public ResourceId ResourceId { get; set; }
        public LoadBalancer LoadBalancer { get; set; }
        public bool? ForwardRequestHeaders { get; set; }
        public bool? ForwardResponseHeaders { get; set; }
        public bool? ForwardStatusCode { get; set; }
        public IDictionary<string, string> RequestHeaders { get; set; }
        public IDictionary<string, string> ResponseHeaders { get; set; }
        public IDictionary<string, Module> Modules { get; set; }
        public bool? GenerateRequestId { get; set; }
        public bool? GenerateTraceId { get; set; }
        public bool UseLocalUrl { get; set; }
        public IDictionary<string, ExtensionOptions> Extensions { get; set; }
    }
}
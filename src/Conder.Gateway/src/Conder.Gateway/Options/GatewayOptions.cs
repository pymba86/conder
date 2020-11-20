using System.Collections.Generic;
using Conder.Gateway.Configuration;

namespace Conder.Gateway.Options
{
    public class GatewayOptions : IOptions
    {
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
    }
}
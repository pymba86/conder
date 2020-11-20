using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Route = Conder.Gateway.Configuration.Route;

namespace Conder.Gateway
{
    public class ExecutionData
    {
        public string RequestId { get; set; }
        public string ResourceId { get; set; }
        public string TraceId { get; set; }
        public string UserId { get; set; }
        public IDictionary<string,string> Claims { get; set; }
        public string ContentType { get; set; }
        public Route Route { get; set; }
        public HttpContext Context { get; set; }
        public RouteData Data { get; set; }
        public string Downstream { get; set; }
    }
}
using System.Collections.Generic;

namespace Conder.Gateway.Configuration
{
    public class Route
    {
        public ResourceId ResourceId { get; set; }
        public string Upstream { get; set; }
        public string Downstream { get; set; }
        public string Use { get; set; }
        public string Method { get; set; }
        public IEnumerable<string> Methods { get; set; }
        public bool MatchAll { get; set; }
        public bool? PassQueryString { get; set; }
        public IDictionary<string, string> RequestHeaders { get; set; }
        public IDictionary<string, string> ResponseHeaders { get; set; }
    }
}
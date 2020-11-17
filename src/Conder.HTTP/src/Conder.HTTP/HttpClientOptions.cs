using System.Collections.Generic;

namespace Conder.HTTP
{
    public class HttpClientOptions
    {
        public string Type { get; set; }
        public int Retries { get; set; }
        public IDictionary<string, string> Services { get; set; }
        public bool RemoveCharsetFromContentType { get; set; }
        public string CorrelationContextHeader { get; set; }
        public string CorrelationIdHeader { get; set; }
    }
}
using System.Net.Http;
using Conder.HTTP;

namespace Conder.Discovery.Consul.Http
{
    internal sealed class ConsulHttpClient : ConderHttpClient, IConsulHttpClient 
    {
        public ConsulHttpClient(HttpClient client, HttpClientOptions options,
            ICorrelationContextFactory correlationContextFactory,
            ICorrelationIdFactory correlationIdFactory)
            : base(client, options, correlationContextFactory, correlationIdFactory)
        {
        }
    }
}
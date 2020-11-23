using System.Net.Http;
using Conder.HTTP;

namespace Conder.LoadBalancing.Fabio.Http
{
    public class FabioHttpClient : ConderHttpClient, IFabioHttpClient
    {
        public FabioHttpClient(HttpClient client, HttpClientOptions options,
            ICorrelationContextFactory correlationContextFactory,
            ICorrelationIdFactory correlationIdFactory)
            : base(client, options, correlationContextFactory, correlationIdFactory)
        {
        }
    }
}
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Conder.LoadBalancing.Fabio.MessageHandlers
{
    public class FabioMessageHandler : DelegatingHandler
    {
        private readonly FabioOptions _options;
        private readonly string _serviceName;

        public FabioMessageHandler(FabioOptions options, string serviceName = null)
        {
            if (string.IsNullOrWhiteSpace(options.Url))
            {
                throw new InvalidOperationException("Fabio url was not provided");
            }

            _options = options;
            _serviceName = string.IsNullOrWhiteSpace(serviceName) ? string.Empty : $"{serviceName}/";
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
            {
                return base.SendAsync(request, cancellationToken);
            }

            request.RequestUri = GetRequestUri(request);

            return base.SendAsync(request, cancellationToken);
        }

        private Uri GetRequestUri(HttpRequestMessage request)
            => new Uri($"{_options.Url}/{_serviceName}{request.RequestUri.Host}{request.RequestUri.PathAndQuery}");
    }
}
using System.Text;
using Conder.Gateway.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Conder.Gateway.Routing
{
    public class DownstreamBuilder : IDownstreamBuilder
    {
        private readonly GatewayOptions _options;
        private readonly IValueProvider _valueProvider;

        public DownstreamBuilder(IValueProvider valueProvider, GatewayOptions options)
        {
            _valueProvider = valueProvider;
            _options = options;
        }

        public string GetDownstream(RouteConfig routeConfig, HttpRequest request, RouteData data)
        {
            if (string.IsNullOrWhiteSpace(routeConfig.Downstream))
            {
                return null;
            }
            
            var stringBuilder = new StringBuilder();
            var downstream = routeConfig.Downstream;

            stringBuilder.Append(downstream);

            if (downstream.Contains("@"))
            {
                foreach (var token in _valueProvider.Tokens)
                {
                    var tokenName = $"@{token}";
                    stringBuilder.Replace(tokenName, _valueProvider.Get(tokenName, request, data));
                }
            }

            foreach (var (key, value) in data.Values)
            {
                if (value is null)
                {
                    continue;
                }

                if (key is "url")
                {
                    stringBuilder.Append($"/{value}");
                    continue;
                }

                stringBuilder.Replace($"{{{key}}}", value.ToString());
            }

            if (_options.PassQueryString == false || routeConfig.Route.PassQueryString == false)
            {
                return stringBuilder.ToString();
            }

            var queryString = request.QueryString.ToString();
            if (downstream.Contains("?") && !string.IsNullOrWhiteSpace(queryString))
            {
                queryString = $"&{queryString.Substring(1, queryString.Length - 1)}";
            }

            stringBuilder.Append(queryString);

            return stringBuilder.ToString();
        }
    }
}
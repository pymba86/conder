using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Conder.Gateway.Configuration;
using Conder.Gateway.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Conder.Gateway.Handlers
{
    public class DownstreamHandler : IHandler
    {
        private const string ContentTypeApplicationJson = "application/json";
        private const string ContentTypeHeader = "Content-Type";
        private static readonly string[] ExcludedResponseHeaders = {"transfer-encoding", "content-length"};

        private static readonly HttpContent EmptyContent =
            new StringContent("{}", Encoding.UTF8, ContentTypeApplicationJson);

        private readonly IRequestProcessor _requestProcessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DownstreamHandler> _logger;
        private readonly GatewayOptions _options;

        public DownstreamHandler(ILogger<DownstreamHandler> logger, GatewayOptions options,
            IRequestProcessor requestProcessor, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _options = options;
            _requestProcessor = requestProcessor;
            _httpClientFactory = httpClientFactory;
        }

        public string GetInfo(Route route)
            => $"call the downstream '{route.Downstream}'";

        public async Task HandleAsync(HttpContext context, RouteConfig config)
        {
            if (config.Route.Downstream is null)
            {
                return;
            }

            var executionData = _requestProcessor.Process(config, context);

            if (string.IsNullOrWhiteSpace(executionData.Downstream))
            {
                return;
            }

            _logger.LogInformation(
                $"Sending HTTP {context.Request.Method} request to: {config.Downstream} " +
                $"[Trace ID: {context.TraceIdentifier}]");

            var response = await SendRequestAsync(executionData);

            if (response is null)
            {
                _logger.LogWarning($"Did not receive HTTP response for: {executionData.Route.Downstream}");
                return;
            }

            await WriteResponseAsync(context.Response, response, executionData);
        }

        private async Task<HttpResponseMessage> SendRequestAsync(ExecutionData executionData)
        {
            var httpClient = _httpClientFactory.CreateClient("gateway");
            var method = (string.IsNullOrWhiteSpace(executionData.Route.DownstreamMethod)
                ? executionData.Context.Request.Method
                : executionData.Route.DownstreamMethod).ToLowerInvariant();

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(executionData.Downstream)
            };

            if (executionData.Route.ForwardRequestHeaders == true ||
                _options.ForwardRequestHeaders == true && executionData.Route.ForwardRequestHeaders != false)
            {
                foreach (var (key, value) in executionData.Context.Request.Headers)
                {
                    request.Headers.TryAddWithoutValidation(key, value.ToArray());
                }
            }

            var includeBody = false;

            switch (method)
            {
                case "get":
                    request.Method = HttpMethod.Get;
                    break;
                case "post":
                    request.Method = HttpMethod.Post;
                    includeBody = true;
                    break;
                case "put":
                    request.Method = HttpMethod.Put;
                    includeBody = true;
                    break;
                case "patch":
                    request.Method = HttpMethod.Patch;
                    includeBody = true;
                    break;
                case "delete":
                    request.Method = HttpMethod.Delete;
                    break;
                case "head":
                    request.Method = HttpMethod.Head;
                    break;
                case "options":
                    request.Method = HttpMethod.Options;
                    break;
                case "trace":
                    request.Method = HttpMethod.Trace;
                    break;
                default:
                    return null;
            }

            if (!includeBody)
            {
                return await httpClient.SendAsync(request);
            }

            using var content = GetHttpContent(executionData);
            request.Content = content;

            return await httpClient.SendAsync(request);
        }

        private static HttpContent GetHttpContent(ExecutionData executionData)
        {
            var contentType = executionData.ContentType;
            
            if (executionData.Context.Request.Body is null)
            {
                return EmptyContent;
            }

            var httpContent = new StreamContent(executionData.Context.Request.Body);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return httpContent;
        }

        private async Task WriteResponseAsync(HttpResponse response, HttpResponseMessage httpResponse,
            ExecutionData executionData)
        {
            var traceId = executionData.Context.Request.HttpContext.TraceIdentifier;
            var method = executionData.Context.Request.Method;

            if (!string.IsNullOrWhiteSpace(executionData.RequestId))
            {
                response.Headers.Add("Request-ID", executionData.RequestId);
            }

            if (!string.IsNullOrWhiteSpace(executionData.ResourceId)
                && executionData.Context.Request.Method is "POST")
            {
                response.Headers.Add("Resource-ID", executionData.ResourceId);
            }

            if (!string.IsNullOrWhiteSpace(executionData.TraceId))
            {
                response.Headers.Add("Trace-ID", executionData.TraceId);
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    $"Received an invalid response ({httpResponse.StatusCode})" +
                    $"to HTTP {method} request from {executionData.Route.Downstream}" +
                    $"[Trace ID: {traceId}]");

                await SetErrorResponseAsync(response, httpResponse, executionData);
                return;
            }

            _logger.LogInformation($"Received the successful response ({httpResponse.StatusCode}) +" +
                                   $"to HTTP {method} request from: {executionData.Route.Downstream}" +
                                   $"[Trace ID: {traceId}]");

            await SetSuccessResponseAsync(response, httpResponse, executionData);
        }

        private static async Task SetErrorResponseAsync(HttpResponse response, HttpResponseMessage httpResponse,
            ExecutionData executionData)
        {
            var content = await httpResponse.Content.ReadAsStringAsync();
            if (executionData.Context.Request.Method is "GET"
                && !response.Headers.ContainsKey(ContentTypeHeader))
            {
                response.Headers[ContentTypeHeader] = ContentTypeApplicationJson;
            }

            response.StatusCode = 400;

            await response.WriteAsync(content);
        }

        private async Task SetSuccessResponseAsync(HttpResponse response, HttpResponseMessage httpResponse,
            ExecutionData executionData)
        {
            var content = await httpResponse.Content.ReadAsStringAsync();

            if (_options.ForwardStatusCode == false || executionData.Route.ForwardStatusCode == false)
            {
                response.StatusCode = 200;
            }
            else
            {
                response.StatusCode = (int) httpResponse.StatusCode;
            }

            if (executionData.Route.ForwardResponseHeaders == true ||
                _options.ForwardResponseHeaders == true 
                && executionData.Route.ForwardResponseHeaders != false)
            {
                foreach (var (key, value) in httpResponse.Headers)
                {
                    if (ExcludedResponseHeaders.Contains(key.ToLowerInvariant()))
                    {
                        continue;
                    }

                    if (response.Headers.ContainsKey(key))
                    {
                        continue;
                    }

                    response.Headers.Add(key, value.ToArray());
                }

                foreach (var (key, value) in httpResponse.Content.Headers)
                {
                    if (ExcludedResponseHeaders.Contains(key.ToLowerInvariant()))
                    {
                        continue;
                    }

                    if (response.Headers.ContainsKey(key))
                    {
                        continue;
                    }

                    response.Headers.Add(key, value.ToArray());
                }
            }

            var responseHeaders = executionData.Route.ResponseHeaders is null ||
                                  !executionData.Route.ResponseHeaders.Any()
                ? _options.ResponseHeaders ?? new Dictionary<string, string>()
                : executionData.Route.ResponseHeaders;

            foreach (var (key, value) in responseHeaders)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(value))
                {
                    response.Headers.Remove(key);
                    response.Headers.Add(key, value);
                    continue;
                }

                if (!httpResponse.Headers.TryGetValues(key, out var values))
                {
                    continue;
                }

                response.Headers.Remove(key);
                response.Headers.Add(key, values.ToArray());
            }

            if (executionData.Context.Request.Method is "GET" 
                && !response.Headers.ContainsKey(ContentTypeHeader))
            {
                response.Headers[ContentTypeHeader] = ContentTypeApplicationJson;
            }

            if (response.StatusCode != 204)
            {
                await response.WriteAsync(content);
            }
        }
    }
}
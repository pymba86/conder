using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Conder.HTTP
{
    public static class Extensions
    {
        private const string SectionName = "httpClient";
        private const string RegistryName = "http.client";

        public static IConderBuilder AddHttpClient(this IConderBuilder builder, string clientName = "conder",
            string sectionName = SectionName,
            Action<IHttpClientBuilder> httpClientBuilder = null)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            if (string.IsNullOrWhiteSpace(clientName))
            {
                throw new ArgumentException("HTTP client name cannot be empty.", nameof(clientName));
            }

            var options = builder.GetOptions<HttpClientOptions>(sectionName);


            builder.Services.AddSingleton<ICorrelationContextFactory, EmptyCorrelationContextFactory>();
            builder.Services.AddSingleton<ICorrelationIdFactory, EmptyCorrelationIdFactory>();
            builder.Services.AddSingleton(options);
            var clientBuilder = builder.Services.AddHttpClient<IHttpClient, ConderHttpClient>(clientName);
            httpClientBuilder?.Invoke(clientBuilder);


            return builder;
        }
        
        public static void RemoveHttpClient(this IConderBuilder builder)
        {
            var registryType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .SingleOrDefault(t => t.Name == "HttpClientMappingRegistry");
            
            var registry = builder.Services.SingleOrDefault(
                s => s.ServiceType == registryType)?.ImplementationInstance;
            
            var registrations = registry?.GetType()
                .GetProperty("TypedClientRegistrations");
            
            var clientRegistrations = registrations?.GetValue(registry) as IDictionary<Type, string>;
            
            clientRegistrations?.Remove(typeof(IHttpClient));
        }
    }
}
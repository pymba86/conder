using System;
using System.Collections.Generic;
using System.Linq;
using Conder.Discovery.Consul;
using Conder.Discovery.Consul.Models;
using Conder.HTTP;
using Conder.LoadBalancing.Fabio.Builders;
using Conder.LoadBalancing.Fabio.Http;
using Conder.LoadBalancing.Fabio.MessageHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Conder.LoadBalancing.Fabio
{
    public static class Extensions
    {
        private const string SectionName = "fabio";
        private const string RegistryName = "loadBalancing.fabio";

        public static IConderBuilder AddFabio(this IConderBuilder builder, string sectionName = SectionName,
            string consulSectionName = "consul", string httpClientSectionName = "httpClient")
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var fabioOptions = builder.GetOptions<FabioOptions>(sectionName);
            var consulOptions = builder.GetOptions<ConsulOptions>(consulSectionName);
            var httpClientOptions = builder.GetOptions<HttpClientOptions>(httpClientSectionName);

            return builder.AddFabio(fabioOptions, httpClientOptions,
                b => b.AddConsul(consulOptions, httpClientOptions));
        }

        public static IConderBuilder AddFabio(this IConderBuilder builder,
            Func<IFabioOptionsBuilder, IFabioOptionsBuilder> buildOptions,
            Func<IConsulOptionsBuilder, IConsulOptionsBuilder> buildConsulOptions,
            HttpClientOptions httpClientOptions)
        {
            var fabioOptions = buildOptions(new FabioOptionsBuilder()).Build();

            return builder.AddFabio(fabioOptions, httpClientOptions,
                b => b.AddConsul(buildConsulOptions, httpClientOptions));
        }

        public static IConderBuilder AddFabio(this IConderBuilder builder, FabioOptions fabioOptions,
            ConsulOptions consulOptions, HttpClientOptions httpClientOptions)
            => builder.AddFabio(fabioOptions, httpClientOptions, 
                b => b.AddConsul(consulOptions, httpClientOptions));

        public static void AddFabioHttpClient(this IConderBuilder builder, string clientName, string serviceName)
            => builder.Services.AddHttpClient<IHttpClient, FabioHttpClient>(clientName)
                .AddHttpMessageHandler(c
                    => new FabioMessageHandler(c.GetService<FabioOptions>(), serviceName));

        private static IConderBuilder AddFabio(this IConderBuilder builder, FabioOptions fabioOptions,
            HttpClientOptions httpClientOptions, Action<IConderBuilder> registerConsul)
        {
            registerConsul(builder);
            builder.Services.AddSingleton(fabioOptions);

            if (!fabioOptions.Enabled || !builder.TryRegister(RegistryName))
            {
                return builder;
            }

            if (httpClientOptions.Type?.ToLowerInvariant() == "fabio")
            {
                builder.Services.AddTransient<FabioMessageHandler>();
                builder.Services.AddHttpClient<IFabioHttpClient, FabioHttpClient>("fabio-http")
                    .AddHttpMessageHandler<FabioMessageHandler>();

                builder.RemoveHttpClient();
                builder.Services.AddHttpClient<IHttpClient, FabioHttpClient>("fabio")
                    .AddHttpMessageHandler<FabioMessageHandler>();
            }

            using var serviceProvider = builder.Services.BuildServiceProvider();
            var registration = serviceProvider.GetService<ServiceRegistration>();
            var tags = GetFabioTags(registration.Name, fabioOptions.Service);

            if (registration.Tags is null)
            {
                registration.Tags = tags;
            }
            else
            {
                registration.Tags.AddRange(tags);
            }

            builder.Services.UpdateConsulRegistration(registration);

            return builder;
        }

        private static void UpdateConsulRegistration(this IServiceCollection services,
            ServiceRegistration registration)
        {
            var serviceDescriptor = services.FirstOrDefault(
                sd => sd.ServiceType == typeof(ServiceRegistration));

            services.Remove(serviceDescriptor);
            services.AddSingleton(registration);
        }

        private static List<string> GetFabioTags(string consulService, string fabioService)
        {
            var service = (string.IsNullOrWhiteSpace(fabioService) ? consulService : fabioService)
                .ToLowerInvariant();

            return new List<string> {$"urlprefix-/{service} strip=/{service}"};
        }
    }
}
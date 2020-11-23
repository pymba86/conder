using System;
using System.Linq;
using Conder.Discovery.Consul;
using Conder.Discovery.Consul.Models;
using Conder.HTTP;
using Microsoft.Extensions.DependencyInjection;

namespace Conder.Proxy.Traefik
{
    public static class Extensions
    {
        private const string SectionName = "traefik";
        private const string RegistryName = "proxy.traefik";

        public static IConderBuilder AddTraefik(this IConderBuilder builder, string sectionName = SectionName,
            string consulSectionName = "consul", string httpClientSectionName = "httpClient")
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var traefikOptions = builder.GetOptions<TraefikOptions>(sectionName);
            var consulOptions = builder.GetOptions<ConsulOptions>(consulSectionName);
            var httpClientOptions = builder.GetOptions<HttpClientOptions>(httpClientSectionName);

            return builder.AddTraefik(traefikOptions,
                b => b.AddConsul(consulOptions, httpClientOptions));
        }

        private static IConderBuilder AddTraefik(this IConderBuilder builder, TraefikOptions traefikOptions,
            Action<IConderBuilder> registerTraefik)
        {
            registerTraefik(builder);
            builder.Services.AddSingleton(traefikOptions);

            if (!traefikOptions.Enabled || !builder.TryRegister(RegistryName))
            {
                return builder;
            }

            using var serviceProvider = builder.Services.BuildServiceProvider();

            var registration = serviceProvider.GetService<ServiceRegistration>();

            var tags = traefikOptions.Tags;

            if (tags is null)
            {
                throw new ArgumentException(
                    "Traefik 'tags' cannot be empty");
            }

            if (registration.Tags is null)
            {
                registration.Tags = tags.ToList();
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
    }
}
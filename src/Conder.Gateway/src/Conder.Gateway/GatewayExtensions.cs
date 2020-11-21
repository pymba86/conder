using System;
using System.Linq;
using Conder.Gateway.Configuration;
using Conder.Gateway.Extensions;
using Conder.Gateway.Handlers;
using Conder.Gateway.Options;
using Conder.Gateway.Requests;
using Conder.Gateway.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;

namespace Conder.Gateway
{
    public static class GatewayExtensions
    {
        public static IConderBuilder AddGateway(this IConderBuilder builder)
        {
            var (configuration, optionsProvider) = BuildConfiguration(builder.Services);

            builder.Services.AddCoreServices()
                .ConfigureHttpClient(configuration)
                .AddGatewayServices()
                .AddExtensions(optionsProvider);

            return builder;
        }

        private static (GatewayOptions, OptionsProvider) BuildConfiguration(IServiceCollection services)
        {
            IConfiguration config;
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                config = scope.ServiceProvider.GetService<IConfiguration>();
            }

            var optionsProvider = new OptionsProvider(config);
            services.AddSingleton<IOptionsProvider>(optionsProvider);
            var options = optionsProvider.Get<GatewayOptions>();
            services.AddSingleton(options);

            return (options, optionsProvider);
        }

        private static IServiceCollection ConfigureHttpClient(this IServiceCollection services, GatewayOptions options)
        {
            var http = options.Http ?? new Http();
            var httpClientBuilder = services.AddHttpClient("gateway");

            httpClientBuilder.AddTransientHttpErrorPolicy(p =>
                p.WaitAndRetryAsync(http.Retries, retryAttempt =>
                {
                    var interval = http.Exponential
                        ? Math.Pow(http.Interval, retryAttempt)
                        : http.Interval;

                    return TimeSpan.FromSeconds(interval);
                }));

            return services;
        }

        public static IApplicationBuilder UseGateway(this IApplicationBuilder builder)
        {
            var options = builder.ApplicationServices.GetRequiredService<GatewayOptions>();
            var logger = builder.ApplicationServices.GetRequiredService<ILogger<Gateway>>();

            if (options.UseForwardedHeaders)
            {
                logger.LogInformation("Headers forwarding is enabled");
                builder.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.All
                });
            }

            if (options.LoadBalancer?.Enabled == true)
            {
                logger.LogInformation($"Load balancer is enabled: {options.LoadBalancer.Url}");
            }

            builder.UseExtensions();
            builder.RegisterRequestHandlers();
            builder.AddRoutes();

            return builder;
        }

        private static void RegisterRequestHandlers(this IApplicationBuilder builder)
        {
            var logger = builder.ApplicationServices
                .GetRequiredService<ILogger<Gateway>>();

            var options = builder.ApplicationServices
                .GetRequiredService<GatewayOptions>();

            var requestHandlerManager = builder.ApplicationServices
                .GetRequiredService<IRequestHandlerManager>();

            requestHandlerManager.AddHandler("downstream",
                builder.ApplicationServices.GetRequiredService<DownstreamHandler>());

            requestHandlerManager.AddHandler("return_value",
                builder.ApplicationServices.GetRequiredService<ReturnValueHandler>());

            if (options.Modules is null)
            {
                return;
            }

            var handlers = options.Modules
                .Select(m => m.Value)
                .SelectMany(m => m.Routes)
                .Select(r => r.Use)
                .Distinct()
                .ToArray();

            foreach (var handler in handlers)
            {
                if (requestHandlerManager.Get(handler) is null)
                {
                    throw new Exception($"Handler: '{handler}' was not defined");
                }

                logger.LogInformation($"Added handler: '{handler}'");
            }
        }

        private static void AddRoutes(this IApplicationBuilder builder)
        {
            var options = builder.ApplicationServices.GetRequiredService<GatewayOptions>();
            if (options.Modules is null)
            {
                return;
            }

            foreach (var route in options.Modules.SelectMany(m => m.Value.Routes))
            {
                if (route.Methods is {})
                {
                    if (route.Methods.Any(m => m.Equals(route.Method,
                        StringComparison.InvariantCultureIgnoreCase)))
                    {
                        throw new ArgumentException("There's already a method" +
                                                    $"{route.Method.ToUpperInvariant()} declared in route 'methods'");
                    }

                    continue;
                }

                route.Method = (string.IsNullOrWhiteSpace(route.Method) ? "get" : route.Method).ToLowerInvariant();
                route.DownstreamMethod =
                    (string.IsNullOrWhiteSpace(route.DownstreamMethod) ? route.Method : route.DownstreamMethod)
                    .ToLowerInvariant();
            }

            var routeProvider = builder.ApplicationServices.GetRequiredService<IRouteProvider>();

            builder.UseRouting();
            builder.UseEndpoints(routeProvider.Build());
        }

        private static IServiceCollection AddGatewayServices(this IServiceCollection services)
        {
            services.AddSingleton<IRequestHandlerManager, RequestHandlerManager>();
            services.AddSingleton<IDownstreamBuilder, DownstreamBuilder>();
            services.AddSingleton<IRequestProcessor, RequestProcessor>();
            services.AddSingleton<IRouteConfigurator, RouteConfigurator>();
            services.AddSingleton<IRouteProvider, RouteProvider>();
            services.AddSingleton<IUpstreamBuilder, UpstreamBuilder>();
            services.AddSingleton<IValueProvider, ValueProvider>();

            services.AddSingleton<DownstreamHandler>();
            services.AddSingleton<ReturnValueHandler>();

            return services;
        }

        private static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddMvcCore()
                .AddNewtonsoftJson(o =>
                    o.SerializerSettings.Formatting = Formatting.Indented);

            return services;
        }

        private static IServiceCollection AddExtensions(this IServiceCollection services,
            IOptionsProvider optionsProvider)
        {
            var options = optionsProvider.Get<GatewayOptions>();
            var extensionProvider = new ExtensionProvider(options);

            services.AddSingleton<IExtensionProvider>(extensionProvider);

            foreach (var extension in extensionProvider.GetAll())
            {
                if (extension.Options.Enabled == true)
                {
                    extension.Extension.Add(services, optionsProvider);
                }
            }

            return services;
        }

        private static void UseExtensions(this IApplicationBuilder builder)
        {
            var logger = builder.ApplicationServices.GetRequiredService<ILogger<Gateway>>();
            var optionsProvider = builder.ApplicationServices.GetRequiredService<IOptionsProvider>();
            var extensionProvider = builder.ApplicationServices.GetRequiredService<IExtensionProvider>();

            foreach (var extension in extensionProvider.GetAll())
            {
                if (extension.Options.Enabled == false)
                {
                    continue;
                }

                extension.Extension.Use(builder, optionsProvider);

                var orderMessage = extension.Options.Order.HasValue
                    ? $" [order: {extension.Options.Order}]"
                    : string.Empty;

                logger.LogInformation($"Enabled extension: '{extension.Extension.Name}' +" +
                                      $"({extension.Extension.Description}){orderMessage}");
            }
        }

        private class Gateway
        {
        }
    }
}
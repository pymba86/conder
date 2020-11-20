using System;
using System.Linq;
using Conder.Gateway.Handlers;
using Conder.Gateway.Options;
using Conder.Gateway.Requests;
using Conder.Gateway.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Conder.Gateway
{
    public static class GatewayExtensions
    {
        public static IConderBuilder AddGateway(this IConderBuilder builder)
        {
            return builder;
        }

        public static IApplicationBuilder UseGateway(this IApplicationBuilder builder)
        {
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

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
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
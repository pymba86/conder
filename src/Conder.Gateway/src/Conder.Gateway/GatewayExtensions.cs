using Conder.Gateway.Handlers;
using Conder.Gateway.Requests;
using Conder.Gateway.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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
            return builder;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IRequestProcessor, RequestProcessor>();
            services.AddSingleton<IRouteConfigurator, RouteConfigurator>();
            services.AddSingleton<IRouteProvider, RouteProvider>();

            services.AddSingleton<DownstreamHandler>();
            services.AddSingleton<ReturnValueHandler>();

            return services;
        }
    }
}
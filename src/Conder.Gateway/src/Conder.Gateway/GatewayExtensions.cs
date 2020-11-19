using Microsoft.AspNetCore.Builder;

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
    }
}
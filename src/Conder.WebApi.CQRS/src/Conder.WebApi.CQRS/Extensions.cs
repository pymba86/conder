using System;
using Conder.WebApi.CQRS.Builders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Conder.WebApi.CQRS
{
    public static class Extensions
    {
        public static IApplicationBuilder UseDispatcherEndpoints(this IApplicationBuilder app,
            Action<IDispatcherEndpointsBuilder> builder, bool useAuthorization = true,
            Action<IApplicationBuilder> middleware = null)
        {
            var definitions = app.ApplicationServices.GetService<WebApiEndpointDefinitions>();

            app.UseRouting();

            if (useAuthorization)
            {
                app.UseAuthorization();
            }

            middleware?.Invoke(app);

            app.UseEndpoints(router => builder(
                new DispatcherEndpointsBuilder(
                    new EndpointsBuilder(router, definitions))));

            return app;
        }
    }
}
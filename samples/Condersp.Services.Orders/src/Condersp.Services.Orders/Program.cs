using System.Threading.Tasks;
using Conder.Discovery.Consul;
using Conder.Docs.Swagger;
using Conder.LoadBalancing.Fabio;
using Conder.Logging;
using Conder.Metrics.AppMetrics;
using Conder.Proxy.Traefik;
using Conder.WebApi;
using Conder.WebApi.Swagger;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Conder.Samples.Services.Orders
{
    public static class Program
    {
        public static async Task Main(string[] args)
            => await CreateHostBuilder(args)
                .Build()
                .RunAsync();

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services => services
                            .AddConder()
                            .AddErrorHandler<ExceptionToResponseMapper>()
                            .AddWebApi()
                            .AddWebApiSwaggerDocs()
                            .AddConsul()
                            .AddFabio()
                            .AddTraefik()
                            .AddMetrics()
                            .Build())
                        .Configure(app => app
                            .UseConder()
                            .UseErrorHandler()
                            .UseSwaggerDocs()
                            .UseMetrics()
                            .UseEndpoints(endpoints => endpoints
                                .Get("", ctx => ctx.Response.WriteAsync("Orders service"))
                            )
                        )
                        .UseLogging();
                });
    }
}
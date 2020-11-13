using System.Threading.Tasks;
using Conder.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Conder.Samples.Services.Orders
{
    public class Program
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
                            .Build())
                        .Configure(app => app
                            .UseConder()
                            .UseErrorHandler()
                            .UseEndpoints(endpoints => endpoints
                                .Get("/", ctx => ctx.Response.WriteAsync("Order service"))
                            )
                        );
                });
    }
}
using System.Threading.Tasks;
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services => services
                            .AddConder()
                            .Build())
                        .Configure(app => app
                            .UseConder()
                            .UseRouting()
                            .UseEndpoints(endpoints => endpoints
                                .MapGet("/",
                                    async context => { await context.Response.WriteAsync("Hello World!"); })
                            )
                        );
                });
    }
}
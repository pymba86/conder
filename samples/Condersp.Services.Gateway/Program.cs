using System.Threading.Tasks;
using Conder;
using Conder.Gateway;
using Conder.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Condersp.Services.Gateway
{
    public static class Program
    {
        public static Task Main(string[] args)
            => CreateHostBuilder(args).Build().RunAsync();

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services => services.AddConder()
                            .AddGateway())
                        .Configure(app => app.UseConder()
                            .UseGateway())
                        .UseLogging();
                });
    }
}
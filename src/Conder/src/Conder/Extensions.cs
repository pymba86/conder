using System;
using System.Threading.Tasks;
using Conder.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Conder
{
    public static class Extensions
    {
        private const string SectionName = "app";

        public static IConderBuilder AddConder(this IServiceCollection services, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var builder = ConderBuilder.Create(services);

            var options = builder.GetOptions<AppOptions>(sectionName);

            builder.Services.AddMemoryCache();

            services.AddSingleton(options);
            services.AddSingleton<IServiceId, ServiceId>();
            
            services.Configure<HostOptions>(host =>
                host.ShutdownTimeout = TimeSpan.FromSeconds(options.ShutdownTimeout));

            if (!options.DisplayBanner || string.IsNullOrWhiteSpace(options.Name))
            {
                return builder;
            }

            var version = options.DisplayVersion ? $" {options.Version}" : string.Empty;

            Console.WriteLine(Figgle.FiggleFonts.Doom.Render($"{options.Name}{version}"));

            return builder;
        }

        public static IApplicationBuilder UseConder(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var initializer = scope.ServiceProvider.GetRequiredService<IStartupInitializer>();
            Task.Run(() => initializer.InitializeAsync()).GetAwaiter().GetResult();
            return app;
        }

        public static TModel GetOptions<TModel>(this IConfiguration configuration, string sectionName)
            where TModel : new()
        {
            var model = new TModel();
            configuration.GetSection(sectionName).Bind(model);
            return model;
        }

        public static TModel GetOptions<TModel>(this IConderBuilder builder, string sectionName)
            where TModel : new()
        {
            using var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            return configuration.GetOptions<TModel>(sectionName);
        }
    }
}
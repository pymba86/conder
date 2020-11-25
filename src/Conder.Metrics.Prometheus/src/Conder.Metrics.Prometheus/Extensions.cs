using Conder.Metrics.Prometheus.Internals;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Prometheus.SystemMetrics;

namespace Conder.Metrics.Prometheus
{
    public static class Extensions
    {
        private const string SectionName = "prometheus";
        
        public static IConderBuilder AddPrometheus(this IConderBuilder builder,
            string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }
            
            var prometheusOptions = builder.GetOptions<PrometheusOptions>(sectionName);
            
            builder.Services.AddSingleton(prometheusOptions);
            if (!prometheusOptions.Enabled)
            {
                return builder;
            }

            builder.Services.AddHostedService<PrometheusJob>();
            builder.Services.AddSingleton<PrometheusMiddleware>();
            builder.Services.AddSystemMetrics();

            return builder;
            
        }
        
        public static IApplicationBuilder UsePrometheus(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetRequiredService<PrometheusOptions>();
            if (!options.Enabled)
            {
                return app;
            }

            var endpoint = string.IsNullOrWhiteSpace(options.Endpoint) ? "/metrics" :
                options.Endpoint.StartsWith("/") ? options.Endpoint : $"/{options.Endpoint}";

            return app
                .UseMiddleware<PrometheusMiddleware>()
                .UseHttpMetrics()
                .UseGrpcMetrics()
                .UseMetricServer(endpoint);
        }
    }
}
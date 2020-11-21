﻿using System;
using System.Collections.Generic;
using System.Linq;
using Conder.Logging.Options;
using Conder.Types;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.Elasticsearch;

namespace Conder.Logging
{
 public static class Extensions
    {
        private const string LoggerSectionName = "logger";
        private const string AppSectionName = "app";
        internal static readonly LoggingLevelSwitch LoggingLevelSwitch = new LoggingLevelSwitch();

        public static IHostBuilder UseLogging(this IHostBuilder hostBuilder,
            Action<LoggerConfiguration> configure = null, string loggerSectionName = LoggerSectionName,
            string appSectionName = AppSectionName)
            => hostBuilder
                .ConfigureServices(services => services.AddSingleton<ILoggingService, LoggingService>())
                .UseSerilog((context, loggerConfiguration) =>
            {
                if (string.IsNullOrWhiteSpace(loggerSectionName))
                {
                    loggerSectionName = LoggerSectionName;
                }

                if (string.IsNullOrWhiteSpace(appSectionName))
                {
                    appSectionName = AppSectionName;
                }

                var loggerOptions = context.Configuration.GetOptions<LoggerOptions>(loggerSectionName);
                var appOptions = context.Configuration.GetOptions<AppOptions>(appSectionName);

                MapOptions(loggerOptions, appOptions, loggerConfiguration, context.HostingEnvironment.EnvironmentName);
                configure?.Invoke(loggerConfiguration);
            });

        public static IWebHostBuilder UseLogging(this IWebHostBuilder webHostBuilder,
            Action<LoggerConfiguration> configure = null, string loggerSectionName = LoggerSectionName,
            string appSectionName = AppSectionName)
            => webHostBuilder
                .ConfigureServices(services => services.AddSingleton<ILoggingService, LoggingService>())
                .UseSerilog((context, loggerConfiguration) =>
                {
                    if (string.IsNullOrWhiteSpace(loggerSectionName))
                    {
                        loggerSectionName = LoggerSectionName;
                    }

                    if (string.IsNullOrWhiteSpace(appSectionName))
                    {
                        appSectionName = AppSectionName;
                    }

                    var loggerOptions = context.Configuration.GetOptions<LoggerOptions>(loggerSectionName);
                    var appOptions = context.Configuration.GetOptions<AppOptions>(appSectionName);

                    MapOptions(loggerOptions, appOptions, loggerConfiguration,
                        context.HostingEnvironment.EnvironmentName);
                    configure?.Invoke(loggerConfiguration);
                });
        

        private static void MapOptions(LoggerOptions loggerOptions, AppOptions appOptions,
            LoggerConfiguration loggerConfiguration, string environmentName)
        {
            LoggingLevelSwitch.MinimumLevel = GetLogEventLevel(loggerOptions.Level);

            loggerConfiguration.Enrich.FromLogContext()
                .MinimumLevel.ControlledBy(LoggingLevelSwitch)
                .Enrich.WithProperty("Environment", environmentName)
                .Enrich.WithProperty("Application", appOptions.Service)
                .Enrich.WithProperty("Instance", appOptions.Instance)
                .Enrich.WithProperty("Version", appOptions.Version);

            foreach (var (key, value) in loggerOptions.Tags ?? new Dictionary<string, object>())
            {
                loggerConfiguration.Enrich.WithProperty(key, value);
            }

            foreach (var (key, value) in loggerOptions.MinimumLevelOverrides ?? new Dictionary<string, string>())
            {
                var logLevel = GetLogEventLevel(value);
                loggerConfiguration.MinimumLevel.Override(key, logLevel);
            }

            loggerOptions.ExcludePaths?.ToList().ForEach(p => loggerConfiguration.Filter
                .ByExcluding(Matching.WithProperty<string>("RequestPath", 
                    n => n.EndsWith(p))));

            loggerOptions.ExcludeProperties?.ToList().ForEach(p => loggerConfiguration.Filter
                .ByExcluding(Matching.WithProperty(p)));

            Configure(loggerConfiguration, loggerOptions);
        }

        private static void Configure(LoggerConfiguration loggerConfiguration,
            LoggerOptions options)
        {
            var consoleOptions = options.Console ?? new ConsoleOptions();
            var fileOptions = options.File ?? new FileOptions();
            var elkOptions = options.Elk ?? new ElkOptions();

            if (consoleOptions.Enabled)
            {
                loggerConfiguration.WriteTo.Console();
            }

            if (fileOptions.Enabled)
            {
                var path = string.IsNullOrWhiteSpace(fileOptions.Path) ? "logs/logs.txt" : fileOptions.Path;
                if (!Enum.TryParse<RollingInterval>(fileOptions.Interval, true, out var interval))
                {
                    interval = RollingInterval.Day;
                }

                loggerConfiguration.WriteTo.File(path, rollingInterval: interval);
            }

            if (elkOptions.Enabled)
            {
                loggerConfiguration.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elkOptions.Url))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                    IndexFormat = string.IsNullOrWhiteSpace(elkOptions.IndexFormat)
                        ? "logstash-{0:yyyy.MM.dd}"
                        : elkOptions.IndexFormat,
                    ModifyConnectionSettings = connectionConfiguration =>
                        elkOptions.BasicAuthEnabled
                            ? connectionConfiguration.BasicAuthentication(elkOptions.Username, elkOptions.Password)
                            : connectionConfiguration
                }).MinimumLevel.ControlledBy(LoggingLevelSwitch);
            }
            
        }

        internal static LogEventLevel GetLogEventLevel(string level)
            => Enum.TryParse<LogEventLevel>(level, true, out var logLevel) 
                ? logLevel
                : LogEventLevel.Information;

        public static IConderBuilder AddCorrelationContextLogging(this IConderBuilder builder)
        {
            builder.Services.AddTransient<CorrelationContextLoggingMiddleware>();
            
            return builder;
        }
        
        public static IApplicationBuilder UserCorrelationContextLogging(this IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelationContextLoggingMiddleware>();
            
            return app;
        }
    }
}
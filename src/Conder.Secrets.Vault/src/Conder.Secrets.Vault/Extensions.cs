using Conder.Secrets.Vault.Internals;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Conder.Secrets.Vault
{
    public static class Extensions
    {
        private const string SectionName = "vault";

        public static IHostBuilder UseVault(this IHostBuilder builder, string keyValuePath = null,
            string sectionName = SectionName)
            => builder.ConfigureServices(services => services.AddVault(sectionName))
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    var options = cfg.Build().GetOptions<VaultOptions>(sectionName);
                    if (options.Enabled)
                    {
                        cfg.AddVaultAsync(options, keyValuePath).GetAwaiter().GetResult();
                    }
                });

        public static IWebHostBuilder UseVault(this IWebHostBuilder builder, string keyValuePath = null,
            string sectionName = SectionName)
            => builder.ConfigureServices(services => services.AddVault(sectionName))
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    var options = cfg.Build().GetOptions<VaultOptions>(sectionName);
                    if (options.Enabled)
                    {
                        cfg.AddVaultAsync(options, keyValuePath).GetAwaiter().GetResult();
                    }
                });

        private static IServiceCollection AddVault(this IServiceCollection services, string sectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            IConfiguration configuration;

            using (var serviceProvider = services.BuildServiceProvider())
            {
                configuration = serviceProvider.GetService<IConfiguration>();
            }

            var options = configuration.GetOptions<VaultOptions>(sectionName);
            
            VerifyVaultOptions(options);

            services.AddSingleton(options);
            
            services.AddTransient<IKeyValueSecrets, KeyValueSecrets>();

            return services;
        }

        private static void VerifyVaultOptions(VaultOptions options)
        {
            if (options.Kv is null)
            {
                if (!string.IsNullOrWhiteSpace(options.Key))
                {
                    options.Kv = new VaultOptions.KeyValueOptions
                    {
                        Enabled = options.Enabled,
                        Path = options.Key
                    };
                }
                
                return;
            }

            if (options.Kv.EngineVersion > 2 || options.Kv.EngineVersion < 0)
            {
                throw new VaultException("Invalid KV engine version" +
                                         $" {options.Kv.EngineVersion} (available: 1 or 2)");
            }

            if (options.Kv.EngineVersion == 0)
            {
                options.Kv.EngineVersion = 2;
            }
        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Conder.Discovery.Consul.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Conder.Discovery.Consul.Services
{
    public class ConsulHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ConsulHostedService> _logger;
        private readonly IHostApplicationLifetime _appLifetime;

        public ConsulHostedService(IServiceScopeFactory serviceScopeFactory, ILogger<ConsulHostedService> logger,
            IHostApplicationLifetime appLifetime)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _appLifetime = appLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var consulService = scope.ServiceProvider.GetRequiredService<IConsulService>();
            var registration = scope.ServiceProvider.GetRequiredService<ServiceRegistration>();
            _logger.LogInformation($"Registering a service [id: {registration.Id}] in Consul...");

            var response = await consulService.RegisterServiceAsync(registration);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Registered a service [id: {registration.Id}] in Consul. ");
                
                _appLifetime.ApplicationStopping.Register(OnStopped);
                
                return;
            }

            _logger.LogError("There was an error when registering a service" +
                             $" [id: {registration.Id}] in Consul. {response}");
        }

        private void OnStopped()
        {
            _logger.LogInformation("Window will close automatically in 20 seconds.");
            Task.Delay(10000).GetAwaiter().GetResult();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var consulService = scope.ServiceProvider.GetRequiredService<IConsulService>();
            var registration = scope.ServiceProvider.GetRequiredService<ServiceRegistration>();
            _logger.LogInformation($"Unregistering a service [id: {registration.Id}] from Consul...");

            var response = await consulService.DeregisterServiceAsync(registration.Id);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Unregistered a service [id: {registration.Id}] from Consul.");

                return;
            }

            _logger.LogError(
                $"There was an error when unregistering a service [id: {registration.Id}]" +
                $" from Consul. {response}");
        }
    }
}
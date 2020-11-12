using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Conder.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Conder
{
    public sealed class ConderBuilder : IConderBuilder
    {
        private readonly ConcurrentDictionary<string, bool> _registry;
        private readonly List<Action<IServiceProvider>> _buildActions;
        private readonly IServiceCollection _services;

        IServiceCollection IConderBuilder.Services => _services;

        private ConderBuilder(IServiceCollection services)
        {
            _services = services;
            _services.AddSingleton<IStartupInitializer>(new StartupInitializer());

            _registry = new ConcurrentDictionary<string, bool>();
            _buildActions = new List<Action<IServiceProvider>>();
        }

        public static IConderBuilder Create(IServiceCollection services)
            => new ConderBuilder(services);

        public bool TryRegister(string name)
            => _registry.TryAdd(name, true);

        public void AddBuildAction(Action<IServiceProvider> execute)
            => _buildActions.Add(execute);

        public void AddInitializer(IInitializer initializer)
            => AddBuildAction(provider =>
            {
                var startupInitializer = provider.GetService<IStartupInitializer>();
                startupInitializer.AddInitializer(initializer);
            });

        public void AddInitializer<TInitializer>() where TInitializer : IInitializer
            => AddBuildAction(provider =>
            {
                var initializer = provider.GetService<TInitializer>();
                var startupInitializer = provider.GetService<IStartupInitializer>();
                startupInitializer.AddInitializer(initializer);
            });

        public IServiceProvider Build()
        {
            var serviceProvider = _services.BuildServiceProvider();
            _buildActions.ForEach(action => action(serviceProvider));
            return serviceProvider;
        }
    }
}
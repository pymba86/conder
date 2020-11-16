using System;
using Conder.CQRS.Events.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace Conder.CQRS.Events
{
    public static class Extensions
    {
        public static IConderBuilder AddEventHandlers(this IConderBuilder builder)
        {
            builder.Services.Scan(s =>
                s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            return builder;
        }
        
        public static IConderBuilder AddInMemoryEventDispatcher(this IConderBuilder builder)
        {
            builder.Services.AddSingleton<IEventDispatcher, EventDispatcher>();
            return builder;
        }
    }
}
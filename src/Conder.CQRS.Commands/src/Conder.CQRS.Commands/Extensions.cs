using System;
using Conder.CQRS.Commands.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace Conder.CQRS.Commands
{
    public static class Extensions
    {
        public static IConderBuilder AddCommandHandlers(this IConderBuilder builder)
        {
            builder.Services.Scan(s =>
                s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            return builder;
        }

        public static IConderBuilder AddInMemoryCommandDispatcher(this IConderBuilder builder)
        {
            builder.Services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

            return builder;
        }
    }
}
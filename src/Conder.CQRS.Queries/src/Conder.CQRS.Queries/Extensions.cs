using System;
using Conder.CQRS.Queries.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace Conder.CQRS.Queries
{
    public static class Extensions
    {
        public static IConderBuilder AddQueryHandlers(this IConderBuilder builder)
        {
            builder.Services.Scan(s =>
                s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime());

            return builder;
        }

        public static IConderBuilder AddInMemoryQueryDispatcher(this IConderBuilder builder)
        {
            builder.Services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
            return builder;
        }
    }
}
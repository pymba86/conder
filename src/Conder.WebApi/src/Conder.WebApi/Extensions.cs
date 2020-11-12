using System;
using System.Linq;
using Conder.WebApi.Exceptions;
using Conder.WebApi.Formatters;
using Conder.WebApi.Requests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Open.Serialization.Json;
using Open.Serialization.Json.Newtonsoft;

namespace Conder.WebApi
{
    public static class Extensions
    {
        private const string SectionName = "webApi";
        private const string RegistryName = "webApi";

        private static bool _bindRequestFromRoute;

        private static IConderBuilder AddWebApi(this IConderBuilder builder,
            Action<IMvcCoreBuilder> configureMvc = null,
            IJsonSerializer jsonSerializer = null, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            if (!builder.TryRegister(RegistryName))
            {
                return builder;
            }

            if (jsonSerializer is null)
            {
                var factory = new JsonSerializerFactory(
                    new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Converters = {new StringEnumConverter(true)}
                    });

                jsonSerializer = factory.GetSerializer();
            }

            if (jsonSerializer.GetType().Namespace?.Contains("Newtonsoft") == true)
            {
                builder.Services.Configure<KestrelServerOptions>(o => o.AllowSynchronousIO = true);
                builder.Services.Configure<IISServerOptions>(o => o.AllowSynchronousIO = true);
            }

            builder.Services.AddSingleton(jsonSerializer);
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddSingleton(new WebApiEndpointDefinitions());

            var options = builder.GetOptions<WebApiOptions>(sectionName);
            builder.Services.AddSingleton(options);

            _bindRequestFromRoute = options.BindRequestFromRoute;

            var mvcCoreBuilder = builder.Services
                .AddLogging()
                .AddMvcCore();

            mvcCoreBuilder.AddMvcOptions(o =>
                {
                    o.OutputFormatters.Clear();
                    o.OutputFormatters.Add(new JsonOutputFormatter(jsonSerializer));
                    o.InputFormatters.Clear();
                    o.InputFormatters.Add(new JsonInputFormatter(jsonSerializer));
                })
                .AddDataAnnotations()
                .AddApiExplorer()
                .AddAuthorization();

            configureMvc?.Invoke(mvcCoreBuilder);

            builder.Services.Scan(s =>
            {
                s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                    .AddClasses(c => c.AssignableTo(typeof(IRequestHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime();
            });

            builder.Services.AddTransient<IRequestDispatcher, RequestDispatcher>();

            if (builder.Services.All(s => s.ServiceType != typeof(IExceptionToResponseMapper)))
            {
                builder.Services.AddTransient<IExceptionToResponseMapper, EmptyExceptionToResponseMapper>();
            }

            return builder;
        }

        private class EmptyExceptionToResponseMapper : IExceptionToResponseMapper
        {
            public ExceptionResponse Map(Exception exception) => null;
        }
    }
}
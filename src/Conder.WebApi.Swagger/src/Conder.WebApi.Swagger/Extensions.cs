using System;
using Conder.Docs.Swagger;
using Conder.WebApi.Swagger.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Conder.WebApi.Swagger
{
    public static class Extensions
    {
        private const string SectionName = "swagger";

        public static IConderBuilder AddWebApiSwaggerDocs(this IConderBuilder builder,
            string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            return builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(sectionName));
        }
        
        public static IConderBuilder AddWebApiSwaggerDocs(this IConderBuilder builder, 
            Func<ISwaggerOptionsBuilder, ISwaggerOptionsBuilder> buildOptions)
            => builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(buildOptions));
        
        public static IConderBuilder AddWebApiSwaggerDocs(this IConderBuilder builder, SwaggerOptions options)
            => builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(options));
        
        private static IConderBuilder AddWebApiSwaggerDocs(this IConderBuilder builder,
            Action<IConderBuilder> registerSwagger)
        {
            registerSwagger(builder);
            builder.Services.AddSwaggerGen(c => c.DocumentFilter<WebApiDocumentFilter>());
            return builder;
        }
    }
}
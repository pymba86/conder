using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Conder.Auth
{
    public static class Extensions
    {
        private const string SectionName = "jwt";
        private const string RegistryName = "auth";

        public static IConderBuilder AddJwt(this IConderBuilder builder, string sectionName = SectionName,
            Action<JwtBearerOptions> optionsFactory = null)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                sectionName = SectionName;
            }

            var options = builder.GetOptions<JwtOptions>(sectionName);
            return builder.AddJwt(options, optionsFactory);
        }

        private static IConderBuilder AddJwt(this IConderBuilder builder, JwtOptions options,
            Action<JwtBearerOptions> optionsFactory = null)
        {
            return builder;
        }
    }
}
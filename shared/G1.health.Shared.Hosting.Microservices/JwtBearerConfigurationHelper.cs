using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Owl.TokenWildcardIssuerValidator;
using Volo.Abp.Modularity;

namespace G1.health.Shared.Hosting.Microservices;

public static class JwtBearerConfigurationHelper
{
    public static void Configure(
        ServiceConfigurationContext context,
        string audience)
    {
        var configuration = context.Services.GetConfiguration();

        context.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                options.Audience = audience;

                options.TokenValidationParameters.IssuerValidator = TokenWildcardIssuerValidator.IssuerValidator;
                options.TokenValidationParameters.ValidIssuers = new[]
                {
                    configuration["AuthServer:Authority"] + "/",
                  configuration["AuthServer:ValidIssuers"]
                };
            });
    }
}

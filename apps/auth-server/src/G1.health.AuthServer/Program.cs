using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using G1.health.Shared.Hosting.AspNetCore;
using Serilog;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Security.Claims;
using IdentityModel;

namespace G1.health.AuthServer;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        var assemblyName = typeof(Program).Assembly.GetName().Name;

        SerilogConfigurationHelper.Configure(assemblyName!);

        try
        {
            Log.Information($"Starting {assemblyName}.");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host
                .AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();
            if (!builder.Environment.IsDevelopment())
                builder.Configuration.SetBasePath(builder.Environment.ContentRootPath).AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true);
            await builder.AddApplicationAsync<healthAuthServerModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();

            AbpClaimTypes.UserName = JwtClaimTypes.PreferredUserName;
            AbpClaimTypes.Name = JwtClaimTypes.GivenName;
            AbpClaimTypes.SurName = JwtClaimTypes.FamilyName;
            AbpClaimTypes.UserId = JwtClaimTypes.Subject;
            AbpClaimTypes.Role = JwtClaimTypes.Role;
            AbpClaimTypes.Email = JwtClaimTypes.Email;

            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, $"{assemblyName} terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

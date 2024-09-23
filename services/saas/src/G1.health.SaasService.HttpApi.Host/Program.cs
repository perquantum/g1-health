using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using G1.health.Shared.Hosting.AspNetCore;
using Serilog;
using Microsoft.Extensions.Configuration;

namespace G1.health.SaasService;

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
            await builder.AddApplicationAsync<SaasServiceHttpApiHostModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex) when (ex is not HostAbortedException)
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

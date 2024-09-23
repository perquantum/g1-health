using Microsoft.Extensions.Configuration;
using Serilog;

namespace Microsoft.Extensions.Hosting;

public static class AbpHostingHostBuilderExtensions
{
    //public const string AppOcelotJsonPath = "ocelot.json";

    //public static IHostBuilder AddOcelotJson(
    //    this IHostBuilder hostBuilder,
    //    bool optional = true,
    //    bool reloadOnChange = true,
    //    string path =AppOcelotJsonPath)
    //{
    //    //Log.Information(AppOcelotJsonPath);
    //    return hostBuilder.ConfigureAppConfiguration((_, builder) =>
    //    {
    //        builder.AddJsonFile(
    //            path: path,
    //            optional: optional,
    //            reloadOnChange: reloadOnChange
    //        );
    //    });
    //}

    public const string AppYarpJsonPath = "yarp.json";
    public static IHostBuilder AddYarpJson(
        this IHostBuilder hostBuilder,
        bool optional = true,
        bool reloadOnChange = true,
        string path = AppYarpJsonPath)
    {
        //Log.Information(AppOcelotJsonPath);
        return hostBuilder.ConfigureAppConfiguration((_, builder) =>
        {
            builder.AddJsonFile(
                path: path,
                optional: optional,
                reloadOnChange: reloadOnChange
            );
        });
    }
}
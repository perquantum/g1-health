using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using G1.health.AdministrationService.EntityFrameworkCore;
using G1.health.SaasService.EntityFrameworkCore;
using G1.health.Shared.Hosting.AspNetCore;
using StackExchange.Redis;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.BackgroundJobs.RabbitMQ;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;
// using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace G1.health.Shared.Hosting.Microservices;

[DependsOn(
    // typeof(AbpMongoDbModule), // Un-comment if you are using mongodb in any microservice
    typeof(healthSharedHostingAspNetCoreModule),
    typeof(AbpBackgroundJobsRabbitMqModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpDistributedLockingModule),
    typeof(AbpEventBusRabbitMqModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(SaasServiceEntityFrameworkCoreModule),
    typeof(AdministrationServiceEntityFrameworkCoreModule)
)]
public class healthSharedHostingMicroservicesModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = true;
        });

        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "health:";
        });

        var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);

        context.Services
            .AddDataProtection()
            .SetApplicationName("health")
            .PersistKeysToStackExchangeRedis(redis, "health-Protection-Keys");

        context.Services.AddSingleton<IDistributedLockProvider>(_ =>
            new RedisDistributedSynchronizationProvider(redis.GetDatabase()));

        if (hostingEnvironment.IsDevelopment())
        {
            IdentityModelEventSource.ShowPII = true;
        }
    }
}
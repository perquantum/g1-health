using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using G1.health.IdentityService.EntityFrameworkCore;
using G1.health.Shared.Hosting.Microservices.DbMigrations.EfCore;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace G1.health.IdentityService.DbMigrations;

public class IdentityServiceDatabaseMigrationChecker : PendingEfCoreMigrationsChecker<IdentityServiceDbContext>
{
    private readonly IdentityServiceDataSeeder _dataSeeder;

    public IdentityServiceDatabaseMigrationChecker(
        ILoggerFactory loggerFactory,
        IUnitOfWorkManager unitOfWorkManager,
        IServiceProvider serviceProvider,
        ICurrentTenant currentTenant,
        IDistributedEventBus distributedEventBus,
        IAbpDistributedLock abpDistributedLock,
        IdentityServiceDataSeeder dataSeeder) : base(
        loggerFactory,
        unitOfWorkManager,
        serviceProvider,
        currentTenant,
        distributedEventBus,
        abpDistributedLock,
        IdentityServiceDbProperties.ConnectionStringName)
    {
        _dataSeeder = dataSeeder;
    }

    public override async Task CheckAndApplyDatabaseMigrationsAsync()
    {
        await base.CheckAndApplyDatabaseMigrationsAsync();
        
        await TryAsync(async () => await _dataSeeder.SeedAsync());
    }
}
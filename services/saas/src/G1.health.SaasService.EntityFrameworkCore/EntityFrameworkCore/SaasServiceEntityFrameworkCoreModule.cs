using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.MySQL;
using Volo.Abp.Modularity;
//using Volo.Payment.EntityFrameworkCore;
using Volo.Saas.EntityFrameworkCore;

namespace G1.health.SaasService.EntityFrameworkCore;

[DependsOn(
    typeof(SaasServiceDomainModule),
    typeof(SaasEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreMySQLModule)
    //typeof(AbpPaymentEntityFrameworkCoreModule)
)]
public class SaasServiceEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        SaasServiceEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<SaasServiceDbContext>(options =>
        {
            options.ReplaceDbContext<ISaasDbContext>();
            //options.ReplaceDbContext<IPaymentDbContext>();

                /* includeAllEntities: true allows to use IRepository<TEntity, TKey> also for non aggregate root entities */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<SaasServiceDbContext>(c =>
            {
                c.UseMySQL(b =>
                {
                    b.MigrationsHistoryTable("__SaasService_Migrations");
                });
            });
        });
    }
}

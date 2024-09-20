using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.MySQL;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
//using Volo.Chat.EntityFrameworkCore;

namespace G1.health.IdentityService.EntityFrameworkCore;

[DependsOn(
    typeof(IdentityServiceDomainModule),
    typeof(AbpIdentityProEntityFrameworkCoreModule),
    typeof(AbpOpenIddictProEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreMySQLModule)
)]
//[DependsOn(typeof(ChatEntityFrameworkCoreModule))]
    public class IdentityServiceEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        IdentityServiceEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<IdentityServiceDbContext>(options =>
        {
            options.ReplaceDbContext<IIdentityServiceDbContext>();
            options.ReplaceDbContext<IOpenIddictDbContext>();
            options.ReplaceDbContext<IIdentityDbContext, IIdentityServiceDbContext>();
            //options.ReplaceDbContext<IChatDbContext>();

            /* includeAllEntities: true allows to use IRepository<TEntity, TKey> also for non aggregate root entities */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        context.Services.AddDefaultRepository(
            typeof(Volo.Abp.Identity.IdentityUser),
            typeof(Users.EfCoreIdentityUserRepository),
            replaceExisting: true
            );

        context.Services.AddDefaultRepository(
            typeof(Volo.Abp.Identity.IdentityRole),
            typeof(Roles.EfCoreIdentityRoleRepository),
            replaceExisting: true
            );

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<IdentityServiceDbContext>(c =>
            {
                c.UseMySQL(b =>
                {
                    b.MigrationsHistoryTable("__IdentityService_Migrations");
                });
            });
        });
    }
}

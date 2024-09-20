using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.MySQL;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Gdpr;
using Volo.Abp.LanguageManagement.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TextTemplateManagement.EntityFrameworkCore;
using Volo.FileManagement.EntityFrameworkCore;

namespace G1.health.AdministrationService.EntityFrameworkCore;

[DependsOn(
    typeof(AdministrationServiceDomainModule),
    typeof(AbpEntityFrameworkCoreMySQLModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(BlobStoringDatabaseEntityFrameworkCoreModule),
    typeof(LanguageManagementEntityFrameworkCoreModule),
    typeof(TextTemplateManagementEntityFrameworkCoreModule),
    typeof(AbpGdprEntityFrameworkCoreModule)
)]
    [DependsOn(typeof(FileManagementEntityFrameworkCoreModule))]
    public class AdministrationServiceEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        AdministrationServiceEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<AdministrationServiceDbContext>(options =>
        {
            options.ReplaceDbContext<IPermissionManagementDbContext>();
            options.ReplaceDbContext<ISettingManagementDbContext>();
            options.ReplaceDbContext<IFeatureManagementDbContext>();
            options.ReplaceDbContext<IAuditLoggingDbContext>();
            options.ReplaceDbContext<ILanguageManagementDbContext>();
            options.ReplaceDbContext<ITextTemplateManagementDbContext>();
            options.ReplaceDbContext<IGdprDbContext>();
            options.ReplaceDbContext<IBlobStoringDbContext>();
            options.ReplaceDbContext<IFileManagementDbContext>();

                /* includeAllEntities: true allows to use IRepository<TEntity, TKey> also for non aggregate root entities */
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<AdministrationServiceDbContext>(c =>
            {
                c.UseMySQL(b =>
                {
                    b.MigrationsHistoryTable("__AdministrationService_Migrations");
                   
                });
            });
        });
    }
}

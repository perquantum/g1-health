using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AuditLogging;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Gdpr;
using Volo.Abp.LanguageManagement;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TextTemplateManagement;
using Volo.FileManagement;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.BlobStoring;

namespace G1.health.AdministrationService;

[DependsOn(
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpAuditLoggingApplicationModule),
    typeof(LanguageManagementApplicationModule),
    typeof(TextTemplateManagementApplicationModule),
    typeof(AbpGdprApplicationModule),
    typeof(AdministrationServiceApplicationContractsModule),
    typeof(AdministrationServiceDomainModule)
)]
[DependsOn(typeof(FileManagementApplicationModule))]
[DependsOn(typeof(AbpBlobStoringFileSystemModule))]
public class AdministrationServiceApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<AdministrationServiceApplicationModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AdministrationServiceApplicationModule>(validate: true);
        });

        var Configuration = context.Services.GetConfiguration();

        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.Configure<FileManagementContainer>(c =>
            {
                c.UseFileSystem(fileSystem =>
                {
                    fileSystem.BasePath = Configuration.GetSection("BlobConfig:BasePath").Value;
                }); // You can use FileSystem or Azure providers also.
            });
        });

    }
}

using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AuditLogging;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Gdpr;
using Volo.Abp.LanguageManagement;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TextTemplateManagement;
using Volo.FileManagement;

namespace G1.health.AdministrationService;

[DependsOn(
    typeof(AdministrationServiceApplicationContractsModule),
    typeof(AbpPermissionManagementHttpApiClientModule),
    typeof(AbpFeatureManagementHttpApiClientModule),
    typeof(AbpSettingManagementHttpApiClientModule),
    typeof(AbpAuditLoggingHttpApiClientModule),
    typeof(LanguageManagementHttpApiClientModule),
    typeof(TextTemplateManagementHttpApiClientModule),
    typeof(AbpGdprHttpApiClientModule)
)]
    [DependsOn(typeof(FileManagementHttpApiClientModule))]
    public class AdministrationServiceHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(AdministrationServiceApplicationContractsModule).Assembly,
            AdministrationServiceRemoteServiceConsts.RemoteServiceName
        );
    }
}

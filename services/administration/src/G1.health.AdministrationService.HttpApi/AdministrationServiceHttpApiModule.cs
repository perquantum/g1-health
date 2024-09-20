using Volo.Abp.AuditLogging;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Gdpr;
using Volo.Abp.LanguageManagement;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.SettingManagement;
using Volo.Abp.TextTemplateManagement;
using Volo.FileManagement;


namespace G1.health.AdministrationService;

[DependsOn(
    typeof(AdministrationServiceApplicationContractsModule),
    typeof(AbpPermissionManagementHttpApiModule),
    typeof(AbpFeatureManagementHttpApiModule),
    typeof(AbpSettingManagementHttpApiModule),
    typeof(AbpAuditLoggingHttpApiModule),
    typeof(LanguageManagementHttpApiModule),
    typeof(TextTemplateManagementHttpApiModule),
    typeof(AbpGdprHttpApiModule)
)]
    [DependsOn(typeof(FileManagementHttpApiModule))]
    public class AdministrationServiceHttpApiModule : AbpModule
{
}

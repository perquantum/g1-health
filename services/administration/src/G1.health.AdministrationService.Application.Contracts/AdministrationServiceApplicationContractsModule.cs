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
    typeof(AbpPermissionManagementApplicationContractsModule),
    typeof(AbpFeatureManagementApplicationContractsModule),
    typeof(AbpSettingManagementApplicationContractsModule),
    typeof(AbpAuditLoggingApplicationContractsModule),
    typeof(LanguageManagementApplicationContractsModule),
    typeof(TextTemplateManagementApplicationContractsModule),
    typeof(AbpGdprApplicationContractsModule),
    typeof(AdministrationServiceDomainSharedModule)
)]
    [DependsOn(typeof(FileManagementApplicationContractsModule))]
    public class AdministrationServiceApplicationContractsModule : AbpModule
{
}

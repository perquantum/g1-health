using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account.Admin.Web;
using Volo.Abp.Account.Pro.Public.Web.Shared;
using Volo.Abp.AuditLogging.Web;
using Volo.Abp.AutoMapper;
using Volo.Abp.Gdpr.Web;
using Volo.Abp.LanguageManagement;
using Volo.Abp.Modularity;
using Volo.Abp.TextTemplateManagement.Web;
using Volo.Abp.VirtualFileSystem;
using Volo.FileManagement.Web;

namespace G1.health.AdministrationService.Web;

[DependsOn(
    typeof(AbpAuditLoggingWebModule),
    typeof(LanguageManagementWebModule),
    typeof(TextTemplateManagementWebModule),
    typeof(AbpGdprWebModule),
    typeof(AbpAccountAdminWebModule),
    typeof(AbpAccountPublicWebSharedModule),
    typeof(AdministrationServiceApplicationContractsModule)
    )]
    [DependsOn(typeof(FileManagementWebModule))]
    public class AdministrationServiceWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<AdministrationServiceWebModule>();
        });
        
        context.Services.AddAutoMapperObjectMapper<AdministrationServiceWebModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AdministrationServiceWebModule>(validate: true);
        });
    }
}

using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using Volo.Saas.Host;
using Volo.Saas.Tenant;
using Volo.Payment.Admin.Web;

namespace G1.health.SaasService.Web;

[DependsOn(
    typeof(SaasServiceApplicationContractsModule),
    typeof(SaasHostWebModule),
    typeof(SaasTenantWebModule),
    typeof(AbpPaymentAdminWebModule)
)]
public class SaasServiceWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SaasServiceWebModule>();
        });
        
        context.Services.AddAutoMapperObjectMapper<SaasServiceWebModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<SaasServiceWebModule>(validate: true);
        });
        context.Services.Configure<AbpSaasHostWebOptions>(options =>
        {
            options.EnableTenantImpersonation = true;
        });
    }
}

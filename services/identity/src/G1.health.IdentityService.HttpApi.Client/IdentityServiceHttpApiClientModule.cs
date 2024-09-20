using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Modularity;
using Volo.Chat;

namespace G1.health.IdentityService;

[DependsOn(
    typeof(IdentityServiceApplicationContractsModule),
    typeof(AbpOpenIddictProHttpApiClientModule),
    typeof(AbpIdentityHttpApiClientModule),
    typeof(AbpAccountAdminHttpApiClientModule)
    )]
//[DependsOn(typeof(ChatHttpApiClientModule))]
    public class IdentityServiceHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(IdentityServiceApplicationContractsModule).Assembly,
            IdentityServiceRemoteServiceConsts.RemoteServiceName
        );
    }
}

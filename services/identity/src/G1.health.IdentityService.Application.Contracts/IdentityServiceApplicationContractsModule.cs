using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Chat;

namespace G1.health.IdentityService;

[DependsOn(
    typeof(AbpIdentityApplicationContractsModule),
    typeof(AbpOpenIddictProApplicationContractsModule),
    typeof(AbpAccountAdminApplicationContractsModule),
    typeof(IdentityServiceDomainSharedModule)
)]
[DependsOn(typeof(ChatApplicationContractsModule))]
    public class IdentityServiceApplicationContractsModule : AbpModule
{
}

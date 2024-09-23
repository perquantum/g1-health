using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;
using G1.health.IdentityService.Users;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;
//using Volo.Chat;

namespace G1.health.IdentityService;

[DependsOn(
    typeof(AbpIdentityProDomainModule),
    typeof(AbpOpenIddictProDomainModule),
    typeof(IdentityServiceDomainSharedModule)
)]
//[DependsOn(typeof(ChatDomainModule))]
    public class IdentityServiceDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Scoped<UserManager<IdentityUser>, MyIdentityUserManager>());
        context.Services.Replace(ServiceDescriptor.Scoped<IdentityUserManager, MyIdentityUserManager>());
        context.Services.PostConfigure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.DynamicContributors.RemoveAll(x => x == typeof(Volo.Abp.Identity.IdentityDynamicClaimsPrincipalContributor));
        });
    }
}

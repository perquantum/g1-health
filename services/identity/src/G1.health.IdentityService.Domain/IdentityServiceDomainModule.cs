using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;
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
        //context.Services.TryAddScoped();
        //context.Services.TryAddScoped(typeof(UserManager<IdentityUser>), provider => provider.GetService(typeof(Users.IdentityUserManager)));
        context.Services.TryAddScoped(typeof(IdentityUserManager), provider => provider.GetService(typeof(Users.IdentityUserManager)));
        context.Services.PostConfigure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.DynamicContributors.AddIfNotContains(typeof(IdentityDynamicClaimsPrincipalContributor));
        });
    }
}

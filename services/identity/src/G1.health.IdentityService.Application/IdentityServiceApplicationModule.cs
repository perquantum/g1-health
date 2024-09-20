using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Chat;

namespace G1.health.IdentityService;

[DependsOn(
    typeof(AbpIdentityApplicationModule),
    typeof(AbpOpenIddictProApplicationModule),
    typeof(IdentityServiceDomainModule),
    typeof(AbpAccountAdminApplicationModule),
    typeof(IdentityServiceApplicationContractsModule)
)]
//[DependsOn(typeof(ChatApplicationModule))]
    public class IdentityServiceApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<IdentityServiceApplicationModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<IdentityServiceApplicationModule>(validate: true);
        });
        context.Services.AddAuthorization()
        .AddDynamicExternalLoginProviderOptions<FacebookOptions>(
            FacebookDefaults.AuthenticationScheme,
            options =>
            {
                options.WithProperty(x => x.AppId);
                options.WithProperty(x => x.AppSecret, isSecret: true);
            }
        );
    }
}

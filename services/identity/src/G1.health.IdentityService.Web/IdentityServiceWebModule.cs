using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity.Web;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.Pro.Web;
using Volo.Abp.VirtualFileSystem;
using Volo.Chat;
//using Volo.Chat.Web;
using Volo.Abp.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authentication.Facebook;

namespace G1.health.IdentityService.Web;

[DependsOn(
    typeof(AbpIdentityWebModule),
    typeof(AbpOpenIddictProWebModule),
    typeof(IdentityServiceApplicationContractsModule)
)]
//[DependsOn(typeof(ChatSignalRModule))]
    //[DependsOn(typeof(ChatWebModule))]
    //[DependsOn(typeof(AbpAspNetCoreSignalRModule))]
    public class IdentityServiceWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<IdentityServiceWebModule>();
        });

        context.Services.AddAutoMapperObjectMapper<IdentityServiceWebModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<IdentityServiceWebModule>(validate: true);
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
        context.Services.Configure<AbpIdentityWebOptions>(options =>
        {
            options.EnableUserImpersonation = true;
        });
    }
}

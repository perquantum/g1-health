using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using G1.health.AdministrationService.EntityFrameworkCore;
using G1.health.IdentityService;
using G1.health.IdentityService.EntityFrameworkCore;
using G1.health.IdentityService.Users;
using G1.health.SaasService.EntityFrameworkCore;
using G1.health.Shared.Hosting.AspNetCore;
using OpenIddict.Server.AspNetCore;
using Prometheus;
using StackExchange.Redis;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Public.Web.ExternalProviders;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonX;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonX.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.Auditing;
using Volo.Abp.BackgroundJobs.RabbitMQ;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Emailing;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.LeptonX.Shared;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.OpenIddict;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using Microsoft.AspNetCore.Authentication.Facebook;
using Volo.Abp.Sms.Twilio;
using Volo.Abp.Sms;
using Volo.Abp.Http.Client.IdentityModel;
using Volo.Abp.OpenIddict.WildcardDomains;
using Volo.Abp.Account.Public.Web.Impersonation;
using Volo.Abp.Account.Public.Web;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Volo.Abp.Security.Claims;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace G1.health.AuthServer;

[DependsOn(
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpEventBusRabbitMqModule),
    typeof(AbpBackgroundJobsRabbitMqModule),
    typeof(AbpAspNetCoreMvcUiLeptonXThemeModule),
    typeof(AbpAccountPublicWebOpenIddictModule),
    typeof(AbpAccountPublicApplicationModule),
    typeof(AbpAccountPublicHttpApiModule),
    typeof(AdministrationServiceEntityFrameworkCoreModule),
    typeof(IdentityServiceEntityFrameworkCoreModule),
    typeof(SaasServiceEntityFrameworkCoreModule),
    typeof(healthSharedHostingAspNetCoreModule),
    typeof(healthSharedLocalizationModule),
    typeof(AbpTwilioSmsModule),
    typeof(AbpSmsModule),
    typeof(AbpHttpClientIdentityModelModule),
    typeof(AbpAccountPublicWebImpersonationModule)

)]
public class healthAuthServerModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("AccountService");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        Configure<AbpTwilioSmsOptions>(options =>
        {
            options.AccountSId = configuration.GetSection("AbpTwilioSms:AccountSid").Value;
            options.AuthToken = configuration.GetSection("AbpTwilioSms:AuthToken").Value;
            options.FromNumber = configuration.GetSection("AbpTwilioSms:FromNumber").Value;
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(builder =>
            {
                builder.AddSigningCertificate(GetSigningCertificate(hostingEnvironment, configuration));
                builder.AddEncryptionCertificate(GetSigningCertificate(hostingEnvironment, configuration));
                builder.SetIssuer(new Uri(configuration["AuthServer:Authority"]));
            });
        }

        PreConfigure<AbpOpenIddictWildcardDomainOptions>(options =>
        {
            options.EnableWildcardDomainSupport = true;
            options.WildcardDomainsFormat.Add(configuration["AuthServer:WildcardDomainsFormat"]);
        });
        context.Services.Configure<AbpAccountOptions>(options =>
        {
            //For impersonation in Saas module
            options.TenantAdminUserName = "admin";
            //options.ImpersonationTenantPermission = SaasHostPermissions.Tenants.Impersonation;

            //For impersonation in Identity module
            options.ImpersonationUserPermission = IdentityPermissions.Users.Impersonation;
        });

        PreConfigure<IdentityBuilder>(builder =>
        {
            builder.AddClaimsPrincipalFactory<MyUserClaimsPrincipalFactory>();
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        if (!Convert.ToBoolean(configuration["App:DisablePII"]))
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
        }

        if (!Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });
        }

        ConfigureBundles();
        ConfigureSwagger(context, configuration);
        ConfigureSameSiteCookiePolicy(context);
        ConfigureExternalProviders(context);

        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = true;
        });

        Configure<AbpAuditingOptions>(options =>
        {
            options.ApplicationName = "AuthServer";
        });

        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.Applications["Angular"].RootUrl = configuration["App:SelfUrl"];
            options.Applications["Angular"].Urls[AccountUrlNames.EmailConfirmation] = "Account/EmailConfirmation";
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());
        });

        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "health:";
        });

        Configure<AbpTenantResolveOptions>(options =>
        {
            options.TenantResolvers.Clear();
            options.TenantResolvers.Add(new TenantDomainResolver(configuration["AuthServer:TenantResolver"], configuration["Environment"]));

            //options.AddDomainTenantResolver(configuration["AuthServer:TenantResolver"]);
        });

        ConfigValues configValues = new ConfigValues()
        {
            AngularAppUrl = configuration.GetSection("AngularApp:ReturnUrl").Value
        };
        context.Services.AddSingleton(configValues);

        var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName("health");
        var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
        dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "health-Protection-Keys");

        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.Trim().RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        context.Services.AddDistributedMemoryCache();

        context.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(3600);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

#if DEBUG
        context.Services.Replace(ServiceDescriptor.Singleton<IEmailSender, NullEmailSender>());
#endif

        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<healthSharedLocalizationModule>(Path.Combine(
                    hostingEnvironment.ContentRootPath,
                    $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}shared{Path.DirectorySeparatorChar}G1.health.Shared.Localization"));
            });
        }

        Configure<LeptonXThemeOptions>(options =>
        {
            options.DefaultStyle = LeptonXStyleNames.System;
        });
        Configure<AbpTwilioSmsOptions>(options =>
        {
            options.AccountSId = configuration.GetSection("AbpTwilioSms:AccountSid").Value;
            options.AuthToken = configuration.GetSection("AbpTwilioSms:AuthToken").Value;
            options.FromNumber = configuration.GetSection("AbpTwilioSms:FromNumber").Value;
        });

        context.Services.Replace(ServiceDescriptor.Scoped<UserManager<IdentityUser>, MyIdentityUserManager>());
        context.Services.Replace(ServiceDescriptor.Scoped<IdentityUserManager, MyIdentityUserManager>());

        context.Services.PostConfigure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.DynamicContributors.Clear();
            options.DynamicContributors.RemoveAll(x => x == typeof(Volo.Abp.Identity.IdentityDynamicClaimsPrincipalContributor));
        });
    }

    public override void OnPreApplicationInitialization(ApplicationInitializationContext context)
    {
        var identityUserManager = context.ServiceProvider.GetRequiredService<IdentityUserManager>();
        var identityUserManager2 = context.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var userClaimsPrincipalFactory2 = context.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<IdentityUser>>();
        var options = context.ServiceProvider.GetRequiredService<IOptions<AbpClaimsPrincipalFactoryOptions>>().Value;
        base.OnPreApplicationInitialization(context);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        var configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();

        app.Use(async (ctx, next) =>
        {
            if (ctx.Request.Headers.ContainsKey("from-ingress"))
            {
                ctx.Request.Scheme = "https";
            }

            await next();
        });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        app.UseCorrelationId();
        app.UseAbpSecurityHeaders();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();
        app.UseCookiePolicy();
        app.UseHttpMetrics();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();
        app.UseMultiTenancy();
        app.UseAbpSerilogEnrichers();
        app.UseUnitOfWork();
        app.UseAuthorization();
        app.UseSession();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Account Service API");
            options.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
        });
        app.UseAuditing();
        app.UseConfiguredEndpoints(endpoints =>
        {
            endpoints.MapMetrics();
        });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );
        });
    }

    private void ConfigureExternalProviders(ServiceConfigurationContext context)
    {
        context.Services.AddAuthentication()
            .AddGoogle(GoogleDefaults.AuthenticationScheme, _ => { })
            .WithDynamicOptions<GoogleOptions, GoogleHandler>(
                GoogleDefaults.AuthenticationScheme,
                options =>
                {
                    options.WithProperty(x => x.ClientId);
                    options.WithProperty(x => x.ClientSecret, isSecret: true);
                }
            )
            .AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, options =>
            {
                //Personal Microsoft accounts as an example.
                options.AuthorizationEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
                options.TokenEndpoint = "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
            })
            .WithDynamicOptions<MicrosoftAccountOptions, MicrosoftAccountHandler>(
                MicrosoftAccountDefaults.AuthenticationScheme,
                options =>
                {
                    options.WithProperty(x => x.ClientId);
                    options.WithProperty(x => x.ClientSecret, isSecret: true);
                }
            )
            .AddFacebook(FacebookDefaults.AuthenticationScheme, _ => { })
            .WithDynamicOptions<FacebookOptions, FacebookHandler>(
                FacebookDefaults.AuthenticationScheme,
                options =>
                {
                    options.WithProperty(x => x.AppId);
                    options.WithProperty(x => x.AppSecret, isSecret: true);
                }
            )
            .AddTwitter(TwitterDefaults.AuthenticationScheme, options => options.RetrieveUserDetails = true)
            .WithDynamicOptions<TwitterOptions, TwitterHandler>(
                TwitterDefaults.AuthenticationScheme,
                options =>
                {
                    options.WithProperty(x => x.ConsumerKey);
                    options.WithProperty(x => x.ConsumerSecret, isSecret: true);
                }
            );
    }

    private X509Certificate2 GetSigningCertificate(IWebHostEnvironment hostingEnv, IConfiguration configuration)
    {
        var fileName = "authserver.pfx";
        var passPhrase = "2D7AA457-5D33-48D6-936F-C48E5EF468ED";
        var file = Path.Combine(hostingEnv.ContentRootPath, fileName);

        if (!File.Exists(file))
        {
            throw new FileNotFoundException($"Signing Certificate couldn't found: {file}");
        }

        return new X509Certificate2(file, passPhrase);
    }

    private void ConfigureSwagger(ServiceConfigurationContext context, IConfiguration configuration)
    {
        SwaggerConfigurationHelper.ConfigureWithAuth(
            context: context,
            authority: configuration["AuthServer:Authority"]!,
            scopes: new Dictionary<string, string> {
                /* Requested scopes for authorization code request and descriptions for swagger UI only */
                { "AccountService", "Account Service API" }
            },
            apiTitle: "Account Service API"
        );
    }

    private void ConfigureSameSiteCookiePolicy(ServiceConfigurationContext context)
    {
        context.Services.AddSameSiteCookiePolicy();
    }
}

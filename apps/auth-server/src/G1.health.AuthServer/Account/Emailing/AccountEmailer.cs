using System;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using G1.health.Shared.Hosting.Microservices.DbMigrations.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.Account.Emailing;
using Volo.Abp.Account.Emailing.Templates;
using Volo.Abp.Account.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TextTemplating;
using Volo.Abp.UI.Navigation.Urls;

namespace G1.health.AuthServer.Account.Emailing;

public class AccountEmailer : IAccountEmailer, ITransientDependency
{
    protected ITemplateRenderer TemplateRenderer { get; }
    protected IEmailSender EmailSender { get; }
    protected IStringLocalizer<AccountResource> StringLocalizer { get; }
    protected IAppUrlProvider AppUrlProvider { get; }
    protected ICurrentTenant CurrentTenant { get; }
    private IConfiguration Configuration { get; }
    public ILogger<AccountEmailer> Logger { get; set; }
    protected readonly IDistributedEventBus DistributedEventBus;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AccountEmailer(
        IEmailSender emailSender,
        ITemplateRenderer templateRenderer,
        IStringLocalizer<AccountResource> stringLocalizer,
        IAppUrlProvider appUrlProvider,
        ICurrentTenant currentTenant,
        IDistributedEventBus distributedEventBus,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        EmailSender = emailSender;
        StringLocalizer = stringLocalizer;
        AppUrlProvider = appUrlProvider;
        CurrentTenant = currentTenant;
        TemplateRenderer = templateRenderer;
        Configuration = configuration;
        DistributedEventBus = distributedEventBus;
        Logger = NullLogger<AccountEmailer>.Instance;
        _httpContextAccessor = httpContextAccessor;
    }

    public virtual async Task SendPasswordResetLinkAsync(
        IdentityUser user,
        string resetToken,
        string appName,
        string returnUrl = null,
        string returnUrlHash = null)
    {
        Debug.Assert(CurrentTenant.Id == user.TenantId, "This method can only work for current tenant!");
        var request =  _httpContextAccessor.HttpContext.Request;
        var scheme = request.Scheme;
        var host = request.Host.Value;
        var path = "/Account/ResetPassword";

        // Construct the full URL
        var Url = $"{scheme}://{host}{path}";
        //var url = await AppUrlProvider.GetResetPasswordUrlAsync(appName);
        var env = Configuration.GetSection("Environment").Value;
        //TODO: Use AbpAspNetCoreMultiTenancyOptions to get the key
        var link = $"{Url}?userId={user.Id}&{TenantResolverConsts.DefaultTenantKey}={user.TenantId}&resetToken={UrlEncoder.Default.Encode(resetToken)}";

        if (!returnUrl.IsNullOrEmpty())
        {
            link += "&returnUrl=" + NormalizeReturnUrl(returnUrl);
        }

        if (!returnUrlHash.IsNullOrEmpty())
        {
            link += "&returnUrlHash=" + returnUrlHash;
        }
        
        var emailContent = await TemplateRenderer.RenderAsync(
            AccountEmailTemplates.PasswordResetLink,
            new { link = link }
        );
        
        emailContent = string.Format(emailContent, CurrentTenant.Name+env);
        Logger.LogInformation("EmailContent with tenantName" + emailContent);
        string urlPattern = @"href=""([^""]+)""";
        string ResetMyPasswordUrl = "";
        Match match = Regex.Match(emailContent, urlPattern);
        if (match.Success)
        {
            ResetMyPasswordUrl = match.Groups[1].Value;
            Logger.LogInformation("ResetMyPasswordUrl - " + ResetMyPasswordUrl);
        }
        Logger.LogInformation("ResetMyPasswordUrl - " + ResetMyPasswordUrl);
        await DistributedEventBus.PublishAsync(new ResetPasswordDetailsEto
        {
            EmailId = user.Email,
            PasswordReset = StringLocalizer["PasswordReset"],
            PasswordResetInfoInEmail = StringLocalizer["PasswordResetInfoInEmail"],
            ResetMyPassword = StringLocalizer["ResetMyPassword"],
            ResetMyPasswordUrl = ResetMyPasswordUrl,
            TenantId = (Guid)CurrentTenant.Id,
            TenantName = CurrentTenant.Name,
        }) ;

            //await EmailSender.SendAsync(
            //    user.Email,
            //    StringLocalizer["PasswordReset"],
            //    emailContent
            //);

    }

    public virtual async Task SendEmailConfirmationLinkAsync(
        IdentityUser user,
        string confirmationToken,
        string appName,
        string returnUrl = null,
        string returnUrlHash = null)
    {
        Debug.Assert(CurrentTenant.Id == user.TenantId, "This method can only work for current tenant!");

        var url = await AppUrlProvider.GetEmailConfirmationUrlAsync(appName);
        var env = Configuration.GetSection("Environment").Value;
        //TODO: Use AbpAspNetCoreMultiTenancyOptions to get the key
        var link = $"{url}?userId={user.Id}&{TenantResolverConsts.DefaultTenantKey}={user.TenantId}&confirmationToken={UrlEncoder.Default.Encode(confirmationToken)}";

        if (!returnUrl.IsNullOrEmpty())
        {
            link += "&returnUrl=" + NormalizeReturnUrl(returnUrl);
        }

        if (!returnUrlHash.IsNullOrEmpty())
        {
            link += "&returnUrlHash=" + returnUrlHash;
        }
        var httpContext = _httpContextAccessor.HttpContext;
        var request = httpContext.Request;
        var originalUrl = request.GetDisplayUrl();
        var newUrl = originalUrl.Replace("register", "EmailConfirmation", StringComparison.OrdinalIgnoreCase);
        link = $"{newUrl}?userId={user.Id}&{TenantResolverConsts.DefaultTenantKey}={user.TenantId}&confirmationToken={UrlEncoder.Default.Encode(confirmationToken)}";
        var emailContent = await TemplateRenderer.RenderAsync(
            AccountEmailTemplates.EmailConfirmationLink,
            new { link = link }
        );
        //emailContent = string.Format(emailContent, CurrentTenant.Name + env);
        await EmailSender.SendAsync(
            user.Email,
            StringLocalizer["EmailConfirmation"],
            emailContent
        );
    }

    public async Task SendEmailSecurityCodeAsync(IdentityUser user, string code)
    {
        var emailContent = await TemplateRenderer.RenderAsync(
            AccountEmailTemplates.EmailSecurityCode,
            new { code = code }
        );

        await EmailSender.SendAsync(
            user.Email,
            StringLocalizer["EmailSecurityCodeSubject"],
            emailContent
        );
    }

    private string NormalizeReturnUrl(string returnUrl)
    {
        if (returnUrl.IsNullOrEmpty())
        {
            return returnUrl;
        }

        //Handling openid connect login
        if (returnUrl.StartsWith("/connect/authorize/callback", StringComparison.OrdinalIgnoreCase))
        {
            if (returnUrl.Contains("?"))
            {
                var queryPart = returnUrl.Split('?')[1];
                var queryParameters = queryPart.Split('&');
                foreach (var queryParameter in queryParameters)
                {
                    if (queryParameter.Contains("="))
                    {
                        var queryParam = queryParameter.Split('=');
                        if (queryParam[0] == "redirect_uri")
                        {
                            return HttpUtility.UrlDecode(queryParam[1]);
                        }
                    }
                }
            }
        }

        if (returnUrl.StartsWith("/connect/authorize?", StringComparison.OrdinalIgnoreCase))
        {
            return HttpUtility.UrlEncode(returnUrl);
        }

        return returnUrl;
    }
}

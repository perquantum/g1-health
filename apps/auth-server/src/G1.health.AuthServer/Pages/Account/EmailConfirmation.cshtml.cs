using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using G1.health.AuthServer.Web.Pages.Account;
using G1.health.ClinicService.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Volo.Abp.AspNetCore.Mvc.MultiTenancy;
using Volo.Abp.Identity;
using Volo.Abp.Validation;
using static Org.BouncyCastle.Math.EC.ECCurve;
using IAccountAppService = Volo.Abp.Account.IAccountAppService;

namespace G1.health.AuthServer.Pages.Account;

public class EmailConfirmationModel : AccountPageModel
{
    private readonly IAccountAppService _accountAppService;
    private readonly IConfiguration _config;
    public IAbpTenantAppService AbpTenantAppService { get; set; }

    public EmailConfirmationModel(IAccountAppService accountAppService, IConfiguration configuration, IAbpTenantAppService abpTenantAppService)
    {
        _accountAppService = accountAppService;
        _config = configuration;
        AbpTenantAppService = abpTenantAppService;
    }

    [Required]
    [BindProperty(SupportsGet = true)]
    public Guid UserId { get; set; }

    [Required]
    [BindProperty(SupportsGet = true)]
    public string ConfirmationToken { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrlHash { get; set; }

    public bool EmailConfirmed { get; set; }

    public bool InvalidToken { get; set; }
    public string EmailVerificationText {  get; set; }
    public string YourEmailAddressIsSuccessfullyConfirmed { get; set; }
    public readonly IStringLocalizer<ClinicServiceResource> ClinicServiceLocalizer;
    public virtual async Task<IActionResult> OnGetAsync()
    {
        ReturnUrl = await GetRedirectUrlAsync(ReturnUrl, ReturnUrlHash);
        await SetCurrentTenant();
        await SetLocalizationTexts();
        Guid? tenantId = await GetCurrentTenantId();

        var user = await UserManager.GetByIdAsync(UserId);
            if (user.EmailConfirmed)
            {
                EmailConfirmed = true;
                return Page();
            }

            try
            {
                ValidateModel();
            InvalidToken = !await AccountAppService.VerifyEmailConfirmationTokenAsync(
                new Volo.Abp.Account.VerifyEmailConfirmationTokenInput()
                {
                    UserId = UserId,
                    Token = ConfirmationToken
                }
            );

            if (!InvalidToken)
            {
                await _accountAppService.ConfirmEmailAsync(new Volo.Abp.Account.ConfirmEmailInput
                {
                    UserId = UserId,
                    Token = ConfirmationToken
                });

                EmailConfirmed = true;
            }
        }
        catch (Exception e)
        {
            if (e is AbpIdentityResultException && !string.IsNullOrWhiteSpace(e.Message))
            {
                Alerts.Warning(GetLocalizeExceptionMessage(e));
                return Page();
            }

            if (e is AbpValidationException)
            {
                return Page();
            }

            throw;
        }
        

        return Page();
    }

    public virtual Task<IActionResult> OnPostAsync()
    {
        return Task.FromResult<IActionResult>(Page());
    }

    public async Task<Guid?> GetCurrentTenantId()
    {
        var host = Request.Host.Host;
        
        var tenant = new FindTenantResultDto();
        try
        {
            string env = _config.GetSection("Environment").Value;
            string subdomain = host.Split('.')[0];
            if (subdomain != env)
            {
                if (env.IsNullOrEmpty() || subdomain.EndsWith(env))
                {
                    int lastIndex = subdomain.LastIndexOf(env);
                    string tenantName = subdomain.Substring(0, lastIndex);
                    tenant = await AbpTenantAppService.FindTenantByNameAsync(tenantName);
                }
            }
        }
        catch(Exception ex)
        {
            throw;
        }
        return tenant.TenantId;
    }

    public async Task SetLocalizationTexts()
    {
        Guid? tenantId = await GetCurrentTenantId();

        using (CurrentTenant.Change(tenantId))
        {
            EmailVerificationText = L["EmailVerification"].Value;
            YourEmailAddressIsSuccessfullyConfirmed = L["YourEmailAddressIsSuccessfullyConfirmed"].Value;
        }
    }

    public async Task SetCurrentTenant()
    {
        Guid? tenantId = await GetCurrentTenantId();

        if (tenantId.HasValue)
        {
            CurrentTenant.Change(tenantId.Value);
        }
    }
}

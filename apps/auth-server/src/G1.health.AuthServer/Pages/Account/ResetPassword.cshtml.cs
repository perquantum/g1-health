using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using G1.health.AuthServer.Web.Pages.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Volo.Abp.Account;
using Volo.Abp.Auditing;
using Volo.Abp.Identity;
using Volo.Abp.Validation;

namespace G1.health.AuthServer.Pages.Account;

//TODO: Implement live password complexity check on the razor view!
public class ResetPasswordModel : AccountPageModel
{
    [Required]
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid UserId { get; set; }

    [Required]
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public string ResetToken { get; set; }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrlHash { get; set; }

    [Required]
    [BindProperty]
    [DataType(DataType.Password)]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPasswordLength))]
    [DisableAuditing]
    public string Password { get; set; }

    [Required]
    [BindProperty]
    [DataType(DataType.Password)]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPasswordLength))]
    [DisableAuditing]
    public string ConfirmPassword { get; set; }

    public string NormalizeReturnUrl { get; set; }

    public bool LocalLoginDisabled { get; set; }

    public bool InvalidToken { get; set; }

    public virtual async Task<IActionResult> OnGetAsync()
    {
        StringValues tenantId;
        HttpContext.Request.Query.TryGetValue("__tenant", out tenantId);
        CurrentTenant.Change(new Guid(tenantId));

        var localLoginResult = await CheckLocalLoginAsync();
        if (localLoginResult != null)
        {
            LocalLoginDisabled = true;
            return localLoginResult;
        }

        ValidateModel();
        InvalidToken = !await AccountAppService.VerifyPasswordResetTokenAsync(
            new VerifyPasswordResetTokenInput
            {
                UserId = UserId,
                ResetToken = ResetToken
            }
        );

        SetNormalizeReturnUrl();
        return Page();
    }

    public virtual async Task<IActionResult> OnPostAsync()
    {
        var localLoginResult = await CheckLocalLoginAsync();
        if (localLoginResult != null)
        {
            LocalLoginDisabled = true;
            return localLoginResult;
        }

        try
        {
            ValidateModel();
            SetNormalizeReturnUrl();

            await AccountAppService.ResetPasswordAsync(
                new ResetPasswordDto
                {
                    UserId = UserId,
                    ResetToken = ResetToken,
                    Password = Password
                }
            );
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

        //TODO: Try to automatically login!
        return RedirectToPage("./ResetPasswordConfirmation", new {
            returnUrl = NormalizeReturnUrl
        });
    }

    protected override void ValidateModel()
    {
        if (!Equals(Password, ConfirmPassword))
        {
            ModelState.AddModelError("ConfirmPassword",
                L["'{0}' and '{1}' do not match.", "ConfirmPassword", "Password"]);
        }

        base.ValidateModel();
    }

    private async void SetNormalizeReturnUrl()
    {
        NormalizeReturnUrl = await GetRedirectUrlAsync(ReturnUrl, ReturnUrlHash);
    }
}

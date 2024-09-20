using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using G1.health.AuthServer.Web.Pages.Account;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.Validation;

namespace G1.health.AuthServer.Pages.Account;

public class ForgotPasswordModel : AccountPageModel
{
    [Required]
    [EmailAddress]
    [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxEmailLength))]
    [BindProperty]
    public string Email { get; set; }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; }

    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public string ReturnUrlHash { get; set; }

    public bool LocalLoginDisabled { get; set; }

    public virtual async Task<IActionResult> OnGetAsync()
    {
        var localLoginResult = await CheckLocalLoginAsync();
        if (localLoginResult != null)
        {
            LocalLoginDisabled = true;
            return localLoginResult;
        }

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
            await AccountAppService.SendPasswordResetCodeAsync(
                new SendPasswordResetCodeDto
                {
                    Email = Email,
                    AppName = "MVC", //TODO: Const!
                        ReturnUrl = ReturnUrl,
                    ReturnUrlHash = ReturnUrlHash
                }
            );
        }
        catch (BusinessException e)
        {
            Alerts.Danger(GetLocalizeExceptionMessage(e));
            return Page();
        }

        return RedirectToPage(
            "./PasswordResetLinkSent",
            new {
                returnUrl = ReturnUrl,
                returnUrlHash = ReturnUrlHash
            });
    }
}

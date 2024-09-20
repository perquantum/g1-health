using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Owl.reCAPTCHA;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Public.Web.Impersonation;
using Volo.Abp.Account.Security.Recaptcha;
using Volo.Abp.Account.Settings;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Identity;
using Volo.Abp.Settings;
using Volo.Abp.VirtualFileSystem;

namespace G1.health.AuthServer.Account;

[RemoteService(Name = AccountProPublicRemoteServiceConsts.RemoteServiceName)]
[Area(AccountProPublicRemoteServiceConsts.ModuleName)]
[Route("api/account")]
public class AccountController : AbpAccountImpersonationChallengeAccountController, IAccountAppService
{
    protected static byte[] DefaultAvatarContent = null;

    protected IAccountAppService AccountAppService { get; }

    protected IVirtualFileProvider VirtualFileProvider { get; }

    public IAbpRecaptchaValidatorFactory RecaptchaValidatorFactory { get; set; }

    protected IOptionsSnapshot<reCAPTCHAOptions> ReCaptchaOptions { get; }

    protected ISettingProvider SettingProvider { get; }

    public AccountController(
        IAccountAppService accountAppService,
        IVirtualFileProvider virtualFileProvider,
        IOptionsSnapshot<reCAPTCHAOptions> reCaptchaOptions,
        ISettingProvider settingProvider)
    {
        AccountAppService = accountAppService;
        VirtualFileProvider = virtualFileProvider;
        ReCaptchaOptions = reCaptchaOptions;
        SettingProvider = settingProvider;
        RecaptchaValidatorFactory = NullAbpRecaptchaValidatorFactory.Instance;
    }

    [HttpPost]
    [Route("register-user")]
    public virtual async Task<IdentityUserDto> RegisterAsync(RegisterDto input)
    {
        if (await UseCaptchaOnRegistration())
        {
            var reCaptchaVersion = await SettingProvider.GetAsync<int>(AccountSettingNames.Captcha.Version);
            await ReCaptchaOptions.SetAsync(reCaptchaVersion == 3 ? reCAPTCHAConsts.V3 : reCAPTCHAConsts.V2);
        }
        return await AccountAppService.RegisterAsync(input);
    }

    [HttpPost]
    [Route("send-email-confirmation-token-to-user")]
    public Task SendEmailConfirmationTokenAsync(SendEmailConfirmationTokenDto input)
    {
        return AccountAppService.SendEmailConfirmationTokenAsync(input);
    }

    [HttpGet]
    [Route("check-user-by-email/{email}")]
    public Task<bool> CheckIfUserExistsByEmail(string email)
    {
        return AccountAppService.CheckIfUserExistsByEmail(email);
    }

    protected virtual async Task<bool> UseCaptchaOnRegistration()
    {
        return await SettingProvider.IsTrueAsync(AccountSettingNames.Captcha.UseCaptchaOnRegistration);
    }

    [HttpPost]
    [Route("send-password-reset-code")]
    public virtual Task SendPasswordResetCodeAsync(SendPasswordResetCodeDto input)
    {
        return AccountAppService.SendPasswordResetCodeAsync(input);
    }

    [HttpPost]
    [Route("verify-password-reset-token")]
    public Task<bool> VerifyPasswordResetTokenAsync(VerifyPasswordResetTokenInput input)
    {
        return AccountAppService.VerifyPasswordResetTokenAsync(input);
    }

    [HttpPost]
    [Route("reset-password")]
    public virtual Task ResetPasswordAsync(ResetPasswordDto input)
    {
        return AccountAppService.ResetPasswordAsync(input);
    }

    [HttpPost]
    [Route("verify-email-confirmation-token")]
    public Task<bool> VerifyEmailConfirmationTokenAsync(VerifyEmailConfirmationTokenInput input)
    {
        return AccountAppService.VerifyEmailConfirmationTokenAsync(input);
    }

}

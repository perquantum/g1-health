using System;
using System.Threading.Tasks;
using G1.health.Shared.Utilities.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Volo.Abp.Account.Localization;
using Volo.Abp.Account.PhoneNumber;
using Volo.Abp.Account.Security.Recaptcha;
using Volo.Abp.Account.Settings;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectExtending;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using IAccountEmailer = G1.health.AuthServer.Account.Emailing.IAccountEmailer;

namespace Volo.Abp.Account;

public class AccountAppService : ApplicationService, G1.health.AuthServer.Account.IAccountAppService
{
    protected IIdentityRoleRepository RoleRepository { get; }
    protected IIdentitySecurityLogRepository SecurityLogRepository { get; }
    protected IdentityUserManager UserManager { get; }
    protected IAccountEmailer AccountEmailer { get; }
    protected IAccountPhoneService PhoneService { get; }
    protected IdentitySecurityLogManager IdentitySecurityLogManager { get; }
    public IAbpRecaptchaValidatorFactory RecaptchaValidatorFactory { get; set; }

    protected ISettingManager SettingManager { get; }
    protected IBlobContainer<AccountProfilePictureContainer> AccountProfilePictureContainer { get; }
    protected IOptions<IdentityOptions> IdentityOptions { get; }
    protected IApplicationInfoAccessor ApplicationInfoAccessor { get; }
    protected IdentityUserTwoFactorChecker IdentityUserTwoFactorChecker { get; }
    private readonly ICurrentTenant _currentTenant;
    public AccountAppService(
        IdentityUserManager userManager,
        IAccountEmailer accountEmailer,
        IAccountPhoneService phoneService,
        IIdentityRoleRepository roleRepository,
        IdentitySecurityLogManager identitySecurityLogManager,
        IBlobContainer<AccountProfilePictureContainer> accountProfilePictureContainer,
        ISettingManager settingManager,
        IOptions<IdentityOptions> identityOptions,
        IIdentitySecurityLogRepository securityLogRepository,
        IApplicationInfoAccessor applicationInfoAccessor,
        IdentityUserTwoFactorChecker identityUserTwoFactorChecker,
        ICurrentTenant currentTenant)
    {
        RoleRepository = roleRepository;
        IdentitySecurityLogManager = identitySecurityLogManager;
        UserManager = userManager;
        AccountEmailer = accountEmailer;
        PhoneService = phoneService;
        AccountProfilePictureContainer = accountProfilePictureContainer;
        SettingManager = settingManager;
        IdentityOptions = identityOptions;
        SecurityLogRepository = securityLogRepository;
        ApplicationInfoAccessor = applicationInfoAccessor;
        IdentityUserTwoFactorChecker = identityUserTwoFactorChecker;
        _currentTenant = currentTenant;

        LocalizationResource = typeof(AccountResource);
        RecaptchaValidatorFactory = NullAbpRecaptchaValidatorFactory.Instance;
    }

    public virtual async Task<IdentityUserDto> RegisterAsync(RegisterDto input)
    {
        await CheckSelfRegistrationAsync();

        if (await UseCaptchaOnRegistration())
        {
            var reCaptchaValidator = await RecaptchaValidatorFactory.CreateAsync();
            await reCaptchaValidator.ValidateAsync(input.CaptchaResponse);
        }

        await IdentityOptions.SetAsync();

        var user = new IdentityUser(GuidGenerator.Create(), input.UserName, input.EmailAddress, null);
        user.SetIsActive(false);

        input.MapExtraPropertiesTo(user);

        foreach(var key in input.ExtraProperties.Keys)
        {
            switch (key)
            {
                case RegistrationConsts.Name:
                    user.Name = input.ExtraProperties[key].ToString();
                    break;
                case RegistrationConsts.Surname:
                    user.Surname = input.ExtraProperties[key].ToString();
                    break;
                case RegistrationConsts.PhoneNumber:
                    user.SetPhoneNumber(input.ExtraProperties[key].ToString(), false);
                    break;
                case RegistrationConsts.City:
                    break;
                default:
                    user.SetProperty(key, input.ExtraProperties[key]);
                    break;
            }
        }

        (await UserManager.CreateAsync(user, input.Password)).CheckErrors();
        (await UserManager.AddDefaultRolesAsync(user)).CheckErrors();

        if (!user.EmailConfirmed)
        {
            await SendEmailConfirmationTokenAsync(user, input.AppName, input.ReturnUrl, input.ReturnUrlHash);
        }

        return ObjectMapper.Map<IdentityUser, IdentityUserDto>(user);
    }

    public virtual async Task SendEmailConfirmationTokenAsync(SendEmailConfirmationTokenDto input)
    {
        using (CurrentTenant.Change(null))
        {
            var user = await UserManager.GetByIdAsync(input.UserId);
            await SendEmailConfirmationTokenAsync(user, input.AppName, input.ReturnUrl, input.ReturnUrlHash);
        }
    }

    public virtual async Task<bool> CheckIfUserExistsByEmail(string email)
    {
        using (CurrentTenant.Change(null))
        {
            var user = await UserManager.FindByEmailAsync(email);
            if (user != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    protected virtual async Task SendEmailConfirmationTokenAsync(
        IdentityUser user,
        string applicationName,
        string returnUrl,
        string returnUrlHash)
    {
        var confirmationToken = await UserManager.GenerateEmailConfirmationTokenAsync(user);
        await AccountEmailer.SendEmailConfirmationLinkAsync(user, confirmationToken, applicationName, returnUrl, returnUrlHash);
    }

    protected virtual async Task CheckSelfRegistrationAsync()
    {
        if (!await SettingProvider.IsTrueAsync(AccountSettingNames.IsSelfRegistrationEnabled))
        {
            throw new UserFriendlyException(L["Volo.Account:SelfRegistrationDisabled"]);
        }
    }

    protected virtual async Task<bool> UseCaptchaOnRegistration()
    {
        return await SettingProvider.IsTrueAsync(AccountSettingNames.Captcha.UseCaptchaOnRegistration);
    }

    public virtual async Task SendPasswordResetCodeAsync(SendPasswordResetCodeDto input)
    {
        var user = await GetUserByEmail(input.Email);
        var resetToken = await UserManager.GeneratePasswordResetTokenAsync(user);
        await AccountEmailer.SendPasswordResetLinkAsync(user, resetToken, input.AppName, input.ReturnUrl, input.ReturnUrlHash);
    }

    public virtual async Task<bool> VerifyPasswordResetTokenAsync(VerifyPasswordResetTokenInput input)
    {
        var user = await UserManager.GetByIdAsync(input.UserId);
        return await UserManager.VerifyUserTokenAsync(
            user,
            UserManager.Options.Tokens.PasswordResetTokenProvider,
            UserManager<IdentityUser>.ResetPasswordTokenPurpose,
            input.ResetToken);
    }

    public virtual async Task ResetPasswordAsync(ResetPasswordDto input)
    {
        await IdentityOptions.SetAsync();

        var user = await UserManager.GetByIdAsync(input.UserId);
        (await UserManager.ResetPasswordAsync(user, input.ResetToken, input.Password)).CheckErrors();

        await IdentitySecurityLogManager.SaveAsync(new IdentitySecurityLogContext
        {
            Identity = IdentitySecurityLogIdentityConsts.Identity,
            Action = IdentitySecurityLogActionConsts.ChangePassword
        });
    }
    protected virtual async Task<IdentityUser> GetUserByEmail(string email)
    {
        var user = await UserManager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new UserFriendlyException(L["Volo.Account:InvalidEmailAddress", email]);
        }

        return user;
    }
public virtual async Task<bool> VerifyEmailConfirmationTokenAsync(VerifyEmailConfirmationTokenInput input)
    {
        var user = await UserManager.GetByIdAsync(input.UserId);
        return await UserManager.VerifyUserTokenAsync(
            user,
            UserManager.Options.Tokens.EmailConfirmationTokenProvider,
            UserManager<IdentityUser>.ConfirmEmailTokenPurpose,
            input.Token);
    }

}

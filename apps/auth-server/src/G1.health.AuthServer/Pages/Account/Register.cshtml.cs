using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using G1.health.ClinicService.ClinicSetup;
using G1.health.Shared.Hosting.Microservices.DbMigrations.Events;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Owl.reCAPTCHA;
using SendGrid;
using SendGrid.Helpers.Mail;
using survey.FormsService.Forms;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Public.Web.Pages.Account;
using Volo.Abp.Account.Public.Web.Security.Recaptcha;
using Volo.Abp.Account.Settings;
using Volo.Abp.AspNetCore.Mvc.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc.UI.Alerts;
using Volo.Abp.Data;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using Volo.Abp.Identity.Settings;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.Settings;
using Volo.Abp.Uow;
using Volo.Abp.Validation;
using Volo.Forms;
using Volo.Forms.Questions;
using IdentityUser = Volo.Abp.Identity.IdentityUser;
using G1.health.Shared.Utilities.Common;
using G1.health.ClinicService.Common;
using IAccountAppService = G1.health.AuthServer.Account.IAccountAppService;
using System.ComponentModel;
using G1.health.ClinicService.Localization;
using Microsoft.Extensions.Localization;
using G1.health.ClinicService.Practitioner;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace G1.health.AuthServer.Pages.Account;

public class RegisterModel : Web.Pages.Account.AccountPageModel
{
    public IAccountAppService AccountAppService { get; set; }

    public IAbpTenantAppService AbpTenantAppService { get; set; }

    public IFormsAppService FormsAppService { get; set; }

    public IUsersAppService UsersAppService { get; set; }

    public readonly IAlertManager AlertManager;

    private readonly IUnitOfWorkManager UnitOfWorkManager;

    public IDataFilter DataFilter { get; set; }

    private readonly IConfiguration _config;

    protected readonly IDistributedEventBus DistributedEventBus;

    protected readonly ICommonAppService CommonAppService;

    public const string SessionKey = "_FormDto";

    public const string Password = "NewPassword#12";

    public readonly IStringLocalizer<ClinicServiceResource> ClinicServiceLocalizer;

    public RegisterModel(
        IAccountAppService accountAppService,
        IAbpTenantAppService abpTenantAppService,
        IFormsAppService formsAppService,
        IUsersAppService usersAppService,
        IUnitOfWorkManager unitOfWorkManager,
        IAlertManager alertManager,
        IDataFilter dataFilter,
        IConfiguration config,
        IDistributedEventBus distributedEventBus,
        ICommonAppService commonAppService,
        IStringLocalizer<ClinicServiceResource> clinicServiceLocalizer)
    {
        AccountAppService = accountAppService;
        AbpTenantAppService = abpTenantAppService;
        FormsAppService = formsAppService;
        UsersAppService = usersAppService;
        UnitOfWorkManager = unitOfWorkManager;
        AlertManager = alertManager;
        DataFilter = dataFilter;
        _config = config;
        DistributedEventBus = distributedEventBus;
        CommonAppService = commonAppService;
        ClinicServiceLocalizer = clinicServiceLocalizer;
    }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrlHash { get; set; }

    //[BindProperty(SupportsGet = true)]
    //public Guid? TenantId { get; set; }

    [BindProperty]
    public PostInput Input { get; set; }

    [BindProperty]
    public List<Field> Fields { get; set; } = new List<Field>();

    [BindProperty]
    public List<QuestionDto> QuestionDtos { get; set; } = new List<QuestionDto>();

    private async Task CreateExtraFields()
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
        catch
        {

        }

        if (tenant.TenantId != null)
        {
            var form = await FormsAppService.GetRegistrationFormAsync((Guid)tenant.TenantId);
            QuestionDtos = form.Questions;
            HttpContext.Session.SetString(SessionKey, JsonConvert.SerializeObject(QuestionDtos));
        }

        foreach (var item in QuestionDtos)
        {
            if (item.Title != RegistrationConsts.CreationFlag)
            {
                var choices = new List<SelectListItem>();

                foreach (var choice in item.Choices)
                {
                    choices.Add(new SelectListItem()
                    {
                        Text = choice.Value,
                        Value = choice.Value
                    });
                }

                Field field = new Field
                {
                    Index = item.Index,
                    Title = item.Title,
                    Description = item.Description,
                    IsRequired = item.IsRequired,
                    HasOtherOption = item.HasOtherOption,
                    QuestionType = item.QuestionType,
                    Answer = null,
                    Choices = choices,
                    Answers = null
                };
                Fields.Add(field);
            }
        }
    }

    private async Task SetExtraFields()
    {
        Fields = new List<Field>();
        string result = HttpContext.Session.GetString(SessionKey);
        QuestionDtos = JsonConvert.DeserializeObject<List<QuestionDto>>(result);

        foreach (var item in QuestionDtos)
        {
            if (item.Title != RegistrationConsts.CreationFlag)
            {
                var choices = new List<SelectListItem>();

                foreach (var choice in item.Choices)
                {
                    choices.Add(new SelectListItem()
                    {
                        Text = choice.Value,
                        Value = choice.Value
                    });
                }

                Field field = new Field
                {
                    Index = item.Index,
                    Title = item.Title,
                    Description = item.Description,
                    IsRequired = item.IsRequired,
                    HasOtherOption = item.HasOtherOption,
                    QuestionType = item.QuestionType,
                    Answer = null,
                    Choices = choices,
                    Answers = null
                };
                Fields.Add(field);
            }
        }
    }

    [BindProperty(SupportsGet = true)]
    public bool IsExternalLogin { get; set; }

    public bool LocalLoginDisabled { get; set; }

    public bool UseCaptcha { get; set; }

    public string RegisterText { get; set; }

    public string AgreeTermsAndConditionsText { get; set; }

    public virtual async Task<IActionResult> OnGetAsync()
    {

        var localLoginResult = await CheckLocalLoginAsync();
        if (localLoginResult != null)
        {
            LocalLoginDisabled = true;
            return localLoginResult;
        }

        await CheckSelfRegistrationAsync();
        await TrySetEmailAsync();
        await SetUseCaptchaAsync();

        await CreateExtraFields();

        return Page();
    }

    [UnitOfWork] //TODO: Will be removed when we implement action filter
    public virtual async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await CheckSelfRegistrationAsync();
            await SetUseCaptchaAsync();

            string result = HttpContext.Session.GetString(SessionKey);

            IdentityUser existingUser;
            using (CurrentTenant.Change(null))
            {
                existingUser = await UserManager.FindByEmailAsync(Input.EmailAddress);
            }

            if (existingUser != null && result != null)
            {
                AlertManager.Alerts.Add(AlertType.Danger, L["Volo.Abp.Identity:DuplicateEmail", Input.EmailAddress]);
                await SetExtraFields();
                return Page();
            }

            if (result == null)
            {
                return RedirectToPage("./Login", new
                {
                    returnUrl = ReturnUrl,
                    returnUrlHash = ReturnUrlHash
                });
            }
            else
            {
                QuestionDtos = JsonConvert.DeserializeObject<List<QuestionDto>>(result);

                foreach (var item in QuestionDtos)
                {
                    if (item.Title != RegistrationConsts.CreationFlag && item.Title != RegistrationConsts.TermsAndConditions)
                    {
                        var choices = new List<SelectListItem>();

                        foreach (var choice in item.Choices)
                        {
                            choices.Add(new SelectListItem()
                            {
                                Text = choice.Value,
                                Value = choice.Value
                            });
                        }

                        Field field = Fields.Where(x => x.Index == item.Index).Select(x => x).FirstOrDefault();
                        //if(field!= null)
                        field.Choices = choices;
                    }
                }

                IdentityUser user;
                if (IsExternalLogin)
                {
                    var externalLoginInfo = await SignInManager.GetExternalLoginInfoAsync();
                    if (externalLoginInfo == null)
                    {
                        Logger.LogWarning("External login info is not available");
                        return RedirectToPage("./Login");
                    }

                    user = await RegisterExternalUserAsync(externalLoginInfo, Input.EmailAddress);
                }
                else
                {
                    var localLoginResult = await CheckLocalLoginAsync();
                    if (localLoginResult != null)
                    {
                        LocalLoginDisabled = true;
                        return localLoginResult;
                    }

                    user = await RegisterLocalUserAsync();
                    if(user == null)
                    {
                        return Page();
                    }
                }

                if (await SettingProvider.IsTrueAsync(IdentitySettingNames.SignIn.RequireConfirmedEmail) && !user.EmailConfirmed ||
                    await SettingProvider.IsTrueAsync(IdentitySettingNames.SignIn.RequireConfirmedPhoneNumber) && !user.PhoneNumberConfirmed)
                {
                    await StoreConfirmUser(user);

                    return RedirectToPage("./ConfirmUser", new
                    {
                        returnUrl = ReturnUrl,
                        returnUrlHash = ReturnUrlHash
                    });
                }

                //await SignInManager.SignInAsync(user, isPersistent: true);

                return RedirectToPage("./ThankYou");
            }
        }
        catch (BusinessException ex)
        {
            AlertManager.Alerts.Add(AlertType.Danger, ex.Message);
            await SetExtraFields();
            return Page();
        }
    }

    protected virtual async Task<IdentityUser> RegisterLocalUserAsync()
    {
        using (var unitOfWork = UnitOfWorkManager.Begin())
        {
            try
            {
                if (!Input.TermsAndConditions)
                {
                    return null;
                }
                ValidateModel();

                var captchaResponse = string.Empty;
                if (UseCaptcha)
                {
                    captchaResponse = HttpContext.Request.Form[RecaptchaValidatorBase.RecaptchaResponseKey];
                }

                RegisterDto registerDto = new RegisterDto()
                {
                    AppName = "MVC",
                    EmailAddress = Input.EmailAddress,
                    Password = Password,
                    UserName = Input.EmailAddress,
                    ReturnUrl = ReturnUrl,
                    ReturnUrlHash = ReturnUrlHash,
                    CaptchaResponse = captchaResponse
                };

                string countryCode = Fields.Where(x => x.Title == RegistrationConsts.CountryCode).Select(x => x.Answer).FirstOrDefault();

                foreach (var item in Fields)
                {
                    if (item.Title != RegistrationConsts.Email && item.Title != RegistrationConsts.CountryCode && item.Title != RegistrationConsts.TermsAndConditions)
                    {
                        if (item.Answers != null)
                        {
                            string result = "{";
                            foreach (var answer in item.Answers)
                            {
                                result += '"' + answer + '"';
                                if (!(answer == item.Answers.LastOrDefault()))
                                {
                                    result += ",";
                                }
                            }
                            result += "}";
                            item.Answer = result;
                        }

                        if (item.Title == RegistrationConsts.PhoneNumber)
                        {
                            item.Answer = countryCode + item.Answer;
                        }

                        registerDto.SetProperty(item.Title, item.Answer);
                    }
                }

                var UserDetails = "";
                var userName = "";
                foreach (var item in registerDto.ExtraProperties)
                {
                    if (item.Key != RegistrationConsts.Email && item.Key != RegistrationConsts.CountryCode && item.Key != RegistrationConsts.TermsAndConditions)
                    {
                        UserDetails += $"{item.Key}: {item.Value}<br>";

                        if (item.Key.ToLower() == "name")
                        {
                            userName = item.Value.ToString();
                        }
                    }
                }

                registerDto.SetProperty(RegistrationConsts.TermsAndConditions, Input.TermsAndConditions);

                UserDetails += $"Email : {Input.EmailAddress}.";
                
                var userDto = await AccountAppService.RegisterAsync(registerDto);

                    RegisterUser registerUser = new RegisterUser(userDto.Id, Input.EmailAddress, Input.EmailAddress, CurrentTenant.Id, userDto.CreationTime, "Patient", Password);

                foreach (var key in Fields)
                {
                    switch (key.Title.Trim())
                    {
                        case RegistrationConsts.Name:
                            registerUser.first_name = key.Answer;
                            break;
                        case RegistrationConsts.Surname:
                            registerUser.last_name = key.Answer;
                            break;
                        case RegistrationConsts.DateOfBirth:
                            registerUser.dob = DateTime.Parse(key.Answer);
                            break;
                        case RegistrationConsts.Gender:
                            registerUser.gender = key.Answer;
                            break;
                        case RegistrationConsts.PhoneNumber:
                            registerUser.PhoneNumber = key.Answer;
                            break;
                        case RegistrationConsts.City:
                            registerUser.city = key.Answer;
                            break;
                        case RegistrationConsts.CountryCode:
                            registerUser.country_code = key.Answer;
                            break;
                        case RegistrationConsts.PatientFirstName:
                            registerUser.PatientFirstName = key.Answer;
                            break;
                        case RegistrationConsts.PatientLastName:
                            registerUser.PatientLastName = key.Answer;
                            break;
                        case RegistrationConsts.PatientAge:
                            registerUser.PatientAge = Convert.ToInt16(key.Answer);
                            break;
                        case RegistrationConsts.Concerns:
                            registerUser.Concerns = key.Answer;
                            break;
                        default:
                            break;
                    }
                }

                var registeredUser = await UsersAppService.RegisterUser(registerUser);

                string questions = HttpContext.Session.GetString(SessionKey);
                QuestionDtos = JsonConvert.DeserializeObject<List<QuestionDto>>(questions);
                var creationFlagQuestion = QuestionDtos.FirstOrDefault(x => x.Title == RegistrationConsts.CreationFlag);
                if (creationFlagQuestion != null)
                {
                    var actionFlag = Convert.ToInt16(QuestionDtos.Where(x => x.Title == RegistrationConsts.CreationFlag).First().Choices.First().Value);

                    switch (actionFlag)
                    {
                        case 0:
                            break;
                        case 1:
                            var day_type = registerDto.ExtraProperties.Where(x => x.Key == AppointmentCreateConsts.PreferredDay).Select(x => x.Value.ToString()).FirstOrDefault();
                            var slot_time = registerDto.ExtraProperties.Where(x => x.Key == AppointmentCreateConsts.PreferredTimeSlot).Select(x => x.Value.ToString()).FirstOrDefault();

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
                            catch
                            {

                            }

                            var org_branch_id = await CommonAppService.GetOrganizationBranchIdAsync(tenant.TenantId.ToString());

                            if (day_type != null && slot_time != null)
                            {
                                await DistributedEventBus.PublishAsync(new SessionCreatedEto
                                {
                                    PatientId = registeredUser.patient_id,
                                    PreferredDay = day_type,
                                    PreferredSession = slot_time,
                                    Concerns = registerUser.Concerns,
                                    OrganizationBranchId = org_branch_id
                                });
                            }
                            break;
                        case 2:
                            break;
                    }
                }

                //var day_type = registerDto.ExtraProperties.Where(x => x.Key == AppointmentCreateConsts.PreferredDay).Select(x => x.Value.ToString()).FirstOrDefault();
                //var slot_time = registerDto.ExtraProperties.Where(x => x.Key == AppointmentCreateConsts.PreferredTimeSlot).Select(x => x.Value.ToString()).FirstOrDefault();

                //if (day_type != null && slot_time != null)
                //{

                //    var genders = Genders.GendersList;
                //    var org_id = await CommonAppService.GetOrganizationIdAsync(CurrentTenant.Id.ToString());
                //    var org_branch_id = await CommonAppService.GetOrganizationBranchIdAsync(CurrentTenant.Id.ToString());

                //    await DistributedEventBus.PublishAsync(new AppointmentCreateEto
                //    {
                //        first_name = registerUser.first_name,
                //        last_name = registerUser.last_name,
                //        date_of_birth = registerUser.dob,
                //        phone_number = registerUser.PhoneNumber,
                //        email_id = Input.EmailAddress,
                //        symptoms = registerDto.ExtraProperties.Where(x => x.Key == AppointmentCreateConsts.OfficialDiagnosis).Select(x => x.Value.ToString()).FirstOrDefault(),
                //        org_id = org_id,
                //        org_branch_id = org_branch_id,
                //        day_type = day_type,
                //        slot_time = slot_time,
                //        gender = genders.Where(x => x.Key == registerUser.gender).Select(x => x.Value).FirstOrDefault(3),
                //        token = TokenGenerator(),
                //        country_code = countryCode,
                //        patient_id = registeredUser.patient_id,
                //        created_by_id = registeredUser.user_id,
                //        tenant_name = CurrentTenant.Name.ToUpper()
                //    });
                //}
                await SendUserDetails(UserDetails, Input.EmailAddress.ToString(), userName);
                await unitOfWork.CompleteAsync();
                
                using (DataFilter.Disable<IMultiTenant>())
                {
                    return await UserManager.GetByIdAsync(userDto.Id);
                }
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                unitOfWork.Dispose();
                AlertManager.Alerts.Add(AlertType.Danger, ex.Message);
                return null;
            }
        }
    }

    protected virtual async Task<IdentityUser> RegisterExternalUserAsync(ExternalLoginInfo externalLoginInfo, string emailAddress)
    {
        await IdentityOptions.SetAsync();

        var user = new IdentityUser(GuidGenerator.Create(), emailAddress, emailAddress, CurrentTenant.Id);

        (await UserManager.CreateAsync(user)).CheckErrors();
        (await UserManager.AddDefaultRolesAsync(user)).CheckErrors();

        if (!user.EmailConfirmed)
        {
            await AccountAppService.SendEmailConfirmationTokenAsync(
                new SendEmailConfirmationTokenDto
                {
                    AppName = "MVC",
                    UserId = user.Id,
                    ReturnUrl = ReturnUrl,
                    ReturnUrlHash = ReturnUrlHash
                }
            );
        }
        
        var userLoginAlreadyExists = user.Logins.Any(x =>
            x.TenantId == user.TenantId &&
            x.LoginProvider == externalLoginInfo.LoginProvider &&
            x.ProviderKey == externalLoginInfo.ProviderKey);

        if (!userLoginAlreadyExists)
        {
            user.AddLogin(new UserLoginInfo(
                    externalLoginInfo.LoginProvider,
                    externalLoginInfo.ProviderKey,
                    externalLoginInfo.ProviderDisplayName
                )
            );

            (await UserManager.UpdateAsync(user)).CheckErrors();
        }

        return user;
    }

    protected virtual async Task CheckSelfRegistrationAsync()
    {
        if (!await SettingProvider.IsTrueAsync(AccountSettingNames.IsSelfRegistrationEnabled) ||
            !await SettingProvider.IsTrueAsync(AccountSettingNames.EnableLocalLogin))
        {
            throw new UserFriendlyException(L["SelfRegistrationDisabledMessage"]);
        }
    }

    protected virtual async Task SetUseCaptchaAsync()
    {
        UseCaptcha = !IsExternalLogin && await SettingProvider.IsTrueAsync(AccountSettingNames.Captcha.UseCaptchaOnRegistration);
        if (UseCaptcha)
        {
            var reCaptchaVersion = await SettingProvider.GetAsync<int>(AccountSettingNames.Captcha.Version);
            await ReCaptchaOptions.SetAsync(reCaptchaVersion == 3 ? reCAPTCHAConsts.V3 : reCAPTCHAConsts.V2);
        }
    }

    protected virtual async Task StoreConfirmUser(IdentityUser user)
    {
        var identity = new ClaimsIdentity(ConfirmUserModel.ConfirmUserScheme);
        identity.AddClaim(new Claim(AbpClaimTypes.UserId, user.Id.ToString()));
        if (user.TenantId.HasValue)
        {
            identity.AddClaim(new Claim(AbpClaimTypes.TenantId, user.TenantId.ToString()));
        }
        await HttpContext.SignInAsync(ConfirmUserModel.ConfirmUserScheme, new ClaimsPrincipal(identity));
    }

    private async Task TrySetEmailAsync()
    {
        if (IsExternalLogin)
        {
            var externalLoginInfo = await SignInManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo == null)
            {
                return;
            }

            if (!externalLoginInfo.Principal.Identities.Any())
            {
                return;
            }

            var identity = externalLoginInfo.Principal.Identities.First();
            var emailClaim = identity.FindFirst(ClaimTypes.Email);

            if (emailClaim == null)
            {
                return;
            }

            Input = new PostInput { EmailAddress = emailClaim.Value };
        }
    }
    public async Task SendUserDetails(string UserDetails, string email,string username)
    {
        try
        {
            var apiKey = _config.GetSection("UserDetailsEmailing:apiKey").Value;
            var client = new SendGridClient(apiKey);

            SendGridMessage msg = new SendGridMessage();
            //var adminEmail = _config.GetSection("UserDetailsEmailing:adminEmail").Value;
            //msg.SetFrom(new EmailAddress(adminEmail, "Scope"));
            var adminEmail = await CommonAppService.GetAdminEmails();
            //var emailList = adminEmail.Split(',');

            var host = Request.Host.Host;
            var tenant = new FindTenantResultDto();
  
            msg.SetFrom(new EmailAddress(adminEmail[0], CurrentTenant.Name.ToUpper()));

            foreach (var item in adminEmail)
            {
                var recipient = new EmailAddress(item, username);
                msg.AddTo(recipient);
            }
 
            string template_Id = await CommonAppService.GetEmailTemplateId(CurrentTenant.Id.ToString(), EmailTemplatesConsts.registration);
            //var templateId = _config.GetSection("UserDetailsEmailing:templateId").Value;
            msg.SetTemplateId(template_Id);
            var loginUrl = Url.PageLink();
            loginUrl = loginUrl.Replace("/Register", "/login");
            var adminName = _config.GetSection("UserDetailsEmailing:adminName").Value;
            // Add dynamic template data
            var dynamicTemplateData = new
            {
                //admin_name = adminName,
                UserDetails = UserDetails,
                LoginUrl = loginUrl
                //tenantName = CurrentTenant.Name.ToUpper(),
            };
            msg.SetTemplateData(dynamicTemplateData);
            var response = await client.SendEmailAsync(msg);
            if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                response.StatusCode != System.Net.HttpStatusCode.Accepted)
            {
                Logger.LogInformation(response.Body.ToString());
                Logger.LogInformation(response.StatusCode.ToString());
                throw new Exception($"Failed to send confirmation email(Auth)");
            }
        }
        catch(Exception ex)
        {
            Logger.LogInformation(ex.Message);
            throw new Exception($"Failed to send confirmation email");
        }

    }

    private string TokenGenerator()
    {
        int min = 100000;
        int max = 999999;
        Random random = new Random();
        int token = random.Next(0, int.MaxValue) * (max - min + 1) + min;
        return token.ToString();
    }

    public class PostInput
    {
        //[Required]
        //[DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxUserNameLength))]
        //public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxEmailLength))]
        [DisplayName("Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        [RequiredCheckbox]
        public bool TermsAndConditions { get; set; }

        //[Required]
        //[DynamicStringLength(typeof(IdentityUserConsts), nameof(IdentityUserConsts.MaxPasswordLength))]
        //[DataType(DataType.Password)]
        //[DisableAuditing]
        //public string Password { get; set; }

    }

    public class Field
    {
        public int Index { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool IsRequired { get; set; }
        public bool HasOtherOption { get; set; }
        public QuestionTypes QuestionType { get; set; }
        public string? Answer { get; set; }
        public List<string>? Answers { get; set; }
        public List<SelectListItem>? Choices { get; set; }
    }

    public class RequiredCheckboxAttribute : ValidationAttribute
    {
        public RequiredCheckboxAttribute()
        {
            ErrorMessage = "Please accept Terms & Conditions.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is bool isChecked && isChecked)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage);
        }
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
        catch
        {

        }
        return tenant.TenantId;
    }

    public async Task SetLocalizationTexts()
    {
        Guid? tenantId = await GetCurrentTenantId();

        using (CurrentTenant.Change(tenantId))
        {
            RegisterText = L["Register"].Value;
            AgreeTermsAndConditionsText = ClinicServiceLocalizer["AgreeToTermsAndConditions"].Value;
        }
    }
}

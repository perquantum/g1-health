using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace G1.health.AuthServer.Account;

public interface IAccountAppService : IApplicationService, IRemoteService
{
    Task<IdentityUserDto> RegisterAsync(RegisterDto input);

    Task SendEmailConfirmationTokenAsync(SendEmailConfirmationTokenDto input);

    Task<bool> CheckIfUserExistsByEmail(string email);
    Task SendPasswordResetCodeAsync(SendPasswordResetCodeDto input);
    Task<bool> VerifyPasswordResetTokenAsync(VerifyPasswordResetTokenInput input);
    Task ResetPasswordAsync(ResetPasswordDto input);
    Task<bool> VerifyEmailConfirmationTokenAsync(VerifyEmailConfirmationTokenInput input);


}
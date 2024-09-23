using G1.health.IdentityService.Roles;
using G1.health.IdentityService.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;

namespace G1.health.AuthServer;

public class MyUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<Volo.Abp.Identity.IdentityUser, Volo.Abp.Identity.IdentityRole>, ITransientDependency
{
    protected ICurrentPrincipalAccessor CurrentPrincipalAccessor { get; }
    protected IAbpClaimsPrincipalFactory AbpClaimsPrincipalFactory { get; }
    protected MyIdentityUserManager IdentityUserManager { get; }
    protected IdentityRoleManager IdentityRoleManager { get; }

    public MyUserClaimsPrincipalFactory(
        UserManager<Volo.Abp.Identity.IdentityUser> userManager,
        RoleManager<Volo.Abp.Identity.IdentityRole> roleManager,
        IOptions<IdentityOptions> options,
        ICurrentPrincipalAccessor currentPrincipalAccessor,
        IAbpClaimsPrincipalFactory abpClaimsPrincipalFactory,
        MyIdentityUserManager identityUserManager,
        IdentityRoleManager identityRoleManager)
        : base(
            userManager,
            roleManager,
            options)
    {
        CurrentPrincipalAccessor = currentPrincipalAccessor;
        AbpClaimsPrincipalFactory = abpClaimsPrincipalFactory;
        IdentityUserManager = identityUserManager;
        IdentityRoleManager = identityRoleManager;
    }

    [UnitOfWork]
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Volo.Abp.Identity.IdentityUser user)
    {
        var id = await base.GenerateClaimsAsync(user).ConfigureAwait(false);
        if (UserManager.SupportsUserRole)
        {
            var roles = await IdentityUserManager.GetRoleNamesAsync(user).ConfigureAwait(false);
            foreach (var roleName in roles)
            {
                id.AddClaim(new Claim(Options.ClaimsIdentity.RoleClaimType, roleName));
                if (RoleManager.SupportsRoleClaims)
                {
                    var role = await IdentityRoleManager.FindByNameAsync(roleName).ConfigureAwait(false);
                    if (role != null)
                    {
                        id.AddClaims(await RoleManager.GetClaimsAsync(role).ConfigureAwait(false));
                    }
                }
            }
        }
        return id;
    }
}

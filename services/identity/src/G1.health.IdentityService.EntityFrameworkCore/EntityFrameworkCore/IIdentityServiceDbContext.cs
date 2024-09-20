using G1.health.IdentityService.Roles;
using G1.health.IdentityService.Users;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;

namespace G1.health.IdentityService.EntityFrameworkCore;

[ConnectionStringName("AbpIdentity")]
public interface IIdentityServiceDbContext : IIdentityDbContext
{
    DbSet<IdentityUser> Users { get; }

    DbSet<IdentityRole> Roles { get; }

    DbSet<IdentityClaimType> ClaimTypes { get; }

    DbSet<OrganizationUnit> OrganizationUnits { get; }

    DbSet<IdentitySecurityLog> SecurityLogs { get; }

    DbSet<IdentityLinkUser> LinkUsers { get; }

    DbSet<IdentityUserDelegation> UserDelegations { get; }

    DbSet<IdentitySession> Sessions { get; }

    DbSet<UserTenantAssociation> UserTenantAssociations { get; }

    DbSet<TenantRolesAssociation> TenantRolesAssociations { get; }
}
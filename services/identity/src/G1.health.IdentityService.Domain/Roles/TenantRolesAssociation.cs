using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace G1.health.IdentityService.Roles;

public class TenantRolesAssociation : Entity<Guid>, IMultiTenant
{
    public Guid RoleId { get; set; }

    public Guid? TenantId { get; set; }

    public bool IsDefault { get; set; }

    public bool IsPublic { get; set; }

    public TenantRolesAssociation()
    {

    }

    public TenantRolesAssociation(Guid id, Guid roleId, Guid? tenantId, bool isDefault, bool isPublic)
    {
        Id = id;
        RoleId = roleId;
        TenantId = tenantId;
        IsDefault = isDefault;
        IsPublic = isPublic;
    }
}
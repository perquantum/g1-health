using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace G1.health.IdentityService.Users;

public class UserTenantAssociation : Entity<Guid>, IMultiTenant
{
    public Guid UserId { get; set; }

    public Guid? TenantId { get; set; }

    public bool IsActive { get; set; }

    public UserTenantAssociation()
    {

    }

    public UserTenantAssociation(Guid id, Guid userId, Guid? tenantId, bool isActive)
    {
        Id = id;
        UserId = userId;
        TenantId = tenantId;
        IsActive = isActive;
    }
}
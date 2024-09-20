using System;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus;

namespace G1.health.IdentityService.Events;

[Serializable]
[EventName("Empowerm.Tenant.Admin.Create")]
public class TenantAdminCreateEto : EtoBase
{
    public Guid Id { get; set; }

    public string Name { get; set; }
}
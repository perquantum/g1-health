using System;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus;

namespace G1.health.Shared.Hosting.Microservices.DbMigrations.Events;

[Serializable]
[EventName("Empowerm.Session.Created")]
public class SessionCreatedEto : EtoBase
{
    public long PatientId { get; set; }
    public string PreferredDay { get; set; }
    public string PreferredSession { get; set; }
    public string? Concerns { get; set; }
    public long? OrganizationBranchId { get; set; }
}

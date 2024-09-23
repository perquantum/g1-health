using System;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus;

namespace G1.health.Shared.Hosting.Microservices.DbMigrations.Events;

[Serializable]
[EventName("Abp.Identity.User.Created")]
public class UserCreatedEto : EtoBase
{
    public Guid Id { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public string Username { get; set; }

    public Guid CurrentUserId { get; set; }

    public Guid? TenantId { get; set; }
}
using System;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus;

namespace G1.health.IdentityService.Events;

[Serializable]
[EventName("Empowerm.User.Create")]
public class PlatformUserCreateEto : EtoBase
{
    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Password { get; set; }

    public string Username { get; set; }

    public int RegistrationType { get; set; }

    public string Email { get; set; }

    public bool IsExternal { get; set; } = false;

    public string? PhoneNumber { get; set; }

    public bool TwoFactorEnabled { get; set; } = false;

    public DateTime? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; } = false;

    public bool ShouldChangePasswordOnNextLogin { get; set; } = false;

    public long UserId { get; set; }

    public Guid? CurrentuserId { get; set; }

    public long CountryId { get; set; }

    public string DoctorType { get; set; }

    public Guid? LoggedInUserId { get; set; }
}

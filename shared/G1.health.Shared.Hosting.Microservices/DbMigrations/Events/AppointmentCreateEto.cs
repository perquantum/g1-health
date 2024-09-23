using System;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus;

namespace G1.health.Shared.Hosting.Microservices.DbMigrations.Events;

[Serializable]
[EventName("Empowerm.Appointment.Created")]
public class AppointmentCreateEto : EtoBase
{
    public string? first_name { get; set; }

    public string? last_name { get; set; }

    public DateTime? date_of_birth { get; set; }

    public string? phone_number { get; set; }

    public string? email_id { get; set; }

    public long? org_id { get; set; }

    public long? org_branch_id { get; set; }

    public string day_type { get; set; }

    public int gender { get; set; }

    public string? token { get; set; }

    public long? patient_id { get; set; }

    public string slot_time { get; set; }

    public string? country_code { get; set; }

    public string? symptoms { get; set; }

    public long? created_by_id { get; set; }

    public string tenant_name { get; set; }
}

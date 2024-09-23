using System;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus;

namespace G1.health.Shared.Hosting.Microservices.DbMigrations.Events;

[Serializable]
[EventName("Empowerm.Appointment.Created.UpdatePatient")]
public class AppointmentUpdateEto : EtoBase
{
    public long? PatientId { get; set; }

    public int Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }
}
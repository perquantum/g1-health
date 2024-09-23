using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.EventBus;

namespace G1.health.Shared.Hosting.Microservices.DbMigrations.Events
{

    [Serializable]
    [EventName("Empowerm.Appointment.Created.SendEmail")]
    public class AppointmentEmailDetails : EtoBase
    {
        public string email { get; set; }
        public string patient_name { get; set; }
        public string practitioner_name { get; set; }
        public string hospital_name { get; set; }
        public DateTime date { get; set; }
        public long time { get; set; }
        public string tenantName { get; set; }
    }
}

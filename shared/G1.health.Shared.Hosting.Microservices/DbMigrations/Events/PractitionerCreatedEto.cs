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
    [EventName("Practitioner.Created")]
    public class PractitionerCreatedEto : EtoBase
    {

        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Events.Distributed;

namespace G1.health.Shared.Hosting.Microservices.DbMigrations.Events
{
    public class ResetPasswordDetailsEto : EtoBase
    {
        public string EmailId {  get; set; }
        public string PasswordReset { get; set; }
        public string PasswordResetInfoInEmail { get; set; }
        public string ResetMyPassword { get; set; }
        public string ResetMyPasswordUrl { get; set; }
        public Guid TenantId { get; set; }
        public string TenantName { get; set; }
    }
}

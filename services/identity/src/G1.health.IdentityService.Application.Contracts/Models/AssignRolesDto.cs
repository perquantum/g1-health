using System;
using System.Collections.Generic;
using System.Text;

namespace G1.health.IdentityService.Models
{
    public class AssignRolesDto
    {
        public Guid TenantId { get; set; }
        public string[] RoleNames { get; set; }
    }
}

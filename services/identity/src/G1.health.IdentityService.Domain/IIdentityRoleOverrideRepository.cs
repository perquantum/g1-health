using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace G1.health.IdentityService
{
    public interface IIdentityRoleOverrideRepository
    {
        Task<List<string>> GetAssignableRoles(Guid tenantId);
        Task AddAdditionalRoles(Guid tenantId, string[] roles);
    }
}

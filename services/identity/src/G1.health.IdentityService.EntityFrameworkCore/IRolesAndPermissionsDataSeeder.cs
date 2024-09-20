using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G1.health.IdentityService
{
    public interface IRolesAndPermissionsDataSeeder
    {
        Task SeedRolesAsync(Guid? tenantId, string[] roleNames);
        Task SeedPermissionsAsync(Guid? tenantId, Dictionary<string, IEnumerable<string>> roleWithPermissions);
        Task<Dictionary<string, IEnumerable<string>>> GetPermissionsListByRoleNameAsync(string[] roleNames);
    }
}

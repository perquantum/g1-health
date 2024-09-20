using G1.health.IdentityService.EntityFrameworkCore;
using G1.health.IdentityService.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;

namespace G1.health.IdentityService
{

    public class EfCoreIdentityRoleOverrideRepository : EfCoreRepository<IdentityServiceDbContext, IdentityRole, Guid>, IIdentityRoleOverrideRepository
    {

        protected IRolesAndPermissionsDataSeeder RolesAndPermissionsDataSeeder { get; set; }

        protected Roles.IIdentityRoleRepository RoleRepository { get; }

        public EfCoreIdentityRoleOverrideRepository(
            IDbContextProvider<IdentityServiceDbContext> dbContextProvider,
            IRolesAndPermissionsDataSeeder rolesAndPermissionsDataSeeder,
            Roles.IIdentityRoleRepository roleRepository) : base(dbContextProvider)
        {
            RolesAndPermissionsDataSeeder = rolesAndPermissionsDataSeeder;
            RoleRepository = roleRepository;
        }

        public async Task<List<string>> GetAssignableRoles(Guid tenantId)
        {
            var dbContext = await GetDbContextAsync();
            var assignedRoles = new List<string>();
            var assignableRoles = dbContext.Roles.Where(x => (x.TenantId == null || x.TenantId == Guid.Empty) && (x.IsPublic) && (x.Name.ToLower() != "admin")).Select(x => x.Name).ToList();

            using (CurrentTenant.Change(tenantId))
            {
                assignedRoles = dbContext.Roles.Select(x => x.Name).ToList();
            }

            var result = assignableRoles.Where(x => !assignedRoles.Contains(x)).ToList();

            return result;
        }

        public async Task AddAdditionalRoles(Guid tenantId, string[] roles)
        {
            /*
            Initial Implementation : to create new Roles for a specific tenant in AbpRoles table
            Author : Paresh Vala

            var permissionsList = new Dictionary<string, IEnumerable<string>>();
            await RolesAndPermissionsDataSeeder.SeedRolesAsync(tenantId, roles);
            permissionsList = await RolesAndPermissionsDataSeeder.GetPermissionsListByRoleNameAsync(roles);
            await RolesAndPermissionsDataSeeder.SeedPermissionsAsync(tenantId, permissionsList);
            */

            // New Implementation : to create Role record in the TenantRolesAssociation table

            var dbContext = await GetDbContextAsync();
            foreach (var roleName in roles)
            {
                var role = dbContext.Roles.Where(x => x.Name == roleName).FirstOrDefault();
                var roleAssociation = new TenantRolesAssociation(GuidGenerator.Create(), role.Id, tenantId, role.IsDefault, role.IsPublic);
                await RoleRepository.CreateRoleAssociation(roleAssociation);
            }
        }
    }
}

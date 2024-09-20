using G1.health.IdentityService.DbMigrations;
using G1.health.IdentityService.EntityFrameworkCore;
using G1.health.IdentityService.Roles;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;

namespace G1.health.IdentityService.BackgroundJobs;

public class RolesAndPermissionsDataSeeder : AsyncBackgroundJob<RolesAndPermissionsDataSeeder>, IRolesAndPermissionsDataSeeder, ITransientDependency
{
    protected IPermissionManager PermissionManager { get; }
    protected IPermissionDataSeeder PermissionDataSeeder { get; set; }
    protected ICurrentTenant CurrentTenant;
    protected IIdentityServiceDbContext IdentityServiceDbContext { get; }
    protected IGuidGenerator GuidGenerator { get; set; }
    protected Roles.IdentityRoleManager RoleManager { get; }
    public Guid? TenantId { get; set; }
    public const string roleProvider = IdentityServiceDataSeederConsts.RoleProvider;
    public const string adminRoleName = IdentityServiceDataSeederConsts.AdminRoleName;

    protected IPermissionGrantRepository PermissionGrantRepository { get; }

    public RolesAndPermissionsDataSeeder(
        IPermissionManager permissionManager,
        ICurrentTenant currentTenant,
        IPermissionDataSeeder permissionDataSeeder,
        IIdentityServiceDbContext identityServiceDbContext,
        IGuidGenerator guidGenerator,
        Roles.IdentityRoleManager roleManager,
        IPermissionGrantRepository permissionGrantRepository
        )
    {
        PermissionManager = permissionManager;
        CurrentTenant = currentTenant;
        PermissionDataSeeder = permissionDataSeeder;
        IdentityServiceDbContext = identityServiceDbContext;
        GuidGenerator = guidGenerator;
        RoleManager = roleManager;
        PermissionGrantRepository = permissionGrantRepository;
    }

    public async override Task ExecuteAsync(RolesAndPermissionsDataSeeder arg)
    {
        
    }

    public async Task SeedAsync(Guid? tenantId)
    {
        string[] validRoleNames;
        var rolePermissions = new Dictionary<string, IEnumerable<string>>();

        using (CurrentTenant.Change(null))
        {
            var rolesList = IdentityServiceDbContext.Roles.ToList();
            validRoleNames = rolesList.Where(x => (x.TenantId == null || x.TenantId == Guid.Empty) && (x.IsPublic) && (x.Name.ToLower() != adminRoleName)).Select(x => x.Name).ToArray();
            rolePermissions = await GetPermissionsListByRoleNameAsync(validRoleNames);
        }

        await SeedRolesAsync(tenantId, validRoleNames);
        await SeedPermissionsAsync(tenantId, rolePermissions);
    }

    public async Task SeedRolesAsync(Guid? tenantId, string[] roleNames)
    {
        using (CurrentTenant.Change(null))
        {
            foreach (var roleName in roleNames)
            {
                /*var role = new Volo.Abp.Identity.IdentityRole(GuidGenerator.Create(), roleName, tenantId)
                {
                    IsStatic = false,
                    IsPublic = false
                };
                (await RoleManager.CreateAsync(role)).CheckErrors();*/

                var roleId = IdentityServiceDbContext.Roles.Where(x => x.Name == roleName).Select(x => x.Id).First();
                var role = new TenantRolesAssociation(GuidGenerator.Create(), roleId, tenantId, false, false);
                await IdentityServiceDbContext.TenantRolesAssociations.AddAsync(role);
                await IdentityServiceDbContext.SaveChangesAsync();
            }
        }
    }

    public async Task SeedPermissionsAsync(Guid? tenantId, Dictionary<string, IEnumerable<string>> rolePermissions)
    {
        using (CurrentTenant.Change(tenantId))
        {
            foreach (var role in rolePermissions)
            {
                await PermissionDataSeeder.SeedAsync(roleProvider, role.Key, role.Value, tenantId);
            }
        }
    }

    public async Task<Dictionary<string, IEnumerable<string>>> GetPermissionsListByRoleNameAsync(string[] roleNames)
    {
        var rolePermissions = new Dictionary<string, IEnumerable<string>>();
        using (CurrentTenant.Change(null))
        {
            foreach (var roleName in roleNames)
            {
                var permissions = await PermissionGrantRepository.GetListAsync(roleProvider, roleName);
                //var permissions = await PermissionManager.GetAllAsync(roleProvider, roleName);
                var permissionsList = permissions.Select(x => x.Name);
                rolePermissions.Add(roleName, permissionsList);
            }
        }

        return rolePermissions;
    }
}
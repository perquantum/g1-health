using System;
using System.Threading.Tasks;
using G1.health.IdentityService.BackgroundJobs;
using G1.health.Shared.Hosting.Microservices.DbMigrations.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace G1.health.IdentityService.DbMigrations;

public class IdentityServiceDataSeeder : ITransientDependency
{
    private readonly ILogger<IdentityServiceDataSeeder> _logger;
    private readonly IIdentityDataSeeder _identityDataSeeder;
    private readonly OpenIddictDataSeeder _openIddictDataSeeder;
    private readonly ICurrentTenant _currentTenant;

    protected IGuidGenerator GuidGenerator { get; }
    protected Roles.IIdentityRoleRepository RoleRepository { get; }
    protected Users.IIdentityUserRepository UserRepository { get; }
    protected ILookupNormalizer LookupNormalizer { get; }
    protected Users.IdentityUserManager UserManager { get; }
    protected Roles.IdentityRoleManager RoleManager { get; }
    protected IOptions<IdentityOptions> IdentityOptions { get; }
    protected RolesAndPermissionsDataSeeder RolesAndPermissionsDataSeeder { get; }
    protected PermissionDataSeeder PermissionDataSeeder { get; }
    protected IPermissionManager PermissionManager { get; }
    private readonly IDistributedEventBus DistributedEventBus;


    public IdentityServiceDataSeeder(
        IIdentityDataSeeder identityDataSeeder,
        OpenIddictDataSeeder openIddictDataSeeder,
        ICurrentTenant currentTenant,
        ILogger<IdentityServiceDataSeeder> logger,
        IGuidGenerator guidGenerator,
        Roles.IIdentityRoleRepository roleRepository,
        Users.IIdentityUserRepository userRepository,
        ILookupNormalizer lookupNormalizer,
        Users.IdentityUserManager userManager,
        Roles.IdentityRoleManager roleManagerm,
        IOptions<IdentityOptions> identityOptions,
        RolesAndPermissionsDataSeeder rolesAndPermissionsDataSeeder,
        PermissionDataSeeder permissionDataSeeder,
        IPermissionManager permissionManager,
        IDistributedEventBus distributedEventBus)
    {
        _identityDataSeeder = identityDataSeeder;
        _openIddictDataSeeder = openIddictDataSeeder;
        _currentTenant = currentTenant;
        _logger = logger;
        GuidGenerator = guidGenerator;
        RoleRepository = roleRepository;
        UserRepository = userRepository;
        LookupNormalizer = lookupNormalizer;
        UserManager = userManager;
        RoleManager = roleManagerm;
        IdentityOptions = identityOptions;
        RolesAndPermissionsDataSeeder = rolesAndPermissionsDataSeeder;
        PermissionDataSeeder = permissionDataSeeder;
        PermissionManager = permissionManager;
        DistributedEventBus = distributedEventBus;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation($"Seeding IdentityServer data...");
            await _openIddictDataSeeder.SeedAsync();
            _logger.LogInformation($"Seeding Identity data...");
            await _identityDataSeeder.SeedAsync(
                IdentityServiceDbProperties.DefaultAdminEmailAddress,
                IdentityServiceDbProperties.DefaultAdminPassword
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            throw;
        }
    }

    //public async Task SeedAsync(Guid? tenantId, string adminEmail, string adminPassword)
    //{
    //    try
    //    {
    //        using (_currentTenant.Change(tenantId))
    //        {
    //            if (tenantId == null)
    //            {
    //                _logger.LogInformation($"Seeding IdentityServer data...");
    //                await _openIddictDataSeeder.SeedAsync();
    //            }

    //            _logger.LogInformation($"Seeding Identity data...");
    //            await _identityDataSeeder.SeedAsync(
    //                adminEmail,
    //                adminPassword,
    //                tenantId
    //            );
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        _logger.LogError(e.Message);
    //        throw;
    //    }
    //}

    [UnitOfWork]
    public virtual async Task<IdentityDataSeedResult> SeedAsync(
    string adminEmail,
    string adminPassword,
    Guid? tenantId = null)
    {
        Check.NotNullOrWhiteSpace(adminEmail, nameof(adminEmail));
        Check.NotNullOrWhiteSpace(adminPassword, nameof(adminPassword));

        using (_currentTenant.Change(tenantId))
        {
            if (tenantId == null)
            {
                _logger.LogInformation($"Seeding IdentityServer data...");
                await _openIddictDataSeeder.SeedAsync();
            }
            await IdentityOptions.SetAsync();

            var result = new IdentityDataSeedResult();
            //"admin" user
            const string adminUserName = IdentityServiceDataSeederConsts.AdminUserName;
            var adminUser = await UserRepository.FindByNormalizedUserNameAsync(
                LookupNormalizer.NormalizeName(adminUserName)
            );

            if (adminUser != null)
            {
                return result;
            }

            var adminUserId = GuidGenerator.Create();
            adminUser = new Volo.Abp.Identity.IdentityUser(
                adminUserId,
                adminEmail,
                adminEmail,
                tenantId
            )
            {
                Name = adminUserName
            };

            (await UserManager.CreateAsync(adminUser, adminPassword, validatePassword: false)).CheckErrors();
            result.CreatedAdminUser = true;

            //"admin" role
            const string adminRoleName = IdentityServiceDataSeederConsts.AdminRoleName;

            if (tenantId == null)
            {
                var adminRole =
                    await RoleRepository.FindByNormalizedNameAsync(LookupNormalizer.NormalizeName(adminRoleName));
                if (adminRole == null)
                {
                    adminRole = new Volo.Abp.Identity.IdentityRole(
                        GuidGenerator.Create(),
                        adminRoleName,
                        tenantId
                    )
                    {
                        IsStatic = true,
                        IsPublic = true
                    };

                    (await RoleManager.CreateAsync(adminRole)).CheckErrors();
                    result.CreatedAdminRole = true;
                }
            }

            using (_currentTenant.Change(null))
            {
                (await UserManager.AddToRoleAsync(adminUser, adminRoleName)).CheckErrors();
            }

            await RolesAndPermissionsDataSeeder.SeedAsync(tenantId);

            await DistributedEventBus.PublishAsync(new UserCreatedEto
            {
                Email = adminEmail,
                Password = adminPassword,
                Username = adminUserName,
                CurrentUserId = adminUserId,
                TenantId = tenantId
            });

            return result;
        }
    }
}
﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity.Localization;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Threading;
using Volo.Abp;

namespace G1.health.IdentityService.Roles;

public class IdentityRoleManager : RoleManager<IdentityRole>, IDomainService
{
    protected override CancellationToken CancellationToken => CancellationTokenProvider.Token;

    protected IStringLocalizer<IdentityResource> Localizer { get; }
    protected ICancellationTokenProvider CancellationTokenProvider { get; }
    protected IIdentityUserRepository UserRepository { get; }
    protected IOrganizationUnitRepository OrganizationUnitRepository { get; }
    protected OrganizationUnitManager OrganizationUnitManager { get; }
    protected IDistributedCache<AbpDynamicClaimCacheItem> DynamicClaimCache { get; }
    protected IIdentityRoleRepository RoleRepository { get; }

    public IdentityRoleManager(
        IdentityRoleStore store,
        IEnumerable<IRoleValidator<IdentityRole>> roleValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        ILogger<IdentityRoleManager> logger,
        IStringLocalizer<IdentityResource> localizer,
        ICancellationTokenProvider cancellationTokenProvider,
        IIdentityUserRepository userRepository,
        IOrganizationUnitRepository organizationUnitRepository,
        OrganizationUnitManager organizationUnitManager,
        IDistributedCache<AbpDynamicClaimCacheItem> dynamicClaimCache,
        IIdentityRoleRepository roleRepository)
        : base(
              store,
              roleValidators,
              keyNormalizer,
              errors,
              logger)
    {
        Localizer = localizer;
        CancellationTokenProvider = cancellationTokenProvider;
        UserRepository = userRepository;
        OrganizationUnitRepository = organizationUnitRepository;
        OrganizationUnitManager = organizationUnitManager;
        DynamicClaimCache = dynamicClaimCache;
        RoleRepository = roleRepository;
    }

    public virtual async Task<IdentityRole> GetByIdAsync(Guid id)
    {
        var role = await Store.FindByIdAsync(id.ToString(), CancellationToken);
        if (role == null)
        {
            throw new EntityNotFoundException(typeof(IdentityRole), id);
        }

        return role;
    }

    public async override Task<IdentityResult> SetRoleNameAsync(IdentityRole role, string name)
    {
        if (role.IsStatic && role.Name != name)
        {
            throw new BusinessException(IdentityErrorCodes.StaticRoleRenaming);
        }

        var userIdList = await UserRepository.GetUserIdListByRoleIdAsync(role.Id, cancellationToken: CancellationToken);
        var result = await base.SetRoleNameAsync(role, name);
        if (result.Succeeded)
        {
            Logger.LogDebug($"Remove dynamic claims cache for users of role: {role.Id}");
            await DynamicClaimCache.RemoveManyAsync(userIdList.Select(userId => AbpDynamicClaimCacheItem.CalculateCacheKey(userId, role.TenantId)), token: CancellationToken);
        }

        return result;
    }

    public async override Task<IdentityResult> DeleteAsync(IdentityRole role)
    {
        if (role.IsStatic)
        {
            throw new BusinessException(IdentityErrorCodes.StaticRoleDeletion);
        }

        var userIdList = await UserRepository.GetUserIdListByRoleIdAsync(role.Id, cancellationToken: CancellationToken);
        var orgList = await OrganizationUnitRepository.GetListByRoleIdAsync(role.Id, includeDetails: false, cancellationToken: CancellationToken);
        var result = await base.DeleteAsync(role);
        if (result.Succeeded)
        {
            Logger.LogDebug($"Remove dynamic claims cache for users of role: {role.Id}");
            await DynamicClaimCache.RemoveManyAsync(userIdList.Select(userId => AbpDynamicClaimCacheItem.CalculateCacheKey(userId, role.TenantId)), token: CancellationToken);
            foreach (var organizationUnit in orgList)
            {
                await OrganizationUnitManager.RemoveDynamicClaimCacheAsync(organizationUnit);
            }
        }

        return result;
    }

    public override async Task<IdentityRole?> FindByNameAsync(string roleName)
    {
        return await RoleRepository.FindByNormalizedNameAsync(roleName, cancellationToken: CancellationToken);
    }
}

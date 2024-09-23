﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;
using Volo.Abp.Identity.Settings;
using Volo.Abp.Security.Claims;
using Volo.Abp.Settings;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace G1.health.IdentityService.Users;

public class MyIdentityUserManager : IdentityUserManager
{
    private readonly IServiceProvider _services;

    public MyIdentityUserManager(
        IdentityUserStore store,
        IIdentityRoleRepository roleRepository,
        Volo.Abp.Identity.IIdentityUserRepository userRepository,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<IdentityUser> passwordHasher,
        IEnumerable<IUserValidator<IdentityUser>> userValidators,
        IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators,
        ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
        IServiceProvider services, ILogger<IdentityUserManager> logger,
        ICancellationTokenProvider cancellationTokenProvider,
        IOrganizationUnitRepository organizationUnitRepository,
        ISettingProvider settingProvider,
        IDistributedEventBus distributedEventBus,
        IIdentityLinkUserRepository identityLinkUserRepository,
        IDistributedCache<AbpDynamicClaimCacheItem> dynamicClaimCache)
        : base(store, roleRepository, userRepository, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger, cancellationTokenProvider, organizationUnitRepository, settingProvider, distributedEventBus, identityLinkUserRepository, dynamicClaimCache)
    {
        _services = services;
    }

    public override async Task<IdentityResult> CreateAsync(IdentityUser user, string password, bool validatePassword)
    {
        var result = await UpdatePasswordHash(user, password, validatePassword);
        if (!result.Succeeded)
        {
            return result;
        }

        return await CreateAsync(user);
    }

    public override async Task<IdentityResult> DeleteAsync(IdentityUser user)
    {
        user.Claims.Clear();
        user.Roles.Clear();
        user.Tokens.Clear();
        user.Logins.Clear();
        user.OrganizationUnits.Clear();
        await IdentityLinkUserRepository.DeleteAsync(new IdentityLinkUserInfo(user.Id, user.TenantId), CancellationToken);
        await UpdateAsync(user);

        return await base.DeleteAsync(user);
    }

    protected override async Task<IdentityResult> UpdateUserAsync(IdentityUser user)
    {
        var result = await base.UpdateUserAsync(user);

        if (result.Succeeded)
        {
            await DynamicClaimCache.RemoveAsync(AbpDynamicClaimCacheItem.CalculateCacheKey(user.Id, user.TenantId), token: CancellationToken);
        }

        return result;
    }

    public override async Task<IdentityUser> GetByIdAsync(Guid id)
    {
        var user = await Store.FindByIdAsync(id.ToString(), CancellationToken);
        if (user == null)
        {
            throw new EntityNotFoundException(typeof(IdentityUser), id);
        }

        return user;
    }

    public override async Task<IdentityResult> SetRolesAsync([NotNull] IdentityUser user,
        [NotNull] IEnumerable<string> roleNames)
    {
        Check.NotNull(user, nameof(user));
        Check.NotNull(roleNames, nameof(roleNames));

        var currentRoleNames = await GetRolesAsync(user);

        var result = await RemoveFromRolesAsync(user, currentRoleNames.Except(roleNames).Distinct());
        if (!result.Succeeded)
        {
            return result;
        }

        result = await AddToRolesAsync(user, roleNames.Except(currentRoleNames).Distinct());
        if (!result.Succeeded)
        {
            return result;
        }

        return IdentityResult.Success;
    }

    public override async Task<bool> IsInOrganizationUnitAsync(Guid userId, Guid ouId)
    {
        var user = await UserRepository.GetAsync(userId, cancellationToken: CancellationToken);
        return user.IsInOrganizationUnit(ouId);
    }

    public override async Task<bool> IsInOrganizationUnitAsync(IdentityUser user, OrganizationUnit ou)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.OrganizationUnits,
            CancellationTokenProvider.Token);
        return user.IsInOrganizationUnit(ou.Id);
    }

    public override async Task AddToOrganizationUnitAsync(Guid userId, Guid ouId)
    {
        await AddToOrganizationUnitAsync(
            await UserRepository.GetAsync(userId, cancellationToken: CancellationToken),
            await OrganizationUnitRepository.GetAsync(ouId, cancellationToken: CancellationToken)
        );
    }

    public override async Task AddToOrganizationUnitAsync(IdentityUser user, OrganizationUnit ou)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.OrganizationUnits,
            CancellationTokenProvider.Token);

        if (user.OrganizationUnits.Any(cou => cou.OrganizationUnitId == ou.Id))
        {
            return;
        }

        await CheckMaxUserOrganizationUnitMembershipCountAsync(user.OrganizationUnits.Count + 1);

        user.AddOrganizationUnit(ou.Id);
        await UserRepository.UpdateAsync(user, cancellationToken: CancellationToken);

        await DynamicClaimCache.RemoveAsync(AbpDynamicClaimCacheItem.CalculateCacheKey(user.Id, user.TenantId), token: CancellationToken);
    }

    public override async Task RemoveFromOrganizationUnitAsync(Guid userId, Guid ouId)
    {
        var user = await UserRepository.GetAsync(userId, cancellationToken: CancellationToken);
        user.RemoveOrganizationUnit(ouId);
        await UserRepository.UpdateAsync(user, cancellationToken: CancellationToken);

        await DynamicClaimCache.RemoveAsync(AbpDynamicClaimCacheItem.CalculateCacheKey(user.Id, user.TenantId), token: CancellationToken);
    }

    public override async Task RemoveFromOrganizationUnitAsync(IdentityUser user, OrganizationUnit ou)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.OrganizationUnits,
            CancellationTokenProvider.Token);

        user.RemoveOrganizationUnit(ou.Id);
        await UserRepository.UpdateAsync(user, cancellationToken: CancellationToken);
    }

    public override async Task SetOrganizationUnitsAsync(Guid userId, params Guid[] organizationUnitIds)
    {
        await SetOrganizationUnitsAsync(
            await UserRepository.GetAsync(userId, cancellationToken: CancellationToken),
            organizationUnitIds
        );
    }

    public override async Task SetOrganizationUnitsAsync(IdentityUser user, params Guid[] organizationUnitIds)
    {
        Check.NotNull(user, nameof(user));
        Check.NotNull(organizationUnitIds, nameof(organizationUnitIds));

        await CheckMaxUserOrganizationUnitMembershipCountAsync(organizationUnitIds.Length);

        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.OrganizationUnits,
            CancellationTokenProvider.Token);

        //Remove from removed OUs
        foreach (var ouId in user.OrganizationUnits.Select(uou => uou.OrganizationUnitId).ToArray())
        {
            if (!organizationUnitIds.Contains(ouId))
            {
                user.RemoveOrganizationUnit(ouId);
            }
        }

        //Add to added OUs
        foreach (var organizationUnitId in organizationUnitIds)
        {
            if (!user.IsInOrganizationUnit(organizationUnitId))
            {
                user.AddOrganizationUnit(organizationUnitId);
            }
        }

        await UserRepository.UpdateAsync(user, cancellationToken: CancellationToken);
    }

    private async Task CheckMaxUserOrganizationUnitMembershipCountAsync(int requestedCount)
    {
        var maxCount =
            await SettingProvider.GetAsync<int>(IdentitySettingNames.OrganizationUnit.MaxUserMembershipCount);
        if (requestedCount > maxCount)
        {
            throw new BusinessException(IdentityErrorCodes.MaxAllowedOuMembership)
                .WithData("MaxUserMembershipCount", maxCount);
        }
    }

    [UnitOfWork]
    public override async Task<List<OrganizationUnit>> GetOrganizationUnitsAsync(IdentityUser user,
        bool includeDetails = false)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.OrganizationUnits,
            CancellationTokenProvider.Token);

        return await OrganizationUnitRepository.GetListAsync(
            user.OrganizationUnits.Select(t => t.OrganizationUnitId),
            includeDetails,
            cancellationToken: CancellationToken
        );
    }

    [UnitOfWork]
    public override async Task<List<IdentityUser>> GetUsersInOrganizationUnitAsync(
        OrganizationUnit organizationUnit,
        bool includeChildren = false)
    {
        if (includeChildren)
        {
            return await UserRepository
                .GetUsersInOrganizationUnitWithChildrenAsync(organizationUnit.Code, CancellationToken);
        }
        else
        {
            return await UserRepository
                .GetUsersInOrganizationUnitAsync(organizationUnit.Id, CancellationToken);
        }
    }

    public override async Task<IdentityResult> AddDefaultRolesAsync([NotNull] IdentityUser user)
    {
        await UserRepository.EnsureCollectionLoadedAsync(user, u => u.Roles, CancellationToken);

        foreach (var role in await RoleRepository.GetDefaultOnesAsync(cancellationToken: CancellationToken))
        {
            if (!user.IsInRole(role.Id))
            {
                user.AddRole(role.Id);
            }
        }

        return await UpdateUserAsync(user);
    }

    public override async Task<bool> ShouldPeriodicallyChangePasswordAsync(IdentityUser user)
    {
        Check.NotNull(user, nameof(user));

        if (user.PasswordHash.IsNullOrWhiteSpace())
        {
            return false;
        }

        var forceUsersToPeriodicallyChangePassword = await SettingProvider.GetAsync<bool>(IdentitySettingNames.Password.ForceUsersToPeriodicallyChangePassword);
        if (!forceUsersToPeriodicallyChangePassword)
        {
            return false;
        }

        var lastPasswordChangeTime = user.LastPasswordChangeTime ?? DateTime.SpecifyKind(user.CreationTime, DateTimeKind.Utc);
        var passwordChangePeriodDays = await SettingProvider.GetAsync<int>(IdentitySettingNames.Password.PasswordChangePeriodDays);

        return passwordChangePeriodDays > 0 && lastPasswordChangeTime.AddDays(passwordChangePeriodDays) < DateTime.UtcNow;
    }

    public override async Task ResetRecoveryCodesAsync(IdentityUser user)
    {
        if (!(Store is IdentityUserStore identityUserStore))
        {
            throw new AbpException($"Store is not an instance of {typeof(IdentityUserStore).AssemblyQualifiedName}");
        }

        await identityUserStore.SetTokenAsync(user, await identityUserStore.GetInternalLoginProviderAsync(), await identityUserStore.GetRecoveryCodeTokenNameAsync(), string.Empty, CancellationToken);
    }

    public override async Task<IdentityResult> SetEmailAsync(IdentityUser user, string email)
    {
        var oldMail = user.Email;

        var result = await base.SetEmailAsync(user, email);

        result.CheckErrors();

        if (!string.IsNullOrEmpty(oldMail) && !oldMail.Equals(email, StringComparison.OrdinalIgnoreCase))
        {
            await DistributedEventBus.PublishAsync(
                new IdentityUserEmailChangedEto
                {
                    Id = user.Id,
                    TenantId = user.TenantId,
                    Email = email,
                    OldEmail = oldMail
                });
        }

        return result;
    }

    public override async Task<IdentityResult> SetUserNameAsync(IdentityUser user, string userName)
    {
        var oldUserName = user.UserName;

        var result = await base.SetUserNameAsync(user, userName);

        result.CheckErrors();

        if (!string.IsNullOrEmpty(oldUserName) && oldUserName != userName)
        {
            await DistributedEventBus.PublishAsync(
                new IdentityUserUserNameChangedEto
                {
                    Id = user.Id,
                    TenantId = user.TenantId,
                    UserName = userName,
                    OldUserName = oldUserName
                });
        }

        return result;
    }

    public override async Task UpdateRoleAsync(Guid sourceRoleId, Guid? targetRoleId)
    {
        var sourceRole = await RoleRepository.GetAsync(sourceRoleId, cancellationToken: CancellationToken);

        Logger.LogDebug($"Remove dynamic claims cache for users of role: {sourceRoleId}");
        var userIdList = await UserRepository.GetUserIdListByRoleIdAsync(sourceRoleId, cancellationToken: CancellationToken);
        await DynamicClaimCache.RemoveManyAsync(userIdList.Select(userId => AbpDynamicClaimCacheItem.CalculateCacheKey(userId, sourceRole.TenantId)), token: CancellationToken);

        var targetRole = targetRoleId.HasValue ? await RoleRepository.GetAsync(targetRoleId.Value, cancellationToken: CancellationToken) : null;
        if (targetRole != null)
        {
            Logger.LogDebug($"Remove dynamic claims cache for users of role: {targetRoleId}");
            userIdList = await UserRepository.GetUserIdListByRoleIdAsync(targetRoleId.Value, cancellationToken: CancellationToken);
            await DynamicClaimCache.RemoveManyAsync(userIdList.Select(userId => AbpDynamicClaimCacheItem.CalculateCacheKey(userId, targetRole.TenantId)), token: CancellationToken);
        }

        await UserRepository.UpdateRoleAsync(sourceRoleId, targetRoleId, CancellationToken);
    }

    public override async Task UpdateOrganizationAsync(Guid sourceOrganizationId, Guid? targetOrganizationId)
    {
        var sourceOrganization = await OrganizationUnitRepository.GetAsync(sourceOrganizationId, cancellationToken: CancellationToken);

        Logger.LogDebug($"Remove dynamic claims cache for users of organization: {sourceOrganizationId}");
        var userIdList = await OrganizationUnitRepository.GetMemberIdsAsync(sourceOrganizationId, cancellationToken: CancellationToken);
        await DynamicClaimCache.RemoveManyAsync(userIdList.Select(userId => AbpDynamicClaimCacheItem.CalculateCacheKey(userId, sourceOrganization.TenantId)), token: CancellationToken);

        var targetOrganization = targetOrganizationId.HasValue ? await OrganizationUnitRepository.GetAsync(targetOrganizationId.Value, cancellationToken: CancellationToken) : null;
        if (targetOrganization != null)
        {
            Logger.LogDebug($"Remove dynamic claims cache for users of organization: {targetOrganizationId}");
            userIdList = await OrganizationUnitRepository.GetMemberIdsAsync(targetOrganizationId.Value, cancellationToken: CancellationToken);
            await DynamicClaimCache.RemoveManyAsync(userIdList.Select(userId => AbpDynamicClaimCacheItem.CalculateCacheKey(userId, targetOrganization.TenantId)), token: CancellationToken);
        }

        await UserRepository.UpdateOrganizationAsync(sourceOrganizationId, targetOrganizationId, CancellationToken);
    }

    public override async Task<bool> ValidateUserNameAsync(string userName, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(Options.User.AllowedUserNameCharacters) && userName.Any(c => !Options.User.AllowedUserNameCharacters.Contains(c)))
        {
            return false;
        }

        var owner = await FindByNameAsync(userName);
        if (owner != null && owner.Id != userId)
        {
            return false;
        }

        return true;
    }

    public override Task<string> GetRandomUserNameAsync(int length)
    {
        var allowedUserNameCharacters = Options.User.AllowedUserNameCharacters;
        if (allowedUserNameCharacters.IsNullOrWhiteSpace())
        {
            allowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        }

        var randomUserName = string.Empty;
        var random = new Random();
        while (randomUserName.Length < length)
        {
            randomUserName += allowedUserNameCharacters[random.Next(0, allowedUserNameCharacters.Length)];
        }

        return Task.FromResult(randomUserName);
    }

    public override async Task<string> GetUserNameFromEmailAsync(string email)
    {
        const int maxTryCount = 20;
        var tryCount = 0;

        var userName = email.Split('@')[0];

        if (await ValidateUserNameAsync(userName))
        {
            // The username is valid.
            return userName;
        }

        if (Options.User.AllowedUserNameCharacters.IsNullOrWhiteSpace())
        {
            // The AllowedUserNameCharacters is not set. So, we are generating a random username.
            tryCount = 0;
            do
            {
                var randomUserName = userName + RandomHelper.GetRandom(1000, 9999);
                if (await ValidateUserNameAsync(randomUserName))
                {
                    return randomUserName;
                }
                tryCount++;
            } while (tryCount < maxTryCount);
        }
        else if (!userName.All(Options.User.AllowedUserNameCharacters.Contains))
        {
            // The username contains not allowed characters. So, we are generating a random username.
            do
            {
                var randomUserName = await GetRandomUserNameAsync(userName.Length);
                if (await ValidateUserNameAsync(randomUserName))
                {
                    return randomUserName;
                }
                tryCount++;
            } while (tryCount < maxTryCount);
        }
        else if (Options.User.AllowedUserNameCharacters.Where(char.IsDigit).Distinct().Count() >= 4)
        {
            // The AllowedUserNameCharacters includes 4 numbers. So, we are generating 4 random numbers and appending to the username.
            var numbers = Options.User.AllowedUserNameCharacters.Where(char.IsDigit).OrderBy(x => Guid.NewGuid()).Take(4).ToArray();
            var minArray = numbers.OrderBy(x => x).ToArray();
            if (minArray[0] == '0')
            {
                var secondItem = minArray[1];
                minArray[0] = secondItem;
                minArray[1] = '0';
            }
            var min = int.Parse(new string(minArray));
            var max = int.Parse(new string(numbers.OrderByDescending(x => x).ToArray()));
            tryCount = 0;
            do
            {
                var randomUserName = userName + RandomHelper.GetRandom(min, max);
                if (await ValidateUserNameAsync(randomUserName))
                {
                    return randomUserName;
                }
                tryCount++;
            } while (tryCount < maxTryCount);
        }
        else
        {
            tryCount = 0;
            do
            {
                // The AllowedUserNameCharacters does not include numbers. So, we are generating 4 random characters and appending to the username.
                var randomUserName = userName + await GetRandomUserNameAsync(4);
                if (await ValidateUserNameAsync(randomUserName))
                {
                    return randomUserName;
                }
                tryCount++;
            } while (tryCount < maxTryCount);
        }

        Logger.LogError($"Could not get a valid user name for the given email address: {email}, allowed characters: {Options.User.AllowedUserNameCharacters}, tried {maxTryCount} times.");
        throw new AbpIdentityResultException(IdentityResult.Failed(new IdentityErrorDescriber().InvalidUserName(userName)));
    }

    public override async Task<IdentityUser?> FindByNameAsync(string userName)
    {
        ThrowIfDisposed();
        // ArgumentNullThrowHelper.ThrowIfNull(userName);
        userName = NormalizeName(userName);

        var user = await FindByNameAsync(userName, CancellationToken).ConfigureAwait(false);

        // Need to potentially check all keys
        if (user == null && Options.Stores.ProtectPersonalData)
        {
            var keyRing = _services.GetService<ILookupProtectorKeyRing>();
            var protector = _services.GetService<ILookupProtector>();
            if (keyRing != null && protector != null)
            {
                foreach (var key in keyRing.GetAllKeyIds())
                {
                    var oldKey = protector.Protect(key, userName);
                    user = await FindByNameAsync(oldKey, CancellationToken).ConfigureAwait(false);
                    if (user != null)
                    {
                        return user;
                    }
                }
            }
        }
        return user;
    }

    private Task<IdentityUser> FindByNameAsync([NotNull] string normalizedUserName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Check.NotNull(normalizedUserName, nameof(normalizedUserName));

        return UserRepository.FindByNormalizedUserNameAsync(normalizedUserName, includeDetails: false, cancellationToken: cancellationToken);
    }

    public async Task<IList<string>> GetRoleNamesAsync(IdentityUser user)
    {
        return await base.GetRolesAsync(user);
    }

    public override async Task<IdentityUser?> FindByIdAsync(string userId)
    {
        ThrowIfDisposed();
        return await base.FindByIdAsync(userId);
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;

namespace G1.health.IdentityService;

public class IdentityDynamicClaimsPrincipalContributorCache : ITransientDependency
{
    public ILogger<IdentityDynamicClaimsPrincipalContributorCache> Logger { get; set; }

    protected IDistributedCache<AbpDynamicClaimCacheItem> DynamicClaimCache { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected Users.IdentityUserManager UserManager { get; }
    protected IUserClaimsPrincipalFactory<IdentityUser> UserClaimsPrincipalFactory { get; }
    protected IOptions<AbpClaimsPrincipalFactoryOptions> AbpClaimsPrincipalFactoryOptions { get; }
    protected IOptions<IdentityDynamicClaimsPrincipalContributorCacheOptions> CacheOptions { get; }

    public IdentityDynamicClaimsPrincipalContributorCache(
        IDistributedCache<AbpDynamicClaimCacheItem> dynamicClaimCache,
        ICurrentTenant currentTenant,
        Users.IdentityUserManager userManager,
        IUserClaimsPrincipalFactory<IdentityUser> userClaimsPrincipalFactory,
        IOptions<AbpClaimsPrincipalFactoryOptions> abpClaimsPrincipalFactoryOptions,
        IOptions<IdentityDynamicClaimsPrincipalContributorCacheOptions> cacheOptions)
    {
        DynamicClaimCache = dynamicClaimCache;
        CurrentTenant = currentTenant;
        UserManager = userManager;
        UserClaimsPrincipalFactory = userClaimsPrincipalFactory;
        AbpClaimsPrincipalFactoryOptions = abpClaimsPrincipalFactoryOptions;
        CacheOptions = cacheOptions;

        Logger = NullLogger<IdentityDynamicClaimsPrincipalContributorCache>.Instance;
    }

    public virtual async Task<AbpDynamicClaimCacheItem> GetAsync(Guid userId, Guid? tenantId = null)
    {
        Logger.LogDebug($"Get dynamic claims cache for user: {userId}");

        if (AbpClaimsPrincipalFactoryOptions.Value.DynamicClaims.IsNullOrEmpty())
        {
            var emptyCacheItem = new AbpDynamicClaimCacheItem();
            await DynamicClaimCache.SetAsync(AbpDynamicClaimCacheItem.CalculateCacheKey(userId, tenantId), emptyCacheItem, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheOptions.Value.CacheAbsoluteExpiration
            });

            return emptyCacheItem;
        }

        return await DynamicClaimCache.GetOrAddAsync(AbpDynamicClaimCacheItem.CalculateCacheKey(userId, tenantId), async () =>
        {
            using (CurrentTenant.Change(tenantId))
            {
                Logger.LogDebug($"Filling dynamic claims cache for user 1: {userId}");

                var user = await UserManager.GetByIdAsync(userId);
                var principal = await UserClaimsPrincipalFactory.CreateAsync(user);

                var dynamicClaims = new AbpDynamicClaimCacheItem();
                foreach (var claimType in AbpClaimsPrincipalFactoryOptions.Value.DynamicClaims)
                {
                    var claims = principal.Claims.Where(x => x.Type == claimType).ToList();
                    if (claims.Any())
                    {
                        dynamicClaims.Claims.AddRange(claims.Select(claim => new AbpDynamicClaim(claimType, claim.Value)));
                    }
                    else
                    {
                        dynamicClaims.Claims.Add(new AbpDynamicClaim(claimType, null));
                    }
                }

                return dynamicClaims;
            }
        }, () => new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheOptions.Value.CacheAbsoluteExpiration
        });
    }

    public virtual async Task ClearAsync(Guid userId, Guid? tenantId = null)
    {
        Logger.LogDebug($"Remove dynamic claims cache for user: {userId}");
        await DynamicClaimCache.RemoveAsync(AbpDynamicClaimCacheItem.CalculateCacheKey(userId, tenantId));
    }
}

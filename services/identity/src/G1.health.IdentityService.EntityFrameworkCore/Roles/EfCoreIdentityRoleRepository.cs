using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using G1.health.IdentityService.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Domain.Entities;

namespace G1.health.IdentityService.Roles;

public class EfCoreIdentityRoleRepository : EfCoreRepository<IIdentityServiceDbContext, IdentityRole, Guid>, IIdentityRoleRepository
{

    public EfCoreIdentityRoleRepository(IDbContextProvider<IIdentityServiceDbContext> dbContextProvider)
        : base(dbContextProvider)
    {

    }

    public virtual async Task<IdentityRole> FindByNormalizedNameAsync(
        string normalizedRoleName,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var list = dbContext.TenantRolesAssociations.Select(x => x.RoleId).ToList();

        using (DataFilter.Disable<IMultiTenant>())
        {
            return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .Where(x => list.Contains(x.Id))
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName, GetCancellationToken(cancellationToken));
        }
    }

    public virtual async Task<List<IdentityRoleWithUserCount>> GetListWithUserCountAsync(
        string sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        string filter = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        var roles = await GetListInternalAsync(sorting, maxResultCount, skipCount, filter, includeDetails, cancellationToken: cancellationToken);

        var roleIds = roles.Select(x => x.Id).ToList();
        var userCount = await (await GetDbContextAsync()).Set<IdentityUserRole>()
            .Where(userRole => roleIds.Contains(userRole.RoleId))
            .GroupBy(userRole => userRole.RoleId)
            .Select(x => new
            {
                RoleId = x.Key,
                Count = x.Count()
            })
            .ToListAsync(GetCancellationToken(cancellationToken));

        return roles.Select(role => new IdentityRoleWithUserCount(role, userCount.FirstOrDefault(x => x.RoleId == role.Id)?.Count ?? 0)).ToList();
    }

    public virtual async Task<List<IdentityRole>> GetListAsync(
        string sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        string filter = null,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        return await GetListInternalAsync(sorting, maxResultCount, skipCount, filter, includeDetails, cancellationToken);
    }

    public virtual async Task<List<IdentityRole>> GetListAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var list = dbContext.TenantRolesAssociations.Select(x => x.RoleId).ToList();

        using (DataFilter.Disable<IMultiTenant>())
        {
            return await (await GetDbSetAsync())
            .Where(t => ids.Contains(t.Id) && list.Contains(t.Id))
            .ToListAsync(GetCancellationToken(cancellationToken));
        }
    }

    public virtual async Task<List<IdentityRole>> GetDefaultOnesAsync(
        bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var list = dbContext.TenantRolesAssociations.Select(x => x.RoleId).ToList();

        using (DataFilter.Disable<IMultiTenant>())
        {
            return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .Where(r => r.IsDefault && list.Contains(r.Id))
            .ToListAsync(GetCancellationToken(cancellationToken));
        }
    }

    public virtual async Task<long> GetCountAsync(
        string filter = null,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var list = dbContext.TenantRolesAssociations.Select(x => x.RoleId).ToList();

        using (DataFilter.Disable<IMultiTenant>())
        {
            return await (await GetDbSetAsync())
            .WhereIf(CurrentTenant.Id != null, x => list.Contains(x.Id))
            .WhereIf(!filter.IsNullOrWhiteSpace(),
                x => x.Name.Contains(filter) ||
                     x.NormalizedName.Contains(filter))
            .LongCountAsync(GetCancellationToken(cancellationToken));
        }
    }

    public virtual async Task RemoveClaimFromAllRolesAsync(string claimType, bool autoSave = false, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var roleClaims = await dbContext.Set<IdentityRoleClaim>().Where(uc => uc.ClaimType == claimType).ToListAsync(cancellationToken: cancellationToken);
        if (roleClaims.Any())
        {
            (await GetDbContextAsync()).Set<IdentityRoleClaim>().RemoveRange(roleClaims);
            if (autoSave)
            {
                await dbContext.SaveChangesAsync(GetCancellationToken(cancellationToken));
            }
        }
    }

    protected virtual async Task<List<IdentityRole>> GetListInternalAsync(
        string sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        string filter = null,
        bool includeDetails = true,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var list = dbContext.TenantRolesAssociations.Select(x => x.RoleId).ToList();

        using (DataFilter.Disable<IMultiTenant>())
        {
            return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .WhereIf(CurrentTenant.Id != null, x => list.Contains(x.Id))
            .WhereIf(!filter.IsNullOrWhiteSpace(),
                x => x.Name.Contains(filter) ||
                     x.NormalizedName.Contains(filter))
            .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof(IdentityRole.Name) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
        }
    }

    [Obsolete("Use WithDetailsAsync")]
    public override IQueryable<IdentityRole> WithDetails()
    {
        return GetQueryable().IncludeDetails();
    }

    public override async Task<IQueryable<IdentityRole>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).IncludeDetails();
    }

    public async Task CreateRoleAssociation(TenantRolesAssociation input, CancellationToken cancellationToken = default(CancellationToken))
    {
        var dbContext = await GetDbContextAsync();
        await dbContext.TenantRolesAssociations.AddAsync(input);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateRoleAssociation(TenantRolesAssociation input, CancellationToken cancellationToken = default(CancellationToken))
    {
        var dbContext = await GetDbContextAsync();
        var roles = await dbContext.TenantRolesAssociations.Where(x => x.RoleId == input.Id).ToListAsync();
        foreach (var role in roles)
        {
            role.IsDefault = input.IsDefault;
            role.IsPublic = input.IsPublic;
            dbContext.TenantRolesAssociations.Update(role);
        }
        await dbContext.SaveChangesAsync();
    }





















    public override async Task<List<IdentityRole>> GetListAsync(bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken))
    {
        var dbContext = await GetDbContextAsync();
        var list = dbContext.TenantRolesAssociations.Select(x => x.RoleId).ToList();
        var result = new List<IdentityRole>();

        using (DataFilter.Disable<IMultiTenant>())
        {
            result = await (await GetDbSetAsync())
                .IncludeDetails(includeDetails)
                .WhereIf(CurrentTenant.Id != null, x => list.Contains(x.Id)).ToListAsync(GetCancellationToken(cancellationToken));
        }
        return result;
    }

    public override async Task<long> GetCountAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        var dbContext = await GetDbContextAsync();
        var list = dbContext.TenantRolesAssociations.Select(x => x.RoleId).ToList();
        long count = 0;

        using (DataFilter.Disable<IMultiTenant>())
        {
            count = await (await GetDbSetAsync()).WhereIf(CurrentTenant.Id != null, x => list.Contains(x.Id)).LongCountAsync(GetCancellationToken(cancellationToken));
        }
        return count;
    }

    public override async Task<List<IdentityRole>> GetPagedListAsync(int skipCount, int maxResultCount, string sorting, bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken))
    {
        var dbContext = await GetDbContextAsync();
        var list = dbContext.TenantRolesAssociations.Select(x => x.RoleId).ToList();

        using (DataFilter.Disable<IMultiTenant>())
        {
            return await (await GetDbSetAsync())
            .IncludeDetails(includeDetails)
            .WhereIf(CurrentTenant.Id != null, x => list.Contains(x.Id))
            .OrderBy(sorting.IsNullOrWhiteSpace() ? nameof(IdentityRole.Name) : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
        }
    }

    public override async Task<IdentityRole> GetAsync(Guid id, bool includeDetails = true, CancellationToken cancellationToken = default(CancellationToken))
    {
        var result = await FindAsync(id, includeDetails, cancellationToken);

        if (result == null)
        {
            throw new EntityNotFoundException(typeof(IdentityRole), id);
        }
        else
        {
            return result;
        }
    }

    public override async Task<IdentityRole?> FindAsync(Guid id, bool includeDetails = true, CancellationToken cancellationToken = default(CancellationToken))
    {
        var dbContext = await GetDbContextAsync();
        var list = dbContext.TenantRolesAssociations.Select(x => x.RoleId).ToList();

        using (DataFilter.Disable<IMultiTenant>())
        {
            return await (await GetDbSetAsync())
                .IncludeDetails(includeDetails)
                .Where(x => x.Id == id && list.Contains(x.Id))
                .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
        }
    }

    /* May need to override in the future. Author: Paresh Vala
    
    public override async Task DeleteAsync(Guid id, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken)) { }

    public override async Task DeleteManyAsync(IEnumerable<Guid> ids, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken)) { }

    public override async Task<IdentityRole> InsertAsync(IdentityRole entity, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken)) { return null; }

    public override async Task InsertManyAsync(IEnumerable<IdentityRole> entities, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken)) { }

    public override async Task<IdentityRole> UpdateAsync(IdentityRole entity, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken)) { return null; }

    public override async Task UpdateManyAsync(IEnumerable<IdentityRole> entities, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken)) { }

    public override async Task DeleteAsync(IdentityRole entity, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken)) { }

    public override async Task DeleteManyAsync(IEnumerable<IdentityRole> entities, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken)) { }

    */

}

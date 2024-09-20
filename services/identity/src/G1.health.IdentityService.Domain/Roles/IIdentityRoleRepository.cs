using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace G1.health.IdentityService.Roles;

public interface IIdentityRoleRepository : IBasicRepository<IdentityRole, Guid>, IBasicRepository<IdentityRole>, IReadOnlyBasicRepository<IdentityRole>, IRepository, IReadOnlyBasicRepository<IdentityRole, Guid>
{
    Task<IdentityRole> FindByNormalizedNameAsync(string normalizedRoleName, bool includeDetails = true, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<IdentityRoleWithUserCount>> GetListWithUserCountAsync(string sorting = null, int maxResultCount = int.MaxValue, int skipCount = 0, string filter = null, bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<IdentityRole>> GetListAsync(string sorting = null, int maxResultCount = int.MaxValue, int skipCount = 0, string filter = null, bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<IdentityRole>> GetListAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<IdentityRole>> GetDefaultOnesAsync(bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken));

    Task<long> GetCountAsync(string filter = null, CancellationToken cancellationToken = default(CancellationToken));

    Task RemoveClaimFromAllRolesAsync(string claimType, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

    Task CreateRoleAssociation(TenantRolesAssociation input, CancellationToken cancellationToken = default(CancellationToken));

    Task UpdateRoleAssociation(TenantRolesAssociation input, CancellationToken cancellationToken = default(CancellationToken));
}
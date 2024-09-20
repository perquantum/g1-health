using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace G1.health.IdentityService.Users;

public interface IIdentityUserRepository : IBasicRepository<IdentityUser, Guid>, IBasicRepository<IdentityUser>, IReadOnlyBasicRepository<IdentityUser>, IRepository, IReadOnlyBasicRepository<IdentityUser, Guid>
{
    Task<IdentityUser> FindByNormalizedUserNameAsync(string normalizedUserName, bool includeDetails = true, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<string>> GetRoleNamesAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<string>> GetRoleNamesInOrganizationUnitAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));

    Task<IdentityUser> FindByLoginAsync(string loginProvider, string providerKey, bool includeDetails = true, CancellationToken cancellationToken = default(CancellationToken));

    Task<IdentityUser> FindByNormalizedEmailAsync(string normalizedEmail, bool includeDetails = true, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<IdentityUser>> GetListByClaimAsync(Claim claim, bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken));

    Task RemoveClaimFromAllUsersAsync(string claimType, bool autoSave = false, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<IdentityUser>> GetListByNormalizedRoleNameAsync(string normalizedRoleName, bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<Guid>> GetUserIdListByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<IdentityUser>> GetListAsync(string sorting = null, int maxResultCount = int.MaxValue, int skipCount = 0, string filter = null, bool includeDetails = false, Guid? roleId = null, Guid? organizationUnitId = null, string userName = null, string phoneNumber = null, string emailAddress = null, string name = null, string surname = null, bool? isLockedOut = null, bool? notActive = null, bool? emailConfirmed = null, bool? isExternal = null, DateTime? maxCreationTime = null, DateTime? minCreationTime = null, DateTime? maxModifitionTime = null, DateTime? minModifitionTime = null, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<IdentityRole>> GetRolesAsync(Guid id, bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<Volo.Abp.Identity.OrganizationUnit>> GetOrganizationUnitsAsync(Guid id, bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<IdentityUser>> GetUsersInOrganizationUnitAsync(Guid organizationUnitId, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<IdentityUser>> GetUsersInOrganizationsListAsync(List<Guid> organizationUnitIds, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<IdentityUser>> GetUsersInOrganizationUnitWithChildrenAsync(string code, CancellationToken cancellationToken = default(CancellationToken));

    Task<long> GetCountAsync(string filter = null, Guid? roleId = null, Guid? organizationUnitId = null, string userName = null, string phoneNumber = null, string emailAddress = null, string name = null, string surname = null, bool? isLockedOut = null, bool? notActive = null, bool? emailConfirmed = null, bool? isExternal = null, DateTime? maxCreationTime = null, DateTime? minCreationTime = null, DateTime? maxModifitionTime = null, DateTime? minModifitionTime = null, CancellationToken cancellationToken = default(CancellationToken));

    Task<IdentityUser> FindByTenantIdAndUserNameAsync(string userName, Guid? tenantId, bool includeDetails = true, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<IdentityUser>> GetListByIdsAsync(IEnumerable<Guid> ids, bool includeDetails = false, CancellationToken cancellationToken = default(CancellationToken));

    Task UpdateRoleAsync(Guid sourceRoleId, Guid? targetRoleId, CancellationToken cancellationToken = default(CancellationToken));

    Task UpdateOrganizationAsync(Guid sourceOrganizationId, Guid? targetOrganizationId, CancellationToken cancellationToken = default(CancellationToken));

    Task<List<IdentityUserIdWithRoleNames>> GetRoleNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default(CancellationToken));

    Task<IList<string>> GetRoleNamesAsync(IdentityUser user, CancellationToken cancellationToken = default(CancellationToken));

    Task<IdentityUser> FindByIdAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));
}
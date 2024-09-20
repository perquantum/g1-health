using G1.health.IdentityService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace G1.health.IdentityService.Roles;

public interface IIdentityRoleAppService : ICrudAppService<
    IdentityRoleDto,
    Guid,
    GetIdentityRoleListInput,
    IdentityRoleCreateDto,
    IdentityRoleUpdateDto>
{
    Task<ListResultDto<IdentityRoleDto>> GetAllListAsync();

    Task UpdateClaimsAsync(Guid id, List<IdentityRoleClaimDto> input);

    Task<List<ClaimTypeDto>> GetAllClaimTypesAsync();

    Task<List<IdentityRoleClaimDto>> GetClaimsAsync(Guid id);

    Task MoveAllUsersAsync(Guid id, Guid? targetRoleId);

    Task<List<string>> GetAssignableRoles(Guid tenantId);

    Task AddAdditionalRoles(AssignRolesDto input);
}

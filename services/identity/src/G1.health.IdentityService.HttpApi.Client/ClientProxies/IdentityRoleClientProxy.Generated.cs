using G1.health.IdentityService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client.ClientProxying;
using Volo.Abp.Identity;

namespace G1.health.IdentityService.ClientProxies;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(G1.health.IdentityService.Roles.IIdentityRoleAppService), typeof(IdentityRoleClientProxy))]
public partial class IdentityRoleClientProxy : ClientProxyBase<G1.health.IdentityService.Roles.IIdentityRoleAppService>, G1.health.IdentityService.Roles.IIdentityRoleAppService
{
    public virtual async Task<IdentityRoleDto> GetAsync(Guid id)
    {
        return await RequestAsync<IdentityRoleDto>(nameof(GetAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id }
        });
    }

    public virtual async Task<IdentityRoleDto> CreateAsync(IdentityRoleCreateDto input)
    {
        return await RequestAsync<IdentityRoleDto>(nameof(CreateAsync), new ClientProxyRequestTypeValue
        {
            { typeof(IdentityRoleCreateDto), input }
        });
    }

    public virtual async Task<IdentityRoleDto> UpdateAsync(Guid id, IdentityRoleUpdateDto input)
    {
        return await RequestAsync<IdentityRoleDto>(nameof(UpdateAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id },
            { typeof(IdentityRoleUpdateDto), input }
        });
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        await RequestAsync(nameof(DeleteAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id }
        });
    }

    public virtual async Task<ListResultDto<IdentityRoleDto>> GetAllListAsync()
    {
        return await RequestAsync<ListResultDto<IdentityRoleDto>>(nameof(GetAllListAsync));
    }

    public virtual async Task<PagedResultDto<IdentityRoleDto>> GetListAsync(GetIdentityRoleListInput input)
    {
        return await RequestAsync<PagedResultDto<IdentityRoleDto>>(nameof(GetListAsync), new ClientProxyRequestTypeValue
        {
            { typeof(GetIdentityRoleListInput), input }
        });
    }

    public virtual async Task UpdateClaimsAsync(Guid id, List<IdentityRoleClaimDto> input)
    {
        await RequestAsync(nameof(UpdateClaimsAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id },
            { typeof(List<IdentityRoleClaimDto>), input }
        });
    }

    public virtual async Task<List<IdentityRoleClaimDto>> GetClaimsAsync(Guid id)
    {
        return await RequestAsync<List<IdentityRoleClaimDto>>(nameof(GetClaimsAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id }
        });
    }

    public virtual async Task MoveAllUsersAsync(Guid id, Guid? roleId)
    {
        await RequestAsync(nameof(MoveAllUsersAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id },
            { typeof(Guid?), roleId }
        });
    }

    public virtual async Task<List<ClaimTypeDto>> GetAllClaimTypesAsync()
    {
        return await RequestAsync<List<ClaimTypeDto>>(nameof(GetAllClaimTypesAsync));
    }

    public virtual async Task<List<string>> GetAssignableRoles(Guid tenantId)
    {
        return await RequestAsync<List<string>>(nameof(GetAssignableRoles), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), tenantId }
        });
    }

    public virtual async Task AddAdditionalRoles(AssignRolesDto input)
    {
        await RequestAsync<List<string>>(nameof(AddAdditionalRoles), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), input }
        });
    }
}

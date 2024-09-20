using Asp.Versioning;
using G1.health.IdentityService.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Controllers;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Identity;
using IIdentityRoleAppService = G1.health.IdentityService.Roles.IIdentityRoleAppService;

namespace G1.health.IdentityService.Controllers;

[ReplaceControllers(typeof(Volo.Abp.Identity.IdentityRoleController))]
[RemoteService(Name = IdentityProRemoteServiceConsts.RemoteServiceName)]
[Area(IdentityProRemoteServiceConsts.ModuleName)]
[ControllerName("Role")]
[Route("api/identity/roles")]
public class IdentityRoleController : AbpControllerBase, IIdentityRoleAppService
{
    protected IIdentityRoleAppService RoleAppService { get; }

    public IdentityRoleController(IIdentityRoleAppService roleAppService)
    {
        RoleAppService = roleAppService;
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<IdentityRoleDto> GetAsync(Guid id)
    {
        return RoleAppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<IdentityRoleDto> CreateAsync(IdentityRoleCreateDto input)
    {
        return RoleAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    public virtual Task<IdentityRoleDto> UpdateAsync(Guid id, IdentityRoleUpdateDto input)
    {
        return RoleAppService.UpdateAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return RoleAppService.DeleteAsync(id);
    }

    [HttpGet]
    [Route("all")]
    public virtual Task<ListResultDto<IdentityRoleDto>> GetAllListAsync()
    {
        return RoleAppService.GetAllListAsync();
    }

    [HttpGet]
    [Route("")]
    public virtual Task<PagedResultDto<IdentityRoleDto>> GetListAsync(GetIdentityRoleListInput input)
    {
        return RoleAppService.GetListAsync(input);
    }

    [HttpPut]
    [Route("{id}/claims")]
    public virtual Task UpdateClaimsAsync(Guid id, List<IdentityRoleClaimDto> input)
    {
        return RoleAppService.UpdateClaimsAsync(id, input);
    }

    [HttpGet]
    [Route("{id}/claims")]
    public virtual Task<List<IdentityRoleClaimDto>> GetClaimsAsync(Guid id)
    {
        return RoleAppService.GetClaimsAsync(id);
    }

    [HttpPut]
    [Route("{id}/move-all-users")]
    public virtual Task MoveAllUsersAsync(Guid id, [FromQuery] Guid? roleId)
    {
        return RoleAppService.MoveAllUsersAsync(id, roleId);
    }

    [HttpGet]
    [Route("all-claim-types")]
    public virtual Task<List<ClaimTypeDto>> GetAllClaimTypesAsync()
    {
        return RoleAppService.GetAllClaimTypesAsync();
    }

    [HttpGet]
    [Route("assignable-roles/{tenantId}")]
    public virtual Task<List<string>> GetAssignableRoles(Guid tenantId)
    {
        return RoleAppService.GetAssignableRoles(tenantId);
    }

    [HttpPost]
    [Route("assign-roles")]
    public virtual Task AddAdditionalRoles(AssignRolesDto input)
    {
        return RoleAppService.AddAdditionalRoles(input);
    }
}

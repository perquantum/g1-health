﻿using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Identity;
using G1.health.IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;
using Volo.Abp.ObjectExtending;
using Volo.Abp;
using IdentityRole = Volo.Abp.Identity.IdentityRole;
using G1.health.IdentityService.Users;

namespace G1.health.IdentityService.Roles;

[Authorize(IdentityPermissions.Roles.Default)]
public class IdentityRoleAppService : IdentityAppServiceBase, IIdentityRoleAppService
{
    protected IdentityRoleManager RoleManager { get; }
    protected IIdentityRoleRepository RoleRepository { get; }
    protected IIdentityClaimTypeRepository IdentityClaimTypeRepository { get; }
    protected Users.IIdentityUserRepository UserRepository { get; }
    protected MyIdentityUserManager UserManager { get; }
    protected IIdentityRoleOverrideRepository RoleOverrideRepository { get; }

    public IdentityRoleAppService(
        IdentityRoleManager roleManager,
        IIdentityRoleRepository roleRepository,
        IIdentityClaimTypeRepository identityClaimTypeRepository,
        Users.IIdentityUserRepository userRepository,
        MyIdentityUserManager userManager,
        IIdentityRoleOverrideRepository roleOverrideRepository)
    {
        RoleManager = roleManager;
        RoleRepository = roleRepository;
        IdentityClaimTypeRepository = identityClaimTypeRepository;
        UserRepository = userRepository;
        UserManager = userManager;
        RoleOverrideRepository = roleOverrideRepository;
    }

    public virtual async Task<IdentityRoleDto> GetAsync(Guid id)
    {
        var dto = ObjectMapper.Map<IdentityRole, IdentityRoleDto>(await RoleManager.GetByIdAsync(id));
        dto.UserCount = await UserRepository.GetCountAsync(roleId: id);
        return dto;
    }

    public virtual async Task<ListResultDto<IdentityRoleDto>> GetAllListAsync()
    {
        var list = await RoleRepository.GetListWithUserCountAsync();

        var roleDtos = new ListResultDto<IdentityRoleDto>(ObjectMapper.Map<List<IdentityRole>, List<IdentityRoleDto>>(list.Select(x => x.Role).ToList()));

        foreach (var roleDto in roleDtos.Items)
        {
            roleDto.UserCount = list.First(x => x.Role.Id == roleDto.Id).UserCount;
        }
        return roleDtos;
    }

    public virtual async Task<PagedResultDto<IdentityRoleDto>> GetListAsync(GetIdentityRoleListInput input)
    {
        var list = await RoleRepository.GetListWithUserCountAsync(input.Sorting, input.MaxResultCount, input.SkipCount, filter: input.Filter);
        var totalCount = await RoleRepository.GetCountAsync(input.Filter);

        var roleDtos = new PagedResultDto<IdentityRoleDto>(
            totalCount,
            ObjectMapper.Map<List<IdentityRole>, List<IdentityRoleDto>>(list.Select(x => x.Role).ToList())
        );

        foreach (var roleDto in roleDtos.Items)
        {
            roleDto.UserCount = list.First(x => x.Role.Id == roleDto.Id).UserCount;
        }

        return roleDtos;
    }

    [Authorize(IdentityPermissions.Roles.Update)]
    public virtual async Task UpdateClaimsAsync(Guid id, List<IdentityRoleClaimDto> input)
    {
        var role = await RoleRepository.GetAsync(id);

        foreach (var claim in input)
        {
            var existing = role.FindClaim(new Claim(claim.ClaimType, claim.ClaimValue));
            if (existing == null)
            {
                role.AddClaim(GuidGenerator, new Claim(claim.ClaimType, claim.ClaimValue));
            }
        }

        //Copied with ToList to avoid modification of the collection in the for loop
        foreach (var claim in role.Claims.ToList())
        {
            if (!input.Any(c => claim.ClaimType == c.ClaimType && claim.ClaimValue == c.ClaimValue))
            {
                role.RemoveClaim(new Claim(claim.ClaimType, claim.ClaimValue));
            }
        }

        if (role.Claims.Count != 0)
        {
            var claimNames = role.Claims.Select(x => x.ClaimType);
            var claims = await IdentityClaimTypeRepository.GetListByNamesAsync(claimNames);

            foreach (var claim in claims.Where(x => x.ValueType == IdentityClaimValueType.String))
            {
                var roleClaim = role.Claims.FirstOrDefault(x => x.ClaimType == claim.Name);
                if (roleClaim == null)
                {
                    continue;
                }

                if (claim.Required && roleClaim.ClaimValue.IsNullOrWhiteSpace())
                {
                    throw new UserFriendlyException(L["ClaimValueCanNotBeBlank"]);
                }
                if (!claim.Regex.IsNullOrWhiteSpace() && !Regex.IsMatch(roleClaim.ClaimValue, claim.Regex, RegexOptions.None, TimeSpan.FromSeconds(1)))
                {
                    throw new UserFriendlyException(L["ClaimValueIsInvalid", claim.Name]);
                }
            }
        }

        await RoleRepository.UpdateAsync(role);
    }

    [Authorize(IdentityPermissions.Roles.Default)]
    public virtual async Task<List<ClaimTypeDto>> GetAllClaimTypesAsync()
    {
        var claimTypes = await IdentityClaimTypeRepository.GetListAsync();

        var dtos = ObjectMapper.Map<List<IdentityClaimType>, List<ClaimTypeDto>>(claimTypes);
        foreach (var dto in dtos)
        {
            dto.ValueTypeAsString = dto.ValueType.ToString();
        }

        return dtos;
    }

    [Authorize(IdentityPermissions.Roles.Default)]
    public virtual async Task<List<IdentityRoleClaimDto>> GetClaimsAsync(Guid id)
    {
        var role = await RoleRepository.GetAsync(id);
        return new List<IdentityRoleClaimDto>(
            ObjectMapper.Map<List<IdentityRoleClaim>, List<IdentityRoleClaimDto>>(role.Claims.ToList())
        );
    }

    [Authorize(IdentityPermissions.Roles.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var role = await RoleManager.GetByIdAsync(id);
        await UserManager.UpdateRoleAsync(id, null);
        await RoleManager.DeleteAsync(role);
    }

    [Authorize(IdentityPermissions.Roles.Update)]
    public virtual async Task MoveAllUsersAsync(Guid id, Guid? targetRoleId)
    {
        var role = await RoleManager.GetByIdAsync(id);
        await UserManager.UpdateRoleAsync(role.Id, targetRoleId);
    }

    [Authorize(IdentityPermissions.Roles.Create)]
    public virtual async Task<IdentityRoleDto> CreateAsync(IdentityRoleCreateDto input)
    {
        var role = new IdentityRole(
            GuidGenerator.Create(),
            input.Name,
            CurrentTenant.Id
        )
        {
            IsDefault = input.IsDefault,
            IsPublic = input.IsPublic
        };

        input.MapExtraPropertiesTo(role);

        (await RoleManager.CreateAsync(role)).CheckErrors();

        var roleAssociation = new TenantRolesAssociation(GuidGenerator.Create(), role.Id, CurrentTenant.Id, input.IsDefault, input.IsPublic);
        await RoleRepository.CreateRoleAssociation(roleAssociation);

        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<IdentityRole, IdentityRoleDto>(role);
    }

    [Authorize(IdentityPermissions.Roles.Update)]
    public virtual async Task<IdentityRoleDto> UpdateAsync(Guid id, IdentityRoleUpdateDto input)
    {
        var role = await RoleManager.GetByIdAsync(id);
        role.SetConcurrencyStampIfNotNull(input.ConcurrencyStamp);

        (await RoleManager.SetRoleNameAsync(role, input.Name)).CheckErrors();

        role.IsDefault = input.IsDefault;
        role.IsPublic = input.IsPublic;

        input.MapExtraPropertiesTo(role);

        (await RoleManager.UpdateAsync(role)).CheckErrors();
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<IdentityRole, IdentityRoleDto>(role);
    }

    public virtual Task<List<string>> GetAssignableRoles(Guid tenantId)
    {
        return RoleOverrideRepository.GetAssignableRoles(tenantId);
    }

    [Authorize(IdentityPermissions.Roles.Create)]
    public virtual Task AddAdditionalRoles(AssignRolesDto input)
    {
        return RoleOverrideRepository.AddAdditionalRoles(input.TenantId, input.RoleNames);
    }
}

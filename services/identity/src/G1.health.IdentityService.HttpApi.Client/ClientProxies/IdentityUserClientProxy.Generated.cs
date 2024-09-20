using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client.ClientProxying;
using Volo.Abp.Identity;

namespace G1.health.IdentityService.ClientProxies;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(G1.health.IdentityService.Users.IIdentityUserAppService), typeof(IdentityUserClientProxy))]
public partial class IdentityUserClientProxy : ClientProxyBase<G1.health.IdentityService.Users.IIdentityUserAppService>, G1.health.IdentityService.Users.IIdentityUserAppService
{
    public virtual async Task<IdentityUserDto> GetAsync(Guid id)
    {
        return await RequestAsync<IdentityUserDto>(nameof(GetAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id }
        });
    }

    public virtual async Task<PagedResultDto<IdentityUserDto>> GetListAsync(GetIdentityUsersInput input)
    {
        return await RequestAsync<PagedResultDto<IdentityUserDto>>(nameof(GetListAsync), new ClientProxyRequestTypeValue
        {
            { typeof(GetIdentityUsersInput), input }
        });
    }

    public virtual async Task<IdentityUserDto> CreateAsync(IdentityUserCreateDto input)
    {
        return await RequestAsync<IdentityUserDto>(nameof(CreateAsync), new ClientProxyRequestTypeValue
        {
            { typeof(IdentityUserCreateDto), input }
        });
    }

    public virtual async Task<IdentityUserDto> UpdateAsync(Guid id, IdentityUserUpdateDto input)
    {
        return await RequestAsync<IdentityUserDto>(nameof(UpdateAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id },
            { typeof(IdentityUserUpdateDto), input }
        });
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        await RequestAsync(nameof(DeleteAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id }
        });
    }

    public virtual async Task<IdentityUserDto> FindByIdAsync(Guid id)
    {
        return await RequestAsync<IdentityUserDto>(nameof(FindByIdAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id }
        });
    }

    public virtual async Task<ListResultDto<IdentityRoleDto>> GetRolesAsync(Guid id)
    {
        return await RequestAsync<ListResultDto<IdentityRoleDto>>(nameof(GetRolesAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id }
        });
    }

    public virtual async Task<ListResultDto<IdentityRoleDto>> GetAssignableRolesAsync()
    {
        return await RequestAsync<ListResultDto<IdentityRoleDto>>(nameof(GetAssignableRolesAsync));
    }

    public virtual async Task<ListResultDto<OrganizationUnitWithDetailsDto>> GetAvailableOrganizationUnitsAsync()
    {
        return await RequestAsync<ListResultDto<OrganizationUnitWithDetailsDto>>(nameof(GetAvailableOrganizationUnitsAsync));
    }

    public virtual async Task<List<ClaimTypeDto>> GetAllClaimTypesAsync()
    {
        return await RequestAsync<List<ClaimTypeDto>>(nameof(GetAllClaimTypesAsync));
    }

    public virtual async Task<List<IdentityUserClaimDto>> GetClaimsAsync(Guid id)
    {
        return await RequestAsync<List<IdentityUserClaimDto>>(nameof(GetClaimsAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id }
        });
    }

    public virtual async Task<List<OrganizationUnitDto>> GetOrganizationUnitsAsync(Guid id)
    {
        return await RequestAsync<List<OrganizationUnitDto>>(nameof(GetOrganizationUnitsAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id }
        });
    }

    public virtual async Task UpdateRolesAsync(Guid id, IdentityUserUpdateRolesDto input)
    {
        await RequestAsync(nameof(UpdateRolesAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id },
            { typeof(IdentityUserUpdateRolesDto), input }
        });
    }

    public virtual async Task UpdateClaimsAsync(Guid id, List<IdentityUserClaimDto> input)
    {
        await RequestAsync(nameof(UpdateClaimsAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id },
            { typeof(List<IdentityUserClaimDto>), input }
        });
    }

    public virtual async Task LockAsync(Guid id, DateTime lockoutEnd)
    {
        await RequestAsync(nameof(LockAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id },
            { typeof(DateTime), lockoutEnd }
        });
    }

    public virtual async Task UnlockAsync(Guid id)
    {
        await RequestAsync(nameof(UnlockAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id }
        });
    }

    public virtual async Task<IdentityUserDto> FindByUsernameAsync(string username)
    {
        return await RequestAsync<IdentityUserDto>(nameof(FindByUsernameAsync), new ClientProxyRequestTypeValue
        {
            { typeof(string), username }
        });
    }

    public virtual async Task<IdentityUserDto> FindByEmailAsync(string email)
    {
        return await RequestAsync<IdentityUserDto>(nameof(FindByEmailAsync), new ClientProxyRequestTypeValue
        {
            { typeof(string), email }
        });
    }

    public virtual async Task<bool> GetTwoFactorEnabledAsync(Guid id)
    {
        return await RequestAsync<bool>(nameof(GetTwoFactorEnabledAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id }
        });
    }

    public virtual async Task SetTwoFactorEnabledAsync(Guid id, bool enabled)
    {
        await RequestAsync(nameof(SetTwoFactorEnabledAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id },
            { typeof(bool), enabled }
        });
    }

    public virtual async Task UpdatePasswordAsync(Guid id, IdentityUserUpdatePasswordInput input)
    {
        await RequestAsync(nameof(UpdatePasswordAsync), new ClientProxyRequestTypeValue
        {
            { typeof(Guid), id },
            { typeof(IdentityUserUpdatePasswordInput), input }
        });
    }

    public virtual async Task<List<IdentityRoleLookupDto>> GetRoleLookupAsync()
    {
        return await RequestAsync<List<IdentityRoleLookupDto>>(nameof(GetRoleLookupAsync));
    }

    public virtual async Task<List<OrganizationUnitLookupDto>> GetOrganizationUnitLookupAsync()
    {
        return await RequestAsync<List<OrganizationUnitLookupDto>>(nameof(GetOrganizationUnitLookupAsync));
    }

    public virtual async Task<List<ExternalLoginProviderDto>> GetExternalLoginProvidersAsync()
    {
        return await RequestAsync<List<ExternalLoginProviderDto>>(nameof(GetExternalLoginProvidersAsync));
    }

    public virtual async Task<IdentityUserDto> ImportExternalUserAsync(ImportExternalUserInput input)
    {
        return await RequestAsync<IdentityUserDto>(nameof(ImportExternalUserAsync), new ClientProxyRequestTypeValue
        {
            { typeof(ImportExternalUserInput), input }
        });
    }

    public virtual async Task<IRemoteStreamContent> GetListAsExcelFileAsync(GetIdentityUserListAsFileInput input)
    {
        return await RequestAsync<IRemoteStreamContent>(nameof(GetListAsExcelFileAsync), new ClientProxyRequestTypeValue
        {
            { typeof(GetIdentityUserListAsFileInput), input }
        });
    }

    public virtual async Task<IRemoteStreamContent> GetListAsCsvFileAsync(GetIdentityUserListAsFileInput input)
    {
        return await RequestAsync<IRemoteStreamContent>(nameof(GetListAsCsvFileAsync), new ClientProxyRequestTypeValue
        {
            { typeof(GetIdentityUserListAsFileInput), input }
        });
    }

    public virtual async Task<DownloadTokenResultDto> GetDownloadTokenAsync()
    {
        return await RequestAsync<DownloadTokenResultDto>(nameof(GetDownloadTokenAsync));
    }

    public virtual async Task<IRemoteStreamContent> GetImportUsersSampleFileAsync(GetImportUsersSampleFileInput input)
    {
        return await RequestAsync<IRemoteStreamContent>(nameof(GetImportUsersSampleFileAsync), new ClientProxyRequestTypeValue
        {
            { typeof(GetImportUsersSampleFileInput), input }
        });
    }

    public virtual async Task<ImportUsersFromFileOutput> ImportUsersFromFileAsync(ImportUsersFromFileInputWithStream input)
    {
        return await RequestAsync<ImportUsersFromFileOutput>(nameof(ImportUsersFromFileAsync), new ClientProxyRequestTypeValue
        {
            { typeof(ImportUsersFromFileInputWithStream), input }
        });
    }

    public virtual async Task<IRemoteStreamContent> GetImportInvalidUsersFileAsync(GetImportInvalidUsersFileInput input)
    {
        return await RequestAsync<IRemoteStreamContent>(nameof(GetImportInvalidUsersFileAsync), new ClientProxyRequestTypeValue
        {
            { typeof(GetImportInvalidUsersFileInput), input }
        });
    }
}

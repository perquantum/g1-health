using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http.Client.ClientProxying;
using Volo.Saas.Host.Dtos;

namespace G1.health.SaasService.ClientProxies;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ITenantOverrideAppService), typeof(TenantOverrideClientProxy))]
public partial class TenantOverrideClientProxy : ClientProxyBase<ITenantOverrideAppService>, ITenantOverrideAppService
{

    public virtual async Task<TenantDto> GetTenantIdByName(string input)
    {
        return await RequestAsync<TenantDto>(nameof(GetTenantIdByName), new ClientProxyRequestTypeValue
        {
            { typeof(string), input }
        });
    }

    public virtual async Task<List<TenantDto>> GetTenantNamesList()
    {
        return await RequestAsync<List<TenantDto>>(nameof(GetTenantNamesList));
    }

    public virtual async Task<SaasTenantDto> CreateTenant(SaasTenantCreateDto input)
    {
        return await RequestAsync<SaasTenantDto>(nameof(CreateTenant), new ClientProxyRequestTypeValue
        {
            { typeof(SaasTenantCreateDto), input }
        });
    }
}

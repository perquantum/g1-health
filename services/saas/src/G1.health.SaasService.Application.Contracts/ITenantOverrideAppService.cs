using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Saas.Host.Dtos;

namespace G1.health.SaasService
{
    public interface ITenantOverrideAppService : IApplicationService
    {
        Task<TenantDto> GetTenantIdByName(string name);
        Task<List<TenantDto>> GetTenantNamesList();
        Task<SaasTenantDto> CreateTenant(SaasTenantCreateDto input);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Saas.Tenants;

namespace G1.health.SaasService
{
    public interface ITenantOverrideRepository
    {
        Task<Tenant> GetTenantIdByName(string name);
        Task<List<Tenant>> GetTenantNamesList();
    }
}

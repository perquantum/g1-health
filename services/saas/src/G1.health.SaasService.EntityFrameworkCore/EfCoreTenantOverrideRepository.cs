using G1.health.SaasService.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Saas.Tenants;

namespace G1.health.SaasService
{
    public class EfCoreTenantOverrideRepository : EfCoreRepository<SaasServiceDbContext, Tenant, Guid>, ITenantOverrideRepository
    {
        public EfCoreTenantOverrideRepository(IDbContextProvider<SaasServiceDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }

        public async Task<Tenant> GetTenantIdByName(string name)
        {
            var dbContext = await GetDbContextAsync();
            var result = dbContext.Tenants.Where(x => x.IsDeleted == false && x.Name == name).Select(x => x).FirstOrDefault();
            return result;
        }

        public async Task<List<Tenant>> GetTenantNamesList()
        {
            var dbContext = await GetDbContextAsync();
            var result = dbContext.Tenants.Where(x => !x.IsDeleted).Select(x => x).ToList();
            return result;
        }
    }
}

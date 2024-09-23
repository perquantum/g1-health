using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Saas.Host;
using Volo.Saas.Host.Dtos;

namespace G1.health.SaasService.Controllers
{
    [RemoteService(Name = SaasServiceRemoteServiceConsts.RemoteServiceName)]
    [Area(SaasHostRemoteServiceConsts.ModuleName)]
    //[Controller("Tenant")]
    [Route("/api/saas/tenants")]
    public class TenantOverrideController : AbpController, ITenantOverrideAppService
    {

        private readonly ITenantOverrideAppService Service;

        public TenantOverrideController(ITenantOverrideAppService service)
        {
            Service = service;
        }

        [HttpGet]
        [Route("names/{name}")]
        public virtual Task<TenantDto> GetTenantIdByName(string name)
        {
            return Service.GetTenantIdByName(name);
        }

        [HttpGet]
        [Route("names/all")]
        public virtual Task<List<TenantDto>> GetTenantNamesList()
        {
            return Service.GetTenantNamesList();
        }

        [HttpPost]
        [Route("create")]
        public virtual Task<SaasTenantDto> CreateTenant(SaasTenantCreateDto input)
        {
            return Service.CreateTenant(input);
        }
    }
}

using G1.health.SaasService.Application;
using Volo.Abp.Modularity;

namespace G1.health.SaasService;

[DependsOn(
    typeof(SaasServiceApplicationModule),
    typeof(SaasServiceDomainTestModule)
    )]
public class SaasServiceApplicationTestModule : AbpModule
{

}

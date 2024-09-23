using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;
using Volo.Abp.Data;

namespace G1.health.AdministrationService;

public class AdditionalApplicationConfigurationContributor : IApplicationConfigurationContributor
{

    public async Task ContributeAsync(ApplicationConfigurationContributorContext context)
    {
        var formId = 0;
        context.ApplicationConfiguration.SetProperty("registrationFormId", formId);
    }
}

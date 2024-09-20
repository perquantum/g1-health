using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace G1.health.AuthServer;

[Dependency(ReplaceServices = true)]
public class healthBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "health";
}

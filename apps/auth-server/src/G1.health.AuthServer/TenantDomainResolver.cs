using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Text.Formatting;

namespace G1.health.AuthServer;

public class TenantDomainResolver : TenantResolveContributorBase
{
    public const string ContributorName = "Custom";

    public override string Name => ContributorName;

    private static readonly string[] ProtocolPrefixes = { "http://", "https://" };

    private readonly string DomainFormat;

    private readonly string Environment;

    public TenantDomainResolver(string domainFormat, string environment)
    {
        DomainFormat = domainFormat;
        Environment = environment;
    }
    public override async Task ResolveAsync(ITenantResolveContext context)
    {
        var httpContext = context.GetHttpContext();

        var referer = httpContext.Request.Headers["Referer"].ToString();

        if (string.IsNullOrEmpty(referer))
        {
            return;
        }

        referer = referer.RemovePreFix(ProtocolPrefixes);
        var extractResult = FormattedStringValueExtracter.Extract(referer, DomainFormat, ignoreCase: true);
        if (extractResult != null && extractResult.IsMatch)
        {
            string subdomain = extractResult.Matches[0].Value;
            if (subdomain != Environment)
            {
                if (Environment.IsNullOrEmpty() || subdomain.EndsWith(Environment))
                {
                    context.Handled = true;
                    int lastIndex = subdomain.LastIndexOf(Environment);
                    context.TenantIdOrName = subdomain.Substring(0, lastIndex);
                }
            }
        }

    }
}
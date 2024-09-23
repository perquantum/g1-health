using G1.health.SaasService.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace G1.health.SaasService.Permissions;

public class SaasServicePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(SaasServicePermissions.GroupName, L("Permission:SaasService"));

        //Define your own permissions here. Example:
        //myGroup.AddPermission(BookStorePermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<SaasServiceResource>(name);
    }
}

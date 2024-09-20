using System.Collections.Generic;

namespace G1.health.AdministrationService.DbMigrations
{
    public class AdministrationServiceDataSeederConsts
    {
        public const string ProviderKey = "admin";

        public static readonly List<string> ExcludedPermissions = new List<string>()
        {
            "AbpIdentity.Roles.Create",
            "AbpIdentity.Roles.Update",
            "AbpIdentity.Roles.Delete",
            "AbpIdentity.Roles.ManagePermissions",
            "AbpIdentity.Users.ManagePermissions"
        };
    }
}

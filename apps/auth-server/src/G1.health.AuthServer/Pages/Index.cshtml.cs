using System;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace G1.health.AuthServer.Pages;

public class IndexModel : AbpPageModel
{
    public readonly ConfigValues ConfigValues;

    public IndexModel(ConfigValues configValues)
    {
        ConfigValues = configValues;
    }

    public ActionResult OnGet()
    {
        if (Request.Query["ex"] == "yes")
        {
            throw new DivideByZeroException("This is a test exception!");
        }
    
        if (!CurrentUser.IsAuthenticated)
        {
            return Redirect("~/Account/Login");
        }
        else
        {
            string HostName = HttpContext.Request.Host.Host.ToString();
            string returnUrl = string.Format(ConfigValues.AngularAppUrl, HostName);

            return Redirect(returnUrl);
        }
    }
}

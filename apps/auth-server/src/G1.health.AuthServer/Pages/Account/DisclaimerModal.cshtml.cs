using G1.health.ClinicService.Localization;
using Microsoft.Extensions.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace G1.health.AuthServer.Pages.Account
{
    public class DisclaimerModalModel : AbpPageModel
    {
        public string DisclaimerContent { get; set; }

        public string Title { get; set; }

        public readonly IStringLocalizer<ClinicServiceResource> ClinicServiceLocalizer;

        public DisclaimerModalModel(IStringLocalizer<ClinicServiceResource> clinicServiceLocalizer)
        {
            ClinicServiceLocalizer = clinicServiceLocalizer;
        }

        public void OnGet()
        {
            Title = ClinicServiceLocalizer["Disclaimer"];
            DisclaimerContent = ProcessHtml(ClinicServiceLocalizer["DisclaimerContent"]);
        }

        private string ProcessHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return input.Replace("\\r\\n", "<br />").Replace("\\n", "<br />").Replace("\\r", "<br />");
        }
    }
}


using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Skarabeus_Api.Settings;
using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Utils.EmaillTemplates
{
    public class EmailHelper(IOptions<EnvironmentSettings> envOptions,UserManager<ApplicationUser> userManager)
    {
        /// <summary>
        /// Gets email confirm template from file system, and assign userName and confirmationLink
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="confirmationLink">Email confirmation link for user</param>
        /// <returns></returns>
        public async Task<string> GetEmailConfirmationTemplate(ApplicationUser user)
        {

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            var tokenEncoded = Uri.EscapeDataString(token);

            var confirmLink = $"{envOptions.Value.FrontendHostUrl}{envOptions.Value.FrontendConfirmUrl}/{user.Email}/{tokenEncoded}";

            var template = (await File.ReadAllTextAsync(".\\Utils\\EmaillTemplates\\EmailConfirmation.html"));
            template = template.Replace("[UserName]", user.UserName).Replace("[ConfirmationLink]", confirmLink);

            return template;
        }
    }
}

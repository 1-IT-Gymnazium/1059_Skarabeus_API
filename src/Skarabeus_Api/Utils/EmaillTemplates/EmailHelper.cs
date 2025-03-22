using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Skarabeus_Api.Settings;
using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Utils.EmaillTemplates
{
    public class EmailHelper(IOptions<EnvironmentSettings> envOptions, UserManager<ApplicationUser> userManager)
    {
        /// <summary>
        /// Gets email confirm template from file system, and assigns userName and confirmationLink
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Task with the Template</returns>
        public async Task<string> GetEmailConfirmationTemplate(ApplicationUser user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenEncoded = Uri.EscapeDataString(token);
            var confirmLink = $"{envOptions.Value.FrontendHostUrl}{envOptions.Value.FrontendConfirmUrl}?Email={user.Email}&Token={tokenEncoded}";

            var template = await File.ReadAllTextAsync(".\\Utils\\EmaillTemplates\\EmailConfirmation.html");
            template = template.Replace("[UserName]", user.UserName).Replace("[ConfirmationLink]", confirmLink);

            return template;
        }

        /// <summary>
        /// Gets account creation template from file system, and assigns userName and loginLink
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Task with the Template</returns>
        public async Task<string> GetAccountCreationTemplate(ApplicationUser user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var passwordToken = await userManager.GeneratePasswordResetTokenAsync(user);

            var tokenEncoded = Uri.EscapeDataString(token);
            var passwordTokenEncoded = Uri.EscapeDataString(passwordToken);
            var loginLink = $"{envOptions.Value.FrontendHostUrl}{envOptions.Value.FrontendPasswordResetUrl}?Email={user.Email}&Token={tokenEncoded}&PasswordToken={passwordTokenEncoded}";

            var template = await File.ReadAllTextAsync(".\\Utils\\EmaillTemplates\\AccountCreation.html");
            template = template.Replace("[UserName]", user.UserName).Replace("[Link]", loginLink);

            return template;
        }

        /// <summary>
        /// Gets password reset template from file system, and assigns userName and resetLink
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Task with the Template</returns>
        public async Task<string> GetPasswordResetTemplate(ApplicationUser user)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var tokenEncoded = Uri.EscapeDataString(token);
            var resetLink = $"{envOptions.Value.FrontendHostUrl}{envOptions.Value.FrontendPasswordResetUrl}?Email={user.Email}&PasswordToken={tokenEncoded}";

            var template = await File.ReadAllTextAsync(".\\Utils\\EmaillTemplates\\ChangePassword.html");
            template = template.Replace("[UserName]", user.UserName).Replace("[ResetLink]", resetLink);

            return template;
        }
    }
}

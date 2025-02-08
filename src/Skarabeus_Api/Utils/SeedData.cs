using Microsoft.AspNetCore.Identity;
using NodaTime;
using Skarabeus_Data.Entities;
using Skarabeus_Data.Interfaces;
using System.Security.Claims;

namespace Skarabeus_Api.Utils
{
    public class SeedData
    {
        public static async Task Initialize(
            UserManager<ApplicationUser> userManager
            )
        {
            if (userManager.Users.Any()) return;
            var newUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                LogginName = "user",
                Email = "user@example.com",
                UserName = "user@example.com",
            }.SetCreateBySystem(Instant.MinValue);

            await userManager.CreateAsync(newUser);
            await userManager.AddPasswordAsync(newUser, "String!123");

            var check = await userManager.ConfirmEmailAsync(newUser,await userManager.GenerateEmailConfirmationTokenAsync(newUser));

            await userManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Role, "Admin"));
        }
    }

}

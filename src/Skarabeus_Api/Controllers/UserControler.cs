using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using ProjectManager.Api.Services;
using Skarabeus_Api.Controllers.Models.Auth;
using Skarabeus_Api.Controllers.Models.PersonModels;
using Skarabeus_Api.Controllers.Models.UserModels;
using Skarabeus_Api.Utils;
using Skarabeus_Api.Utils.EmaillTemplates;
using Skarabeus_Data;
using Skarabeus_Data.Entities;
using Skarabeus_Data.Interfaces;
using System.Security.Claims;
using System.Text;

namespace Skarabeus_Api.Controllers;


[Route("api/v1/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly EmailSenderService _emailService;
    private readonly IClock _clock;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbcontext; ///skibidi

    public UserController(
        EmailSenderService emailService,
        IClock clock,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbcontext
        )
    {
        _emailService = emailService;
        _clock = clock;
        _userManager = userManager;
        _dbcontext = dbcontext;
    }

    [HttpGet]
    public async Task<ActionResult> GetList()
    {
        var users =
            (await _userManager.Users
            .Include(x => x.Person)
            .ToArrayAsync());

        var list = new List<UserInfoModel>();
        foreach (var user in users)
        {
            var roles = (await _userManager.GetClaimsAsync(user));
            var role = roles.FirstOrDefault(x => x.Type == ClaimTypes.Role);
            var us = user.ToModel();
            us.Role = role == null ? "" : role.Value;
            list.Add(us);
        }
        return Ok(list);
    }

    [Authorize(Policy = "UserManager")]
    [HttpPost("CreateUser")]
    public async Task<ActionResult> CreateUser(
       [FromBody] RegisterModel model,
       [FromServices] EmailHelper emailHelper
       )
    {
        var now = _clock.GetCurrentInstant();

        var newUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            LogginName = model.Name,
            Email = model.Email,
            UserName = model.Name,
        };

        if (model.PersonId != null)
        {
            var person = _dbcontext.Persons.FirstOrDefault(x => x.Id == model.PersonId);
            if (person == null)
            {
                ModelState.AddModelError<RegisterModel>(x => x.PersonId, "person not found");
            }
            else
            {
                newUser.Person = person;
            }
        }

        if (User.Identity != null && User.Identity.IsAuthenticated) newUser.SetCreateBy(User.GetName(), now);
        else newUser.SetCreateBySystem(now);

        await _userManager.CreateAsync(newUser);
        await _userManager.AddPasswordAsync(newUser, GenerateRandomPassword(12));

        await _emailService.AddEmailToSendAsync(newUser.Email, "Email confirm account creation", await emailHelper.GetAccountCreationTemplate(newUser));

        /*
        var token = string.Empty;
                
        token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

        await _emailService.AddEmailToSendAsync(
            model.Email,
            "Potvrzení registrace",
            $"<a href=\"localhost:5000/api/v1/Auth/ValidateToken?token={Uri.EscapeDataString(token)}&email={(model.Email)}\">{token}</a>"
            );
        */
        return Ok(ModelState);
    }

    [Authorize(Policy = "UserManager")]
    [HttpDelete("SoftDelete/{id}")]
    public async Task<ActionResult> SoftDeleteUser(Guid id)
    {
        var now = _clock.GetCurrentInstant();

        var user = await _dbcontext.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null) return NotFound();

        user.SetDeleteBy(User.GetName(), now);

        await _dbcontext.SaveChangesAsync();

        return Ok();
    }

    [Authorize(Policy = "UserManager")]
    [HttpGet("UndeleteUser/{id}")]
    public async Task<ActionResult> UndeleteUser(Guid id)
    {
        var now = _clock.GetCurrentInstant();

        var user = await _dbcontext.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null) return NotFound();

        user.SetModifyBy(User.GetName(), now);
        user.DeletedAt = null;

        await _dbcontext.SaveChangesAsync();

        return Ok();
    }

    [Authorize(Policy = "UserManager")]
    [HttpGet("AddRole")]
    public async Task<ActionResult> AddRole(
        Guid userId,
        string role
        )
    {
        var now = _clock.GetCurrentInstant();
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null) return NotFound();

        await RemoveRole(user);

        var res = await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, role));

        user.SetModifyBy(User.GetName(), now);

        await _dbcontext.SaveChangesAsync();

        return Ok(res);
    }

    [Authorize(Policy = "UserManager")]
    [HttpPost("AddPerson")]
    public async Task<ActionResult> AddPerson(
        Guid userId,
        Guid personId
        )
    {
        var now = _clock.GetCurrentInstant();
        var user = await _dbcontext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        var person = await _dbcontext.Persons.FirstOrDefaultAsync(x => x.Id == personId);

        if (user == null || person == null) return NotFound();

        user.Person = person;

        user.SetModifyBy(User.GetName(), now);

        await _dbcontext.SaveChangesAsync();
        return Ok(user.ToModel());
    }

    [Authorize(Policy = "UserManager")]
    [HttpGet("RemoveRole")]
    public async Task<ActionResult> RemoveUserRole(
        Guid userId
        )
    {
        var now = _clock.GetCurrentInstant();
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null) return NotFound();

        await RemoveRole(user);

        user.SetModifyBy(User.GetName(), now);

        await _dbcontext.SaveChangesAsync();

        return Ok();
    }


    private async Task RemoveRole(
        ApplicationUser usr
        )
    {
        foreach (var c in (await _userManager.GetClaimsAsync(usr)).Where(x => x.Type == ClaimTypes.Role)) await _userManager.RemoveClaimAsync(usr, c);
    }


    [Authorize]
    [HttpPatch("{id}")]
    public async Task<ActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<UserPatchModel> patch,
        [FromServices] EmailHelper emailHelper
        )
    {
        var roles = new string[] { "UserManager", "Admin" };
        if (id == User.GetUserId() || roles.Any(x => x == (User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value)))
        {
            var user = await _dbcontext.Users.Include(x => x.Person).FirstOrDefaultAsync(x => x.Id == id);


            if (user == null)
            {
                return NotFound();
            }

            var now = _clock.GetCurrentInstant();

            var toUpdate = new UserPatchModel
            {
                UserName = user.UserName,
                Email = user.Email,
            };
            patch.ApplyTo(toUpdate);

            if (!(ModelState.IsValid && TryValidateModel(toUpdate)))
            {
                return ValidationProblem(ModelState);
            }

            user.UserName = toUpdate.UserName;
            user.NormalizedUserName = toUpdate.UserName.ToUpperInvariant();
            if (user.Email != toUpdate.Email)
            {
                user.Email = toUpdate.Email;
                user.NormalizedEmail = toUpdate.Email.ToUpperInvariant();
                user.EmailConfirmed = false;
                await _emailService.AddEmailToSendAsync(user.Email, "confirm modified email", await emailHelper.GetEmailConfirmationTemplate(user));
            }
            else
            {
                user.Email = toUpdate.Email;
                user.NormalizedEmail = toUpdate.Email.ToUpperInvariant();
            }


            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                user.SetModifyBy(User.GetName(), now);
            }
            else
            {
                user.SetModifyBySystem(now);
            }

            await _dbcontext.SaveChangesAsync();
            return Ok();
        }
        return Unauthorized();
    }

    [HttpGet("changePassword/{email}")]
    public async Task<ActionResult> ChangePassword(
            [FromRoute] string email,
            [FromServices] EmailHelper emailHelper
        )
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user != null)
        {
            await _emailService.AddEmailToSendAsync(user.Email, "Reset password", await emailHelper.GetPasswordResetTemplate(user));
        }

        return NoContent();
    }

    /// <summary>
    /// generates random password that meets requirements and is of set length
    /// </summary>
    /// <param name="length">length of the password</param>
    /// <returns>generated password as a string</returns>
    private static string GenerateRandomPassword(int length)
    {

        const string upper = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digit = "0123456789";
        const string special = "!@$?_-";
        string allChars = upper + lower + digit + special;

        Random rand = new Random();
        StringBuilder password = new StringBuilder();

        // Ensure password meets requirements
        password.Append(upper[rand.Next(upper.Length)]);
        password.Append(lower[rand.Next(lower.Length)]);
        password.Append(digit[rand.Next(digit.Length)]);
        password.Append(special[rand.Next(special.Length)]);

        // Fill the rest of the password with random characters
        for (int i = 4; i < length; i++)
        {
            password.Append(allChars[rand.Next(allChars.Length)]);
        }

        // Shuffle the password to avoid predictable patterns
        return new string(password.ToString().OrderBy(x => rand.Next()).ToArray());
    }
}

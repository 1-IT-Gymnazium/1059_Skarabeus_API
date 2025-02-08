using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using ProjectManager.Api.Services;
using Skarabeus_Api.Controllers.Models.Auth;
using Skarabeus_Api.Controllers.Models.UserModels;
using Skarabeus_Api.Utils;
using Skarabeus_Data;
using Skarabeus_Data.Entities;
using Skarabeus_Data.Interfaces;
using System.Security.Claims;

namespace Skarabeus_Api.Controllers;


[Authorize(Policy = "UserManager")]
[Route("api/v1/[controller]")]
[ApiController]
public class UserControler : ControllerBase
{
    private readonly EmailSenderService _emailService;
    private readonly IClock _clock;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbcontext;

    public UserControler(
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
        var list = await _userManager.Users.Include(x => x.Person).Select(x => x.ToModel()).ToArrayAsync();

        return Ok(list);
    }

    [HttpPost("CreateUser")]
    public async Task<ActionResult> CreateUser(
       [FromBody] RegisterModel model
       )
    {
        var validator = new PasswordValidator<ApplicationUser>();
        var now = _clock.GetCurrentInstant();

        var newUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            LogginName = model.Name,
            Email = model.Email,
            UserName = model.Email,
            EmailConfirmed = true,
        };

        var checkPassword = await validator.ValidateAsync(_userManager, newUser, model.Password);

        if (!checkPassword.Succeeded)
        {
            ModelState.AddModelError<RegisterModel>(
                x => x.Password, string.Join("\n", checkPassword.Errors.Select(x => x.Description)));
            return ValidationProblem(ModelState);
        }

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
        await _userManager.AddPasswordAsync(newUser, model.Password);
        
        /*
        var token = string.Empty;
                
        token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

        await _emailService.AddEmailToSendAsync(
            model.Email,
            "Potvrzení registrace",
            $"<a href=\"localhost:5000/api/v1/Auth/ValidateToken?token={Uri.EscapeDataString(token)}&email={(model.Email)}\">{token}</a>"
            );
        */
        return Ok(/*new { Token = token, Modelstate = ModelState }*/);
    }

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

    [HttpPost("UndeleteUser/{id}")]
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

    [HttpPost("AddClaim")]
    public async Task<ActionResult> AddClaim(
        Guid userId,
        string claim
        )
    {
        var now = _clock.GetCurrentInstant();
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null) return NotFound();

        RemoveRole(user);

        var res = await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, claim));

        user.SetModifyBy(User.GetName(), now);

        await _dbcontext.SaveChangesAsync();

        return Ok(res);
    }

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

    [HttpPost("RemoveClaim")]
    public async Task<ActionResult> RemoveClaim(
        Guid userId,
        string claim
        )
    {
        var now = _clock.GetCurrentInstant();
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null) return NotFound();

        var res = await _userManager.RemoveClaimAsync(user, new Claim(ClaimTypes.Role, claim));

        user.SetModifyBy(User.GetName(), now);

        await _dbcontext.SaveChangesAsync();

        return Ok(res);
    }


    private async void RemoveRole(
        ApplicationUser usr
        )
    {
        foreach (var c in (await _userManager.GetClaimsAsync(usr)).Where(x => x.ValueType == ClaimTypes.Role)) await _userManager.RemoveClaimAsync(usr, c);
    }
}

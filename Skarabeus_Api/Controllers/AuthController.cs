
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using ProjectManager.Api.Services;
using Skarabeus_Api.Controllers.Models.Auth;
using Skarabeus_Data.Entities;
using Skarabeus_Data.Interfaces;
using System.Security.Claims;

namespace Skarabeus_Api.Controllers;
[ApiController]
public class AuthController : ControllerBase
{
    private readonly EmailSenderService _emailService;
    private readonly IClock _clock;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(
        EmailSenderService emailService,
        IClock clock,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager
        )
    {
        _emailService = emailService;
        _clock = clock;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("api/v1/Auth/Register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register(
       [FromBody] RegisterModel model
       )
    {
        var validator = new PasswordValidator<ApplicationUser>();
        var now = _clock.GetCurrentInstant();

        var newUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = model.Name,
            Email = model.Email,
            UserName = model.Email,
        }.SetCreateBySystem(now);

        var checkPassword = await validator.ValidateAsync(_userManager, newUser, model.Password);

        if (!checkPassword.Succeeded)
        {
            ModelState.AddModelError<RegisterModel>(
                x => x.Password, string.Join("\n", checkPassword.Errors.Select(x => x.Description)));
            return ValidationProblem(ModelState);
        }

        await _userManager.CreateAsync(newUser);
        await _userManager.AddPasswordAsync(newUser, model.Password);
        var token = string.Empty;
        token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

        await _emailService.AddEmailToSendAsync(
            model.Email,
            "Potvrzení registrace",
            $"<a href=\"https://www.projectmanagement.cz/?token={Uri.EscapeDataString(token)}&email={model.Email}\">{token}</a>"
            );

        return Ok(token);
    }

    [HttpPost("api/v1/Auth/Login")]
    public async Task<ActionResult> Login([FromBody] LoginModel model)
    {
        var normalizedEmail = model.Email.ToUpperInvariant();
        var user = await _userManager
            .Users
            .SingleOrDefaultAsync(x => x.EmailConfirmed && x.NormalizedEmail == normalizedEmail)
            ;

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "LOGIN_FAILED");
            return ValidationProblem(ModelState);
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "LOGIN_FAILED");
            return ValidationProblem(ModelState);
        }

        var userPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
        await HttpContext.SignInAsync(userPrincipal);

        return Ok();
    }

    /// <summary>
    /// unascape token before sending
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("api/v1/Auth/ValidateToken")]
    public async Task<ActionResult> ValidateToken(
        [FromBody] TokenModel model
        )
    {
        var normalizedMail = model.Email.ToUpperInvariant();
        var user = await _userManager
            .Users
            .SingleOrDefaultAsync(x => !x.EmailConfirmed && x.NormalizedEmail == normalizedMail);

        if (user == null)
        {
            ModelState.AddModelError<TokenModel>(x => x.Token, "INVALID_TOKEN");
            return ValidationProblem(ModelState);
        }

        var check = await _userManager.ConfirmEmailAsync(user, model.Token);
        if (!check.Succeeded)
        {
            ModelState.AddModelError<TokenModel>(x => x.Token, "INVALID_TOKEN");
            return ValidationProblem(ModelState);
        }

        return NoContent();
    }

    [HttpGet("api/v1/Auth/UserInfo")]
    public async Task<ActionResult<UserInfoModel>> UserInfo()
    {
        if (User.Identity == null || !User.Identity.IsAuthenticated)
        {
            throw new InvalidOperationException("user not logged in");
        }
        var idString = User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

        var usr = await _userManager
                .Users
                .SingleOrDefaultAsync(x => x.Id == Guid.Parse(idString))
                ;
        var usrModel = usr.ToModel();
        return Ok(usrModel);
    }

    [Authorize]
    [HttpPost("api/v1/Auth/Logout")]
    public async Task<ActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return NoContent();
    }

    [HttpGet("api/v1/Auth/TestMail")]
    public async Task<ActionResult> Test(
        [FromServices] EmailSenderService service
        )
    {
        await service.AddEmailToSendAsync("test@test.cz", "Suuuubject", "<h1>Aaaaaaaaaaa</h1>");
        return Ok();
    }
}

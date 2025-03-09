
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using ProjectManager.Api.Services;
using Skarabeus_Api.Controllers.Models.Auth;
using Skarabeus_Api.Controllers.Models.UserModels;
using Skarabeus_Api.Settings;
using Skarabeus_Api.Utils;
using Skarabeus_Data.Entities;
using Skarabeus_Data.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web.Helpers;

namespace Skarabeus_Api.Controllers;

[Route("api/v1/Auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IClock _clock;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        IClock clock,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtSettings> options
        )
    {
        _clock = clock;
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtSettings = options.Value;
    }

    [HttpPost("Login")]
    public async Task<ActionResult> Login(
        [FromBody] LoginModel model
        )
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if ((user == null) || !user.EmailConfirmed || user.DeletedAt != null)
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
        var claims = (await _userManager.GetClaimsAsync(user));

        //claims.Add(new Claim(ClaimTypes.Name, user.LogginName));
        //claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

        var userPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
        await HttpContext.SignInAsync(userPrincipal);

        //var token = GenerateJwtToken(claims.ToList());
        return Ok(/*new { Token = token }*/);
    }


    [HttpGet("ValidateToken")]
    public async Task<ActionResult> ValidateToken(
        [FromQuery] TokenModel model
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

    [Authorize]
    [HttpPost("Logout")]
    public async Task<ActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return NoContent();
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult> UserInfo()
    {
        var name = User.GetName();
        var user = (await _userManager.Users.Include(x=>x.Person).FirstOrDefaultAsync(x=>x.Email == name));
        if (user == null) return NoContent();
        var roles = (await _userManager.GetClaimsAsync(user));
        var role = roles.FirstOrDefault(x => x.Type == ClaimTypes.Role);
        var us = user.ToModel();
        us.Role = role == null ? "" : role.Value;
        return Ok(us);
    }

    [Authorize]
    [HttpGet("GetRole")]
    public async Task<ActionResult> GetRole()
    {
        var name = User.GetName();
        var model = (await _userManager.GetClaimsAsync((await _userManager.FindByEmailAsync(name)))).FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
        return Ok((model == null ? "none":model.Value).ToString());
    }

    private string GenerateJwtToken(List<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

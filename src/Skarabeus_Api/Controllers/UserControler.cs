using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using ProjectManager.Api.Services;
using Skarabeus_Api.Utils;
using Skarabeus_Data;
using Skarabeus_Data.Entities;
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
    private readonly ApplicationDbContext _dbContext;

    public UserControler(
        EmailSenderService emailService,
        IClock clock,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext
        )
    {
        _emailService = emailService;
        _clock = clock;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult> GetList()
    {
        var list = await _userManager.Users.Include(x => x.Person).ToArrayAsync();

        return Ok(list);
    }

    [HttpPost("AddClaim")]
    public async Task<ActionResult> AddClaim(
        Guid userId,
        string claim
        )
    {
        var usr = await _userManager.FindByIdAsync(userId.ToString());

        if (usr == null) return NotFound();

        RemoveRole(usr);

        var res = await _userManager.AddClaimAsync(usr, new Claim(ClaimTypes.Role, claim));

        return Ok(res);
    }

    [HttpPost("RemoveClaim")]
    public async Task<ActionResult> RemoveClaim(
        Guid userId,
        string claim
        )
    {
        var res = await _userManager.RemoveClaimAsync(await _userManager.FindByIdAsync(userId.ToString()), new Claim(ClaimTypes.Role, claim));

        return Ok(res);
    }


    private async void RemoveRole(
        ApplicationUser usr
        )
    {
        foreach (var c in (await _userManager.GetClaimsAsync(usr)).Where(x => x.ValueType == ClaimTypes.Role)) await _userManager.RemoveClaimAsync(usr, c);
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using ProjectManager.Api.Services;
using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers;

[ApiController]
public class UserControler
{
    private readonly EmailSenderService _emailService;
    private readonly IClock _clock;
    private readonly UserManager<ApplicationUser> _userManager;
    public UserControler(
        EmailSenderService emailService,
        IClock clock,
        UserManager<ApplicationUser> userManager
        )
    {
        _emailService = emailService;
        _clock = clock;
        _userManager = userManager;
    }
}

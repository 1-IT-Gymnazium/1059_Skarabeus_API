using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Skarabeus_Api.Controllers.Models.Auth;

public class TokenModel
{
    [Required]
    [FromQuery(Name = "email")]
    public string Email { get; set; } = null!;
    [Required]
    [FromQuery(Name = "token")]
    public string Token { get; set; } = null!;
}


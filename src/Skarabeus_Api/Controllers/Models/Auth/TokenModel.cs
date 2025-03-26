using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Skarabeus_Api.Controllers.Models.Auth;

public class TokenModel
{
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string Token { get; set; } = null!;
}


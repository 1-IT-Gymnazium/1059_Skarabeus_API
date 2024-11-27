using System.ComponentModel.DataAnnotations;

namespace Skarabeus_Api.Controllers.Models.Auth;

public class LoginModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}

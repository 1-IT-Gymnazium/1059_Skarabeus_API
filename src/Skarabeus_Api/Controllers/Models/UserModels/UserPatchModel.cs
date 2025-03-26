using Skarabeus_Api.Controllers.Models.PersonModels;

namespace Skarabeus_Api.Controllers.Models.UserModels;

public class UserPatchModel
{
    public string UserName { get; set; }
    public string Email { get; set; }
}

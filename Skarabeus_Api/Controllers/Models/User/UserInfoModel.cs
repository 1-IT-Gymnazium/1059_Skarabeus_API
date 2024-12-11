using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers.Models.Auth;
public class UserInfoModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public static class UserInfoModelExtensions
{
    public static UserInfoModel ToModel(this ApplicationUser model)
        => new()
        {
            Id = model.Id,
            Name = model.LogginName,
            Email = model.Email,
        };
}
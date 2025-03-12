using Skarabeus_Api.Controllers.Models.PersonModels;
using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers.Models.UserModels;
public class UserInfoModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public SmallPersonDetailModel Person { get; set; }
    public bool Deleted { get; set; }
    public string Role { get; set; }
    public bool EmailConfirmed { get; set; }
}

public static class UserInfoModelExtensions
{
    public static UserInfoModel ToModel(this ApplicationUser model)
        => new()
        {
            Id = model.Id,
            UserName = model.UserName,
            Email = model.Email,
            Person = model.Person == null ? null : model.Person.ToSmall(),
            Deleted = model.DeletedAt != null,
            EmailConfirmed = model.EmailConfirmed,
        };
}
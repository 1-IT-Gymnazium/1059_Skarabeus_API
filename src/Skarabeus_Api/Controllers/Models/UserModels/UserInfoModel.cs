using Skarabeus_Api.Controllers.Models.PersonModels;
using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers.Models.UserModels;
public class UserInfoModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public SmallPersonDetailModel Person { get; set; }
    public string PhoneNummber { get; set; }
}

public static class UserInfoModelExtensions
{
    public static UserInfoModel ToModel(this ApplicationUser model)
        => new()
        {
            Id = model.Id,
            UserName = model.UserName,
            Email = model.Email,
            Person = model.Person.ToSmall(),
            PhoneNummber = model.PhoneNumber,
        };
}
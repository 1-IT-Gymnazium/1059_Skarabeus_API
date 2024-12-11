using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers.Models.PersonModels
{
    public class PersonDetailModel: SmallPersonDetailModel
    {
        public string? EmailOfMother { get; set; }
        public string? EmailOfFather { get; set; }
        public string? PhoneNummberOfMother { get; set; }
        public string? PhoneNUmmberOfFather { get; set; }
        public string? FullNameOfMother { get; set; }
        public string? FullNameOfFather { get; set; }
    }

    public class SmallPersonDetailModel
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public bool Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? PhoneNummber { get; set; }
        public bool Active { get; set; }

    }



    public static class PersonDetailModelExtensions
    {
        public static PersonDetailModel ToDetail(this Person model)
            => new()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                EmailOfFather = model.EmailOfFather,
                EmailOfMother = model.EmailOfMother,
                Email = model.Email,
                PhoneNummber = model.PhoneNummber,
                PhoneNUmmberOfFather = model.PhoneNUmmberOfFather,
                PhoneNummberOfMother = model.PhoneNummberOfMother,
                FullNameOfFather = model.FullNameOfFather,
                FullNameOfMother = model.FullNameOfMother,
                Active = model.Active
            };
        public static SmallPersonDetailModel ToSmall(this Person model)
            => new()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Email = model.Email,
                PhoneNummber = model.PhoneNummber,
                Active = model.Active
            };

    }
}

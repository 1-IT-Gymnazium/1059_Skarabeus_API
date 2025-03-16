using Skarabeus_Api.Utils;
using Skarabeus_Data.Entities;
using System.Text.Json.Serialization;

namespace Skarabeus_Api.Controllers.Models.PersonModels
{
    public class PersonDetailModel: SmallPersonDetailModel
    {
        public string? EmailOfMother { get; set; }
        public string? EmailOfFather { get; set; }
        public string? PhoneNumberOfMother { get; set; }
        public string? PhoneNumberOfFather { get; set; }
        public string? FullNameOfMother { get; set; }
        public string? FullNameOfFather { get; set; }
    }

    public class SmallPersonDetailModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string Nickname { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public bool Gender { get; set; }
        public PersonStatus Status { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }

    }



    public static class PersonDetailModelExtensions
    {
        public static PersonDetailModel ToDetail(this Person model)
            => new()
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                EmailOfFather = model.EmailOfFather,
                EmailOfMother = model.EmailOfMother,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                PhoneNumberOfFather = model.PhoneNumberOfFather,
                PhoneNumberOfMother = model.PhoneNumberOfMother,
                FullNameOfFather = model.FullNameOfFather,
                FullNameOfMother = model.FullNameOfMother,
                Active = model.Active,
                Status = model.Status,
                Deleted = model.DeletedAt!=null,
                Nickname = model.Nickname
            };
        public static SmallPersonDetailModel ToSmall(this Person model)
            => new()
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Active = model.Active,
                Status = model.Status,
                Deleted = model.DeletedAt != null,
                Nickname = model.Nickname
            };

    }
}

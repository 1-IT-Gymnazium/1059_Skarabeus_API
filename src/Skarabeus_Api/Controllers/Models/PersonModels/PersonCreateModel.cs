using Skarabeus_Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Skarabeus_Api.Controllers.Models.PersonModels
{
    public class PersonCreateModel
    {
        public string FirstName { get; set; } = null!;
        public string? Nickname { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public bool Gender { get; set; }
        public string DateOfBirth { get; set; }
        public PersonStatus Status { get; set; }
        public string? EmailOfMother { get; set; }
        public string? EmailOfFather { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumberOfMother { get; set; }
        public string? PhoneNumberOfFather { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullNameOfMother { get; set; }
        public string? FullNameOfFather { get; set; }
        public bool Active { get; set; }
    }
}

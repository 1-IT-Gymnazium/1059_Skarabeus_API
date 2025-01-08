
namespace Skarabeus_Data.Entities;
public class Person : ITrackable
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? EmailOfMother { get; set; }
    public string? EmailOfFather { get; set; }
    public string? Email { get; set; }
    public string? PhoneNummberOfMother { get; set; }
    public string? PhoneNUmmberOfFather { get; set; }
    public string? PhoneNummber { get; set; }
    public string? FullNameOfMother { get; set; }
    public string? FullNameOfFather { get; set; }
    public bool Active { get; set; }

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; }
    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

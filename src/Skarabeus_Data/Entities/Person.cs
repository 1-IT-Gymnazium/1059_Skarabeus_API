
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
    public string? PhoneNumberOfMother { get; set; }
    public string? PhoneNumberOfFather { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FullNameOfMother { get; set; }
    public string? FullNameOfFather { get; set; }
    public bool Active { get; set; }
    public PersonStatus Status { get; set; }
    public ICollection<Event> Events { get; set; } = new HashSet<Event>();

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; }
    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public static class PersonExtensions
{
    public static IQueryable<Person> FilterActive(this IQueryable<Person> query)
        => query
        .Where(x => x.Active == true)
        ;
    public static IQueryable<Person> FilterDeleted(this IQueryable<Person> query)
        => query
        .Where(x => x.DeletedAt == null)
        ;

    public static IQueryable<Person> Filter(this IQueryable<Person> query)
        => query.FilterActive().FilterDeleted()
        ;
}

public enum PersonStatus
{
    other,
    child,
    instruktor,
    leader,
}
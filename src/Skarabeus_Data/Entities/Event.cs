
namespace Skarabeus_Data.Entities;
public class Event : ITrackable
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public Person? ResponsiblePerson { get; set; } = null!;
    public Guid? ResponsiblePersonId { get; set; }
    public ICollection<Person> Participants { get; set; } = new HashSet<Person>();
    public ICollection<Dish> Dishes { get; set; } = new HashSet<Dish>();
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Place { get; set; }


    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; }
    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

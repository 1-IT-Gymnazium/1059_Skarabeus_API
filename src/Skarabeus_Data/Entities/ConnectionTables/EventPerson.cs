namespace Skarabeus_Data.Entities.ConnectionTables;
public class EventPerson : ITrackable
{
    public Guid Id { get; set; }
    public Event Event { get; set; }
    public Person Person { get; set; }

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; }
    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

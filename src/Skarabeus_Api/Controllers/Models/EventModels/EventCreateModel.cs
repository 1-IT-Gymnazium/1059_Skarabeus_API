using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers.Models.EventModels;

public class EventCreateModel
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public Guid? ResponsiblePersonId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string? Place { get; set; }
}

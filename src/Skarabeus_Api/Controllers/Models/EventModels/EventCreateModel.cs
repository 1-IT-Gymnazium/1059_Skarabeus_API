using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers.Models.EventModels;

public class EventCreateModel
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public Guid? ResponsiblePersonId { get; set; }
    public string Start { get; set; }
    public string End { get; set; }
    public string? Place { get; set; }
}

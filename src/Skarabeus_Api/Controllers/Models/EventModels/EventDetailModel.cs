using Skarabeus_Api.Controllers.Models.PersonModels;
using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers.Models.EventModels
{
    public class EventDetailModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Place { get; set; }
        public SmallPersonDetailModel? ResponsiblePerson { get; set; }
        public ICollection<SmallPersonDetailModel> Participants { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }


    public static class PersonDetailModelExtensions
    {
        public static EventDetailModel ToDetail(this Event model)
            => new()
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Place = model.Place,
                ResponsiblePerson = model.ResponsiblePerson?.ToSmall(),
                Participants = model.Participants.Select(x=>x.ToSmall()).ToArray(),
                Start = model.Start,
                End = model.End
            };

    }
}

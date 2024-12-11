using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers.Models.DishModels
{
    public class DishDetailModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }


    public static class DishDetailModelExtensions
    {
        public static DishDetailModel ToDetail(this Dish model)
            => new()
            {
                Name = model.Name,
                Description = model.Description,
            };

    }
}

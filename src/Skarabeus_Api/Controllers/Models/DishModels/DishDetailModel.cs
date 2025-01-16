using Skarabeus_Api.Controllers.Models.IngredientModels;
using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers.Models.DishModels
{
    public class DishDetailModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<IngredientDishDetailModel> ingredients { get; set; }
    }


    public static class DishDetailModelExtensions
    {
        public static DishDetailModel ToDetail(this Dish model)
            => new()
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
            };

    }
}

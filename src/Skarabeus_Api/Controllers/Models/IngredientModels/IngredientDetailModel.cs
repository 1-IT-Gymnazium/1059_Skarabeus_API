using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers.Models.IngredientModels
{
    public class IngredientDetailModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal PriceForUnit { get; set; }
    }

    public class IngredientDishDetailModel : IngredientDetailModel
    {
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public Guid IngredientId { get; internal set; }
    }


    public static class IngredientDetailModelExtensions
    {
        public static IngredientDetailModel ToDetail(this Skarabeus_Data.Entities.Ingredient model)
            => new()
            {
                Id = model.Id,
                Name = model.Name,
                PriceForUnit = model.PriceForUnit,
            };

    }
}

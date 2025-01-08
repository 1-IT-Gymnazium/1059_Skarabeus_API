using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers.Models.IngredientModels
{
    public class IngredientDetailModel
    {
        public string Name { get; set; }
        public decimal PriceForUnit { get; set; }
    }


    public static class IngredientDetailModelExtensions
    {
        public static IngredientDetailModel ToDetail(this Skarabeus_Data.Entities.Ingredient model)
            => new()
            {
                Name = model.Name,
                PriceForUnit = model.PriceForUnit,
            };

    }
}

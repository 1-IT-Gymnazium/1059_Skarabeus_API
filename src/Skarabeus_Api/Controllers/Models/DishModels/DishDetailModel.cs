using Skarabeus_Api.Controllers.Models.IngredientModels;
using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers.Models.DishModels
{
    public class DishDetailModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get;set; }
        public ICollection<IngredientDishDetailModel> Ingredients { get; set; }
    }


    public static class DishDetailModelExtensions
    {
        public static DishDetailModel ToDetail(this Dish model, bool deep)
            => new()
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Ingredients = deep ? model.Ingredients.Select(y=> new IngredientDishDetailModel
                {
                    Id = y.Id,
                    IngredientId = y.IngredientId,
                    Name = y.Ingredient.Name,
                    PriceForUnit = y.Ingredient.PriceForUnit,
                    Amount = y.AmountInBaseUnits,
                    Price = y.AmountInBaseUnits * y.Ingredient.PriceForUnit,
                }).ToArray() : Array.Empty<IngredientDishDetailModel>(),
                Price = model.Ingredients.Select(x=>x.AmountInBaseUnits * x.Ingredient.PriceForUnit).Sum(),
            };

    }
}

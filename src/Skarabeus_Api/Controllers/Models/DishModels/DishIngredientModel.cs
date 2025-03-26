using Skarabeus_Data.Entities;

namespace Skarabeus_Api.Controllers.Models.DishModels;

public class DishIngredientModel
{
    public Guid DishId { get; set; }
    public Guid IngredientId { get; set; }
}

public class DishAddIngredientModel : DishIngredientModel
{
    public decimal Amount { get; set; }
}

public class DishAddIngredientsModel
{
    public Guid DishId { get; set; }
    public ICollection<IngredientAmount> Ingredients { get; set; }
}

public class IngredientAmount
{
    public Guid IngredientId { get; set; }
    public decimal Amount { get; set; }
}
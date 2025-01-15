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


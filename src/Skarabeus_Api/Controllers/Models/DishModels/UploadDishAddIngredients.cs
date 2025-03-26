namespace Skarabeus_Api.Controllers.Models.DishModels
{
    public class UploadDishAddIngredients
    {
        public string DishName { get; set; }
        public IEnumerable<UploadIngredientAmount> Ingredients { get; set; }
    }
    public class UploadIngredientAmount
    {
        public string IngredientName { get; set; }
        public decimal Amount { get; set; }
    }
}


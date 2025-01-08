namespace Skarabeus_Data.Entities.ConnectionTables;
public class IngredientDish : ITrackable
{
    public Guid Id { get; set; }
    public Ingredient Ingredient { get; set; }
    public Dish Dish { get; set; }
    public decimal AmountInBaseUnits { get; set; }


    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; }
    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

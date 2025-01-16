
namespace Skarabeus_Data.Entities;

public class Ingredient : ITrackable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal PriceForUnit { get; set; }


    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;
    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;
    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
public static class IngredientExtensions
{
    public static IQueryable<Ingredient> FilterDeleted(this IQueryable<Ingredient> query)
        => query
        .Where(x => x.DeletedAt == null)
        ;
}
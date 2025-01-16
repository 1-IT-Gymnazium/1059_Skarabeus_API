using Skarabeus_Data.Entities.ConnectionTables;

namespace Skarabeus_Data.Entities;
public class Dish : ITrackable
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ICollection<IngredientDish> Ingredients { get; set; } = new HashSet<IngredientDish>();
    public ICollection<Event> Events { get; set; } = new HashSet<Event>();

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; }
    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public static class DishExtensions
{
    public static IQueryable<Dish> FilterDeleted(this IQueryable<Dish> query)
        => query
        .Where(x => x.DeletedAt == null)
        ;
}
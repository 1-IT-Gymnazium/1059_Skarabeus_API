using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Api.Services;
using Skarabeus_Data.Entities;
using Skarabeus_Data.Entities.ConnectionTables;

namespace Skarabeus_Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Email> Emails { get; set; }
    public DbSet<Dish> Dishes { get; set; }
    public DbSet<Event> Evets { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<EventDish> EventDishes { get; set; }
    public DbSet<IngredientDish> IngredientDishes { get; set; }
    public DbSet<EventPerson> EventPersons { get; set; }

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<IdentityUserRole<Guid>>();
        modelBuilder.Ignore<IdentityRole<Guid>>();
        modelBuilder.Ignore<IdentityUserLogin<Guid>>();
        //modelBuilder.Ignore<IdentityUserClaim<Guid>>();
        modelBuilder.Ignore<IdentityUserToken<Guid>>();
        modelBuilder.Ignore<IdentityRoleClaim<Guid>>();

        var assemblyWithConfiguration = GetType().Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(assemblyWithConfiguration);
    }
}

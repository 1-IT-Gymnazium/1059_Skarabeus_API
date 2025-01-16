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
    public DbSet<Event> Events { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<IngredientDish> IngredientDishes { get; set; }

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
        modelBuilder.Entity<Event>()
        .HasMany(e => e.Participants)
        .WithMany(p => p.Events);
        modelBuilder.Entity<Event>()
        .HasOne(e => e.ResponsiblePerson)
        .WithMany()
        .HasForeignKey(e => e.ResponsiblePersonId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}

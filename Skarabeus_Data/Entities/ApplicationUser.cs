using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query;

namespace Skarabeus_Data.Entities;
[Table(name: "ApsNetUser")]
public class ApplicationUser : IdentityUser<Guid>, ITrackable
{
    public string? LogginName { get; set; }

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;

    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    private class Configuration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("AspNetUser");
            builder.HasData(SeedData());
        }
        private static IEnumerable<ApplicationUser> SeedData()
        {
            yield return new ApplicationUser()
            {
                Id = new Guid("CEAB6921-DFED-4B4D-B661-DC36B8749067"),
                UserName = "user@example.com",
                NormalizedUserName = "USER@EXAMPLE.COM",
                Email = "user@example.com",
                NormalizedEmail = "USER@EXAMPLE.COM",
                EmailConfirmed = true,
                // sTring123!
                PasswordHash = "AQAAAAEAACcQAAAAELKQmdGcfZbjxaz1GeqZ62mF7gEO9d49ofpdaQ+Mq0904MEIWvUnaMMfx9gJ27NmdQ==",
                SecurityStamp = "2MLDENGLJTQEITJVCJMIJJQOKXOUNSD6",
                ConcurrencyStamp = "ba46c7df-e2cf-469d-a17d-b653c50a0147",
                PhoneNumber = "123456798",
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnd = DateTimeOffset.MinValue,
                LockoutEnabled = true,
                AccessFailedCount = 0,
            }.SetCreateBySystem(Instant.MinValue);
        }
    }
}


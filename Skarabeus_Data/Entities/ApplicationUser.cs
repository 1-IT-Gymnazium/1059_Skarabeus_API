using Microsoft.AspNetCore.Identity;

namespace Skarabeus_Data.Entities;
[Table(name: "ApsNetUser")]
public class ApplicationUser : IdentityUser<Guid>, ITrackable
{
    public string? FullName { get; set; }

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;

    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

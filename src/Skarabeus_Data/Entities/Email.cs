using NodaTime;

namespace ProjectManager.Api.Services
{
    public class Email : ITrackable
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public string Receiver { get; set; }
        public string Sender { get; set; }
        public Instant ScheduledAt { get; set; }
        public Instant? SentAt { get; set; }
        public Instant CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public Instant ModifiedAt { get; set; }
        public string ModifiedBy { get; set; } = null!;
        public Instant? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.Data.Models.Entities
{
    public class AccountTokens : ICreatableEntity
    {
        public Guid AccountTokenId { get; set; }
        public Guid AccountId { get; set; }
        public Accounts Account { get; set; } = null!;

        public DateTime ExpiryTime { get; set; }
        public DateTime? RevokedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // ✅ SESSION MANAGEMENT FIELDS
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? DeviceInfo { get; set; }
        public DateTime? LastAccessedAt { get; set; }
        public string? SessionId { get; set; } // For tracking unique sessions
    }
}

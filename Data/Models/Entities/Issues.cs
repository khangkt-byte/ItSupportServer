using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.Data.Models.Entities
{
    public class Issues : BaseEntity<long>
    {
        public long IssId { get; set; }
        public override long Id => IssId;

        public required string Name { get; set; }

        public string? Description { get; set; }

        public string? Category { get; set; }

        /// <summary>
        /// Severity level: 1 (Low) to 5 (Critical)
        /// </summary>
        public int? Severity { get; set; }

        // Navigation properties
        public ICollection<Causes> Causes { get; set; } = [];
        public ICollection<IssueLogs> IssueLogs { get; set; } = [];
    }
}

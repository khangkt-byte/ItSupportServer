using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.Data.Models.Entities
{
    public class Causes : BaseEntity<long>
    {
        public long CauseId { get; set; }
        public override long Id => CauseId;

        public long IssId { get; set; }
        public Issues Issues { get; set; } = null!;

        public required string Name { get; set; }

        public string? Description { get; set; }

        public ICollection<IssueLogs> IssueLogs { get; set; } = [];
    }
}

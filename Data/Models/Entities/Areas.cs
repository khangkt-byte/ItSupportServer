using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.Data.Models.Entities
{
    public class Areas : BaseEntity<int>
    {
        public int AreaId { get; set; }
        public override int Id => AreaId;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ICollection<Employees> Employees { get; set; } = new List<Employees>();
        public ICollection<IssueLogs> IssueLogs { get; set; } = new List<IssueLogs>();
    }
}

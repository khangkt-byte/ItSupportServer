using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.Data.Models.Entities
{
    public class Departments : BaseEntity<int>
    {
        public int DptId { get; set; }
        public override int Id => DptId;

        required public string Name { get; set; }

        public string? Description { get; set; }

        public ICollection<Employees> Employees { get; set; } = new List<Employees>();
        public ICollection<IssueLogs> IssueLogs { get; set; } = new List<IssueLogs>();
    }
}

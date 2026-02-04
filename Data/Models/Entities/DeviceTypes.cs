using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.Data.Models.Entities
{
    public class DeviceTypes : BaseEntity<int>
    {
        public int DeviceTypeId { get; set; }
        public override int Id => DeviceTypeId;

        required public string Name { get; set; }

        public string? Description { get; set; }

        public ICollection<Devices> Devices { get; set; } = new List<Devices>();
    }
}

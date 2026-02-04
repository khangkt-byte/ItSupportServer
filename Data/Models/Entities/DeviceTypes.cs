using ItSupportServer.src.Shared.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models.Entities
{
    [Table("device_types")]
    public class DeviceTypes : BaseEntity<int>
    {
        [Key]
        [Column("device_type_id")]
        public int DeviceTypeId { get; set; }
        public override int Id => DeviceTypeId;

        [Column("name")]
        [Required, MaxLength(100)]
        required public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        public ICollection<Devices> Devices { get; set; } = new List<Devices>();
    }
}

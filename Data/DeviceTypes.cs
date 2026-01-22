using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data
{
    [Table("device_types")]
    public class DeviceTypes
    {
        [Key]
        [Column("device_type_id")]
        [Required]
        public string DeviceTypeId { get; set; }

        [Column("name")]
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        public ICollection<Devices> Devices { get; set; } = new List<Devices>();
    }
}

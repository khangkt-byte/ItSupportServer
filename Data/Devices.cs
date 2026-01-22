using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data
{
    [Table("devices")]
    public class Devices
    {
        [Key]
        [Column("device_id")]
        [Required]
        public string DeviceId { get; set; }

        [Column("device_type_id")]
        [Required]
        public string DeviceTypeId { get; set; }

        [ForeignKey(nameof(DeviceTypeId))]
        public DeviceTypes DeviceType { get; set; }

        [Column("name")]
        [Required, MaxLength(255)]
        public string Name { get; set; }

        [Column("brand")]
        [Required, MaxLength(64)]
        public string Brand { get; set; }

        [Column("model")]
        [Required, MaxLength(128)]
        public string Model { get; set; }

        [Column("serial_number")]
        [Required, MaxLength(64)]
        public string SerialNumber { get; set; }

        [Column("manufacturer")]
        [Required, MaxLength(150)]
        public string Manufacturer { get; set; }

        [Column("supplier")]
        [MaxLength(255)]
        public string? Supplier { get; set; }

        //[Column("purchase_date")]
        //public DateTime? PurchaseDate { get; set; }

        //[Column("warranty_expiration")]
        //public DateTime? WarrantyExpiration { get; set; }

        //[Column("status")]
        //[Required]
        //public string Status { get; set; }

        [Column("ip_address")]
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [Column("mac_address")]
        [MaxLength(17)]
        public string? MacAddress { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
}

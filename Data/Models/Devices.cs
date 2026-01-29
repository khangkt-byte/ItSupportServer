using ItSupportServer.src.Shared.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models
{
    [Table("devices")]
    public class Devices : BaseEntity<long>
    {
        [Key]
        [Column("device_id")]
        public long DeviceId { get; set; }
        public override long Id => DeviceId;

        [Column("device_type_id")]
        [Required]
        required public int DeviceTypeId { get; set; }

        [ForeignKey(nameof(DeviceTypeId))]
        public DeviceTypes DeviceType { get; set; }

        [Column("name")]
        [Required, MaxLength(255)]
        required public string Name { get; set; }

        [Column("brand")]
        [MaxLength(64)]
        public string? Brand { get; set; }

        [Column("model")]
        [Required, MaxLength(128)]
        required public string Model { get; set; }

        [Column("serial_number")]
        [MaxLength(64)]
        public string? SerialNumber { get; set; }

        //[Column("manufacturer")]
        //[Required, MaxLength(150)]
        //required public string Manufacturer { get; set; }

        //[Column("supplier")]
        //[MaxLength(255)]
        //public string? Supplier { get; set; }

        //[Column("purchase_date")]
        //public DateTime? PurchaseDate { get; set; }

        //[Column("warranty_expiration")]
        //public DateTime? WarrantyExpiration { get; set; }

        //[Column("ip_address")]
        //[MaxLength(45)]
        //public string? IpAddress { get; set; }

        //[Column("mac_address")]
        //[MaxLength(17)]
        //public string? MacAddress { get; set; }

        //[Column("status")]
        //[Required]
        //public string Status { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }
    }
}

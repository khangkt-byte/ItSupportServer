using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.Data.Models.Entities
{
    public class Devices : BaseEntity<long>
    {
        public long DeviceId { get; set; }
        public override long Id => DeviceId;

        required public int DeviceTypeId { get; set; }
        public DeviceTypes DeviceType { get; set; } = null!;

        required public string Name { get; set; }
        public string? Brand { get; set; }
        required public string Model { get; set; }
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

        public string? Notes { get; set; }
    }
}

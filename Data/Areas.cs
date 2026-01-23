using ItSupportServer.src.Shared.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data
{
    [Table("areas")]
    public class Areas : BaseEntity<int>
    {
        [Key]
        [Column("area_id")]
        [Required]
        public int AreaId { get; set; }
        public override int Id => AreaId;

        [Column("name")]
        [Required, MaxLength(255)]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        public ICollection<Employees> Employees { get; set; } = new List<Employees>();
        public ICollection<IssueLogs> IssueLogs { get; set; } = new List<IssueLogs>();
    }
}

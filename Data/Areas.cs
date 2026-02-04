using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data
{
    [Table("areas")]
    public class Areas
    {
        [Key]
        [Column("area_id")]
        [Required]
        public int AreaId { get; set; }

        [Column("name")]
        [Required]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        public ICollection<Employees> Employees { get; set; } = new List<Employees>();
        public ICollection<IssueLogs> IssueLogs { get; set; } = new List<IssueLogs>();
    }
}

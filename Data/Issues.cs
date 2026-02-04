using ItSupportServer.src.Shared.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data
{
    [Table("issues")]
    public class Issues : BaseEntity<long>
    {
        [Key]
        [Column("iss_id")]
        [Required]
        public long IssId { get; set; }
        public override long Id => IssId;

        [Column("name")]
        [Required]
        required public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        public ICollection<Causes> Causes { get; set; } = new List<Causes>();
    }
}

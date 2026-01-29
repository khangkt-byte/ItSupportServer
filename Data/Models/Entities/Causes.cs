using ItSupportServer.src.Shared.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models.Entities
{
    [Table("causes")]
    public class Causes : BaseEntity<long>
    {
        [Key]
        [Column("cause_id")]
        public long CauseId { get; set; }
        
        public override long Id => CauseId;

        [Column("iss_id")]
        [Required]
        public long IssId { get; set; }

        [ForeignKey(nameof(IssId))]
        public Issues Issues { get; set; } = null!;

        [Column("name")]
        [Required]
        [MaxLength(255)]
        public required string Name { get; set; }

        [Column("description")]
        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}

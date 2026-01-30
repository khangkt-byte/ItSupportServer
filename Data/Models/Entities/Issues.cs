using ItSupportServer.src.Shared.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models.Entities
{
    [Table("issues")]
    public class Issues : BaseEntity<long>
    {
        [Key]
        [Column("iss_id")]
        public long IssId { get; set; }
        
        public override long Id => IssId;

        [Column("name")]
        [Required]
        [MaxLength(255)]
        public required string Name { get; set; }

        [Column("description")]
        [MaxLength(1000)]
        public string? Description { get; set; }

        [Column("category")]
        [MaxLength(100)]
        public string? Category { get; set; }

        /// <summary>
        /// Severity level: 1 (Low) to 5 (Critical)
        /// </summary>
        [Column("severity")]
        public int? Severity { get; set; }

        // Navigation properties
        public ICollection<Causes> Causes { get; set; } = [];
        public ICollection<IssueLogs> IssueLogs { get; set; } = [];
    }
}

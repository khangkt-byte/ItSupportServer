using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data
{
    [Table("reasons")]
    public class Reasons
    {
        [Key]
        [Column("reason_id")]
        [Required]
        public string ReasonId { get; set; }

        [Column("name")]
        [Required]
        public string Name { get; set; }

        [Column("iss_id")]
        [Required]
        public string IssId { get; set; }

        [ForeignKey(nameof(IssId))]
        public Issues Issues { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
}

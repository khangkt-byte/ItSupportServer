using ItSupportServer.src.Shared.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data
{
    [Table("reasons")]
    public class Reasons : BaseEntity<long>
    {
        [Key]
        [Column("reason_id")]
        [Required]
        required public long ReasonId { get; set; }
        public override long Id => ReasonId;

        [Column("name")]
        [Required]
        required public string Name { get; set; }

        [Column("iss_id")]
        [Required]
        required public string IssId { get; set; }

        [ForeignKey(nameof(IssId))]
        required public Issues Issues { get; set; }
    }
}

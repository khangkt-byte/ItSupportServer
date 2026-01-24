using ItSupportServer.src.Shared.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models
{
    [Table("causes")]
    public class Causes : BaseEntity<long>
    {
        [Key]
        [Column("cause_id")]
        [Required]
        required public long CauseId { get; set; }
        public override long Id => CauseId;

        [Column("name")]
        [Required]
        required public string Name { get; set; }

        [Column("iss_id")]
        [Required]
        required public long IssId { get; set; }

        [ForeignKey(nameof(IssId))]
        required public Issues Issues { get; set; }
    }
}

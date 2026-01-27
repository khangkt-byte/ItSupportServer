using ItSupportServer.src.Shared.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models
{
    [Table("departments")]
    public class Departments : BaseEntity<int>
    {
        [Key]
        [Column("dpt_id")]
        [Required]
        required public int DptId { get; set; }
        public override int Id => DptId;

        [Column("name")]
        [Required, MaxLength(100)]
        required public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        public ICollection<Employees> Employees { get; set; } = new List<Employees>();
        //public ICollection<IssueLogs> IssueLogs { get; set; } = new List<IssueLogs>();
    }
}

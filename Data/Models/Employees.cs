using ItSupportServer.src.Shared.Base;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data.Models
{
    [Table("employees")]
    [Index(nameof(EmpCode), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class Employees : BaseEntity<Guid>
    {
        [Key]
        [Column("emp_id")]
        [Required]
        required public Guid EmpId { get; set; }
        public override Guid Id => EmpId;

        [Column("emp_code")]
        [MaxLength(20)]
        public string? EmpCode { get; set; }

        [Column("full_name")]
        [Required, MaxLength(150)]
        required public string FullName { get; set; }

        //[Column("birthday")]
        //public DateTime? Birthday { get; set; }

        //[Column("gender")]
        //public string? Gender { get; set; }

        [Column("phone_number")]
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [Column("email")]
        [MaxLength(254)]
        public string? Email { get; set; }

        [Column("dpt_id")]
        [Required]
        required public int DptId { get; set; }

        [ForeignKey(nameof(DptId))]
        public Departments Department { get; set; }

        [Column("area_id")]
        [Required]
        required public int AreaId { get; set; }

        [ForeignKey(nameof(AreaId))]
        public Areas Area { get; set; }

        [Column("position")]
        [MaxLength(150)]
        public string? Position { get; set; }

        //[Column("url_image")]
        //public string? UrlImage { get; set; }

        //[Column("hire_date")]
        //public DateTime? HireDate { get; set; }

        //[Column("status")]
        //public bool? Status { get; set; }

        public Accounts? Account { get; set; }
    }
}

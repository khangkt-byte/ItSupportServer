using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ItSupportServer.Data
{
    [Table("employees")]
    [Index(nameof(EmpCode), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class Employees
    {
        [Key]
        [Column("emp_id")]
        [Required]
        public string EmpId { get; set; }

        [Column("emp_code")]
        [Required, MaxLength(20)]
        public string EmpCode { get; set; }

        [Column("full_name")]
        [Required, MaxLength(150)]
        public string FullName { get; set; }

        [Column("birthday")]
        [Required]
        public DateTime Birthday { get; set; }

        //[Required]
        //[Column("gender")]
        //public string Gender { get; set; }

        [Column("phone_number")]
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [Column("email")]
        [Required, MaxLength(254)]
        public string Email { get; set; }

        [Column("dpt_id")]
        [Required]
        public string DptId { get; set; }

        [ForeignKey(nameof(DptId))]
        public Departments Department { get; set; }

        [Column("area_id")]
        [Required]
        public int AreaId { get; set; }

        [ForeignKey(nameof(AreaId))]
        public Areas Area { get; set; }

        //[Required]
        //[Column("position")]
        //public string Position { get; set; }

        //[Column("url_image")]
        //public string? UrlImage { get; set; }

        //[Column("hire_date")]
        //[Required]
        //public DateTime HireDate { get; set; }

        [Column("status")]
        [Required]
        public bool Status { get; set; }

        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        public ICollection<Accounts> Accounts { get; set; } = new List<Accounts>();
        public ICollection<IssueLogs> IssueLogsAsOperator { get; set; } = new List<IssueLogs>();
        public ICollection<IssueLogs> IssueLogsAsRequester { get; set; } = new List<IssueLogs>();
    }
}

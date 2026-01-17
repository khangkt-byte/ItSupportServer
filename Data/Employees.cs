using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITSupportServer.Data
{
    [Table("employees")]
    public class Employees
    {
        [Key]
        [Column("emp_id")]
        [Required]
        public string EmpId { get; set; }

        [Column("full_name")]
        [Required]
        public string FullName { get; set; }

        [Column("birthday")]
        [Required]
        public DateTime Birthday { get; set; }

        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("email")]
        [Required]
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

        //[Column("position")]
        //[Required]
        //public string Position { get; set; }

        //[Column("hire_date")]
        //[Required]
        //public DateTime HireDate { get; set; }

        //[Column("is_active")]
        //[Required]
        //public bool IsActive { get; set; }

        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
}

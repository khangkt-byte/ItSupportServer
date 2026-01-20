using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ItSupportServer.src.Shared.Helper;

namespace ItSupportServer.Data
{
    [Table("users")]
    [Index(nameof(EmployeeCode), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class Users : BaseEntity<Guid>
    {
        public Users() => Id = Guid.NewGuid();

        [Required]
        [Column("employee_code")]
        public string EmployeeCode { get; set; }

        [Required, MaxLength(200)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("email")]
        public string Email { get; set; }

        [MaxLength(15)]
        [Column("phone_number")]
        public string? PhoneNumber { get; set; }

        [Column("address")]
        public Dictionary<string, object>? Address { get; set; }

        [Column("birthday")]
        public DateTime? Birthday { get; set; }

        [Required]
        [Column("gender")]
        public string Gender { get; set; }

        [Required]
        [Column("position")]
        public string Position { get; set; }

        [Column("url_image")]
        public string? UrlImage { get; set; }

        [Required]
        [Column("status")]
        public bool Status { get; set; } = true;

        public Accounts? Account { get; set; }
    }
}
